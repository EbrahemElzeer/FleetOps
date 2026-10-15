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


        public async Task<List<OutboxMessage>> GetPendingAsync(
                 int batchSize,
                 CancellationToken cancellationToken = default)
        {
            var now = DateTime.UtcNow;

            return await _context.OutboxMessages
                .Where(x =>
                    x.Status == OutboxStatus.Pending &&
                    x.ProcessedAt == null &&
                    (x.NextRetryAt == null ||
                     x.NextRetryAt <= now))
                .OrderBy(x => x.OccurredAt)
                .ThenBy(x => x.Id)
                .Take(batchSize)
                .ToListAsync(cancellationToken);
        }
    }
}
