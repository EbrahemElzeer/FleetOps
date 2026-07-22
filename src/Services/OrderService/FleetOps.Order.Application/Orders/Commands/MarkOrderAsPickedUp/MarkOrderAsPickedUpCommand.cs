using FleetOps.Order.Domain.Common;
using MediatR;

namespace FleetOps.Order.Application.Orders.Commands.MarkOrderAsPickedUp
{
    public sealed record MarkOrderAsPickedUpCommand(Guid OrderId,Guid DriverId ):IRequest<Result>;
}
