using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FleetOps.Order.Application.Orders.Commands.AssignDriver
{
    public sealed record AssignDriverRequest(Guid DriverId);
    
}
