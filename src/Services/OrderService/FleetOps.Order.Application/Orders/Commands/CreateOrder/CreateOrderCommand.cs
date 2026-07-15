using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FleetOps.Order.Application.Orders.Commands.CreateOrder
{
    public sealed record CreateOrderCommand(

          string CustomerName,
          string CustomerPhone,
          CreateOrderLocationDto PickupLocation,
          CreateOrderLocationDto DeliveryLocation

     ):IRequest<CreateOrderResponse>;
   
}
