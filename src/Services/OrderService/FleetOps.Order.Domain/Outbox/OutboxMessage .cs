using FleetOps.Order.Domain.Outbox.Enums;
using FleetOps.Order.Domain.SeedWork;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FleetOps.Order.Domain.Outbox
{
    public sealed class OutboxMessage : Entity
    {
        private const int MaxRetryAttempts = 5;

        public string Type { get; private set; } = null!;

        public string Content { get; private set; } = null!;

        public DateTime OccurredAt { get; private set; }

        public DateTime? ProcessedAt { get; private set; }

        public int RetryCount { get; private set; }
        public DateTime? NextRetryAt { get; private set; }
        public string? Error { get; private set; }
        public OutboxStatus Status { get; private set; } = OutboxStatus.Pending;
        private OutboxMessage()
        {
        }

        public OutboxMessage(
            string type,
            string content,
            DateTime occurredAt)
        {
            Id = Guid.NewGuid();
            Type = type;
            Content = content;
            OccurredAt = occurredAt;
        }

        public void MarkProcessed()
        {
            Status = OutboxStatus.Processed;
            ProcessedAt = DateTime.UtcNow;
            NextRetryAt = null;
            Error = null;
        }

        public void MarkFailed(string error)
        {
            RetryCount++;
            Error = error;

            if (RetryCount >= MaxRetryAttempts)
            {
                Status = OutboxStatus.Dead;
                NextRetryAt = null;
                return;
            }

            var exponent = RetryCount - 1;

            var delaySeconds = Math.Min(
                5 * Math.Pow(2, exponent),
                300);

            NextRetryAt = DateTime.UtcNow
                .AddSeconds(delaySeconds);
        }
    }
}
