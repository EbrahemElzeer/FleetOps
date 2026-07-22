using FleetOps.Order.Domain.Orders.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FleetOps.Order.Application.Orders.Commands.MarkDeliveryFailed
{
    public sealed record MarkDeliveryFailedRequest(Guid DriverId, DeliveryFailureReason FailureReason, string? Notes);
}
