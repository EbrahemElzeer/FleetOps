using FleetOps.Order.Domain.Common;
using MediatR;

namespace FleetOps.Order.Application.Orders.Commands.CancelOrder
{
    public sealed record CancelOrderCommand(Guid OrderId, string? Reason) : IRequest<Result>;

}
