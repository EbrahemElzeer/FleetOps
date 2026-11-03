using FleetOps.Order.Application.Abstractions;
using FleetOps.Order.Domain.Outbox;
using FleetOps.Order.Domain.Outbox.Enums;
using FleetOps.Order.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FleetOps.Order.Infrastructure.Repositories
{
    public sealed class OutboxRepository : IOutboxRepository
    {
        private readonly OrderDbContext _context;

        public OutboxRepository(OrderDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(
            OutboxMessage message,
            CancellationToken cancellationToken = default)
        {
            await _context.OutboxMessages.AddAsync(
                message,
                cancellationToken);
        }



        public async Task<List<OutboxMessage>> ClaimPendingAsync(int batchSize,Guid workerId, CancellationToken cancellationToken = default)
        {
            if (batchSize <= 0)
                throw new ArgumentOutOfRangeException(nameof(batchSize));

            const int leaseSeconds = 120;

            const string sql = """
        ;WITH MessagesToClaim AS
        (
            SELECT TOP (@BatchSize) *
            FROM dbo.OutboxMessages WITH
                (UPDLOCK, READPAST, ROWLOCK, READCOMMITTEDLOCK)
            WHERE
                (
                    Status = @Pending
                    AND (
                        NextRetryAt IS NULL
                        OR NextRetryAt <= SYSUTCDATETIME()
                    )
                )
                OR
                (
                    Status = @Processing
                    AND LockedUntil < SYSUTCDATETIME()
                )
            ORDER BY OccurredAt, Id
        )
        UPDATE MessagesToClaim
        SET
            Status = @Processing,
            LockedBy = @WorkerId,
            LockedUntil = DATEADD(
                SECOND, @LeaseSeconds, SYSUTCDATETIME()
            ),LeaseToken = NEWID()
        OUTPUT INSERTED.Id,
        INSERTED.LeaseToken;
        """;

            var claimedMessages = new Dictionary<Guid, Guid>();

            await _context.Database.OpenConnectionAsync(cancellationToken);

            try
            {
                var connection = _context.Database.GetDbConnection();

                await using var command = connection.CreateCommand();
                command.CommandText = sql;

                void AddParameter(string name, object value)
                {
                    var parameter = command.CreateParameter();
                    parameter.ParameterName = name;
                    parameter.Value = value;
                    command.Parameters.Add(parameter);
                }

                AddParameter("@BatchSize", batchSize);
                AddParameter("@WorkerId", workerId);
                AddParameter("@LeaseSeconds", leaseSeconds);
                AddParameter("@Pending", (int)OutboxStatus.Pending);
                AddParameter("@Processing", (int)OutboxStatus.Processing);

                var claims = new Dictionary<Guid, Guid>();

                await using (var reader =
                    await command.ExecuteReaderAsync(cancellationToken))
                {
                    while (await reader.ReadAsync(cancellationToken))
                    {
                        var messageId = reader.GetGuid(0);
                        var leaseToken = reader.GetGuid(1);

                        claims.Add(messageId, leaseToken);
                    }
                }

                if (claims.Count == 0)
                    return new List<OutboxMessage>();

                var ids = claims.Keys.ToList();

                var messages = await _context.OutboxMessages
                    .AsNoTracking()
                    .Where(x =>
                        ids.Contains(x.Id) &&
                        x.LockedBy == workerId &&
                        x.Status == OutboxStatus.Processing)
                    .OrderBy(x => x.OccurredAt)
                    .ThenBy(x => x.Id)
                    .ToListAsync(cancellationToken);

                return messages
                    .Where(x =>
                        x.LeaseToken.HasValue &&
                        claims.TryGetValue(x.Id, out var expectedToken) &&
                        x.LeaseToken.Value == expectedToken)
                    .ToList();
            }
            finally
            {
                await _context.Database.CloseConnectionAsync();
            }
        }


        public async Task<bool> TryMarkProcessedAsync(Guid messageId,Guid workerId,Guid leaseToken,
            CancellationToken cancellationToken = default)
        {
            var affectedRows = await _context.OutboxMessages
                .Where(x =>
                    x.Id == messageId &&
                    x.Status == OutboxStatus.Processing &&
                    x.LockedBy == workerId &&
                    x.LeaseToken == leaseToken &&
                    x.LockedUntil > DateTime.UtcNow)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(
                        x => x.Status,
                        OutboxStatus.Processed)
                    .SetProperty(
                        x => x.ProcessedAt,
                        DateTime.UtcNow)
                    .SetProperty(
                        x => x.Error,
                        (string?)null)
                    .SetProperty(
                        x => x.NextRetryAt,
                        (DateTime?)null)
                    .SetProperty(
                        x => x.LockedBy,
                        (Guid?)null)
                    .SetProperty(
                        x => x.LockedUntil,
                        (DateTime?)null)
                    .SetProperty(
                        x => x.LeaseToken,
                        (Guid?)null),
                    cancellationToken);

            return affectedRows == 1;
        }


        public async Task<bool> TryMarkFailedAsync(Guid messageId,Guid workerId,Guid leaseToken,string error,
                  CancellationToken cancellationToken = default)
        {
            const int maxRetryAttempts = 5;

            var affectedRows = await _context.Database
                .ExecuteSqlInterpolatedAsync($"""
            UPDATE dbo.OutboxMessages
            SET
                RetryCount = RetryCount + 1,
                [Error] = {error},

                Status = CASE
                    WHEN RetryCount + 1 >= {maxRetryAttempts}
                        THEN {(int)OutboxStatus.Dead}
                    ELSE {(int)OutboxStatus.Pending}
                END,

                NextRetryAt = CASE
                    WHEN RetryCount + 1 >= {maxRetryAttempts}
                        THEN NULL
                    ELSE DATEADD(
                        SECOND,
                        CASE RetryCount
                            WHEN 0 THEN 5
                            WHEN 1 THEN 10
                            WHEN 2 THEN 20
                            WHEN 3 THEN 40
                            WHEN 4 THEN 80
                            WHEN 5 THEN 160
                            ELSE 300
                        END,
                        SYSUTCDATETIME()
                    )
                END,

                LockedBy = NULL,
                LockedUntil = NULL,
                LeaseToken = NULL

            WHERE Id = {messageId}
              AND Status = {(int)OutboxStatus.Processing}
              AND LockedBy = {workerId}
              AND LeaseToken = {leaseToken}
              AND LockedUntil > SYSUTCDATETIME();
            """, cancellationToken);

            return affectedRows == 1;
        }

        public async Task<bool> RenewLeaseAsync(Guid messageId,Guid workerId,Guid leaseToken,CancellationToken cancellationToken = default)
        {
            const int leaseSeconds = 120;

            var affectedRows = await _context.Database
                .ExecuteSqlInterpolatedAsync($"""
            UPDATE dbo.OutboxMessages
            SET LockedUntil = DATEADD(
                SECOND,
                {leaseSeconds},
                SYSUTCDATETIME()
            )
            WHERE Id = {messageId}
              AND Status = {(int)OutboxStatus.Processing}
              AND LockedBy = {workerId}
              AND LeaseToken = {leaseToken}
              AND LockedUntil > SYSUTCDATETIME();
            """, cancellationToken);

            return affectedRows == 1;
        }





    }
}
