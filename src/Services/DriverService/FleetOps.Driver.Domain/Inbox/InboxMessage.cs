using FleetOps.Driver.Domain.SeedWork;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FleetOps.Driver.Domain.Inbox
{
    public sealed class InboxMessage
    {
        public InboxMessage(Guid messageId)
        {
            MessageId = messageId;
            ProcessedAtUtc = DateTime.UtcNow;
        }
        public Guid MessageId { get; private set; }

        public DateTime ProcessedAtUtc { get; private set; }
    }
}
