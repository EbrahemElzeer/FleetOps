using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FleetOps.Order.Application.Orders.Commands.MarkAsReturned
{
    public sealed record MarkAsReturnedRequest(Guid DriverId);
}
