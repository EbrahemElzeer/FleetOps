using FleetOps.Order.Domain.Outbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FleetOps.Order.Application.Abstractions.IntegrationEvents
{
    public interface IIntegrationEventPublisher
    {
        Task<IReadOnlyList<PublishResult>> PublishBatchAsync(
            IReadOnlyList<OutboxMessage> messages,
            CancellationToken cancellationToken = default);
    }

    public sealed record PublishResult(
    OutboxMessage Message,
    bool IsConfirmed,
    string? Error = null);
}
