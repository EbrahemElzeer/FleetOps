using FleetOps.Order.Domain.Common;
using MediatR;

namespace FleetOps.Order.Application.Orders.Commands.AcceptOrder
{ 
    public sealed record AcceptOrderCommand(Guid OrderId,Guid DriverId):IRequest<Result>;
    
}
