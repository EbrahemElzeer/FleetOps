using FleetOps.Order.Domain.Common;
using MediatR;

namespace FleetOps.Order.Application.Orders.Commands.CreateOrder
{
    public sealed record CreateOrderCommand(

          string CustomerName,
          string CustomerPhone,
          CreateOrderLocationDto PickupLocation,
          CreateOrderLocationDto DeliveryLocation

     ):IRequest <Result<CreateOrderResponse>>;
   
}
