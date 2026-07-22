using FleetOps.Order.Domain.Common;
using MediatR;

namespace FleetOps.Order.Application.Orders.Commands.MarkAsReturned
{
    public sealed record MarkAsReturnedCommand(Guid OrderId,Guid DriverId) : IRequest<Result>;
}
