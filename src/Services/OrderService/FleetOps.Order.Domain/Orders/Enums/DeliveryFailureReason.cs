using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FleetOps.Order.Domain.Orders.Enums
{
    public enum DeliveryFailureReason
    {
        CustomerUnavailable = 1,
        CustomerRefused = 2,
        NoAnswer = 3,
        WrongAddress = 4,
        PaymentFailed = 5,
        LocationInaccessible = 6,
        ShipmentDamaged = 7,
        Other = 8
    }
}
