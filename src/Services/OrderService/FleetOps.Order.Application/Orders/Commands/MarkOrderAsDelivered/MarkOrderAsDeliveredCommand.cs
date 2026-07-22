using FleetOps.Order.Domain.Common;
using MediatR;

namespace FleetOps.Order.Application.Orders.Commands.MarkOrderAsDelivered
{
    public sealed record MarkOrderAsDeliveredCommand(Guid OrderId, Guid DriverId) : IRequest<Result>;
}
