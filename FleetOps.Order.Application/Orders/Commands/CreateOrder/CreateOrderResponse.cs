using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FleetOps.Order.Application.Orders.Commands.CreateOrder
{
    public sealed record CreateOrderResponse(
     Guid Id,
     string TrackingNumber,
     string Status,
     DateTime CreatedAt
 );
}
