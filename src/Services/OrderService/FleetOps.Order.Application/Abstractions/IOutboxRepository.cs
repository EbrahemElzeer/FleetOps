using FleetOps.Order.Domain.Outbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FleetOps.Order.Application.Abstractions
{
    public interface IOutboxRepository
    {
        Task AddAsync(
            OutboxMessage message,
            CancellationToken cancellationToken = default);

        Task<List<OutboxMessage>> ClaimPendingAsync(int batchSize, Guid workerId,
            CancellationToken cancellationToken = default);

        Task<bool> TryMarkProcessedAsync(Guid messageId,Guid workerId, Guid leaseToken,
            CancellationToken cancellationToken = default);
        Task<bool> TryMarkFailedAsync(Guid messageId, Guid workerId, Guid leaseToken, string error,CancellationToken cancellationToken = default);
        Task<bool> RenewLeaseAsync(Guid messageId, Guid workerId, Guid leaseToken, CancellationToken cancellationToken = default);
    }
}
