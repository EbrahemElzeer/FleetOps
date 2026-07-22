using FleetOps.Order.Domain.Common;
using FleetOps.Order.Domain.Orders.Enums;
using MediatR;

namespace FleetOps.Order.Application.Orders.Commands.MarkDeliveryFailed
{
    public sealed record MarkDeliveryFailedCommand(Guid OrderId,Guid DriverId, DeliveryFailureReason FailureReason, string? Notes) :IRequest<Result>;
    
}
