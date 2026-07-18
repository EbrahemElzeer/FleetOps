using FleetOps.Order.Application.Abstractions;
using FleetOps.Order.Application.Common;
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
            var pickupLocation = OrderLocation.Create(
                  request.PickupLocation.Country,
                  request.PickupLocation.Governorate,
                  request.PickupLocation.City,
                  request.PickupLocation.Area,
                  request.PickupLocation.Street,
                  request.PickupLocation.BuildingNumber,
                  request.PickupLocation.Landmark,
                  request.PickupLocation.Latitude,
                  request.PickupLocation.Longitude);


            var deliveryLocation = OrderLocation.Create(
                  request.DeliveryLocation.Country,
                  request.DeliveryLocation.Governorate,
                  request.DeliveryLocation.City,
                  request.DeliveryLocation.Area,
                  request.DeliveryLocation.Street,
                  request.DeliveryLocation.BuildingNumber,
                  request.DeliveryLocation.Landmark,
                  request.DeliveryLocation.Latitude,
                  request.DeliveryLocation.Longitude);

            var order = new Order.Domain.Orders.Order(
                request.CustomerName,
                request.CustomerPhone,
                pickupLocation,
                deliveryLocation

                );
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
