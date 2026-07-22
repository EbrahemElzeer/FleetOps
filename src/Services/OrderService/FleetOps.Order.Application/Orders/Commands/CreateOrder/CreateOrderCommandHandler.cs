using FleetOps.Order.Application.Abstractions;
using FleetOps.Order.Application.Common;
using FleetOps.Order.Domain.Common;
using FleetOps.Order.Domain.Orders;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FleetOps.Order.Application.Orders.Commands.CreateOrder
{
    public class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, Result<CreateOrderResponse>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IOrderRepository _orderRepository;

        public CreateOrderCommandHandler(IUnitOfWork unitOfWork,IOrderRepository orderRepository)
        {
            _unitOfWork = unitOfWork;
            _orderRepository = orderRepository;
        }
        public async Task<Result<CreateOrderResponse>> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
        {
            var pickupLocationResult = OrderLocation.Create(
                  request.PickupLocation.Country,
                  request.PickupLocation.Governorate,
                  request.PickupLocation.City,
                  request.PickupLocation.Area,
                  request.PickupLocation.Street,
                  request.PickupLocation.BuildingNumber,
                  request.PickupLocation.Landmark,
                  request.PickupLocation.Latitude,
                  request.PickupLocation.Longitude);

            if (pickupLocationResult.IsFailure)
                return Result<CreateOrderResponse>.Failure(pickupLocationResult.Errors);
         
            var deliveryLocationResult = OrderLocation.Create(
                  request.DeliveryLocation.Country,
                  request.DeliveryLocation.Governorate,
                  request.DeliveryLocation.City,
                  request.DeliveryLocation.Area,
                  request.DeliveryLocation.Street,
                  request.DeliveryLocation.BuildingNumber,
                  request.DeliveryLocation.Landmark,
                  request.DeliveryLocation.Latitude,
                  request.DeliveryLocation.Longitude);

            if (deliveryLocationResult.IsFailure)
                return Result<CreateOrderResponse>.Failure(deliveryLocationResult.Errors);
            var orderResult =  Order.Domain.Orders.Order.Create(
                request.CustomerName,
                request.CustomerPhone,
                pickupLocationResult.Value,
                deliveryLocationResult.Value);

            if (orderResult.IsFailure)
                return Result<CreateOrderResponse>.Failure(orderResult.Errors);
          
            var order = orderResult.Value;
            await _orderRepository.AddAsync(order, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return new CreateOrderResponse(
             order.Id,
             order.TrackingNumber.Value,
             order.Status.ToString(),
             order.CreatedAt);
        }
    }
}
