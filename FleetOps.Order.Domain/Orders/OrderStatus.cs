using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FleetOps.Order.Domain.Orders
{
    public enum OrderStatus
    {
        Pending = 1,
        Assigned = 2,
        DriverAccepted = 3,
        PickedUp = 4,
        Delivered = 5,
        Cancelled = 6,
        DeliveryFailed = 7,
        ReturningToSender = 8,
        Returned = 9
    }
}
