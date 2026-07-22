using FleetOps.Order.Domain.Common;
using MediatR;

namespace FleetOps.Order.Application.Orders.Commands.StartReturnToSender
{
    public sealed record StartReturnToSenderCommand(Guid OrderId) : IRequest<Result>;
}
