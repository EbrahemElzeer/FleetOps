using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FleetOps.Order.Application.Orders.Queries.GetOrders
{
    public sealed record OrderListItemResponse(
     Guid Id,
     string TrackingNumber,
     string CustomerName,
     string CustomerPhone,
     string PickupGovernorate,
     string PickupArea,
     string DeliveryGovernorate,
     string DeliveryArea,
     Guid? DriverId,
     string Status,
     DateTime CreatedAt
 );
}
