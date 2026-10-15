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
        Task<List<OutboxMessage>> GetPendingAsync(int batchSize,
             CancellationToken cancellationToken = default);
    }
}
