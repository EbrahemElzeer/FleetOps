using FleetOps.Order.Domain.Common;
using MediatR;

namespace FleetOps.Order.Application.Orders.Commands.AssignDriver
{
    public sealed record AssignDriverCommand(Guid OrderId,Guid DriverId):IRequest<Result>;
   
}
