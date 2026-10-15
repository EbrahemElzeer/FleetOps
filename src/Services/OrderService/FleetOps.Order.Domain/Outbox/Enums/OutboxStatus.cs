using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FleetOps.Order.Domain.Outbox.Enums
{
    public enum OutboxStatus
    {
        Pending = 1,
        Processed = 2,
        Dead = 3
    }
}
