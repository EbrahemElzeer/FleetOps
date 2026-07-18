using FleetOps.Order.Application.Abstractions;
using FleetOps.Order.Application.Common;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FleetOps.Order.Application.Orders.Queries.GetOrderById
{
    public sealed class GetOrderByIdQueryHandler : IRequestHandler<GetOrderByIdQuery,Result<OrderDetailsResponse>>
    {
        private readonly IOrderRepository _orderRepository;

        public GetOrderByIdQueryHandler(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }
        public async Task<Result<OrderDetailsResponse?>> Handle(GetOrderByIdQuery request, CancellationToken ct)
        {
            var order = await _orderRepository.GetByIdAsync(request.Id, ct);

            if (order is null)

            {
              return  Error.NotFound(
                    code: "Order.NotFound",
                    description: $"The order with ID '{request.Id}' was not found.");
            }

            return new OrderDetailsResponse(
            order.Id,
            order.TrackingNumber.Value,
            order.CustomerName,
            order.CustomerPhone,
            new OrderLocationResponse(
                order.PickupLocation.Country,
                order.PickupLocation.Governorate,
                order.PickupLocation.City,
                order.PickupLocation.Area,
                order.PickupLocation.Street,
                order.PickupLocation.BuildingNumber,
                order.PickupLocation.Landmark,
                order.PickupLocation.Latitude,
                order.PickupLocation.Longitude,
                order.PickupLocation.GetFormattedAddress()
            ),
            new OrderLocationResponse(
                order.DeliveryLocation.Country,
                order.DeliveryLocation.Governorate,
                order.DeliveryLocation.City,
                order.DeliveryLocation.Area,
                order.DeliveryLocation.Street,
                order.DeliveryLocation.BuildingNumber,
                order.DeliveryLocation.Landmark,
                order.DeliveryLocation.Latitude,
                order.DeliveryLocation.Longitude,
                order.DeliveryLocation.GetFormattedAddress()
            ),
            order.DriverId,
            order.Status.ToString(),
            order.CreatedAt,
            order.AssignedAt,
            order.AcceptedAt,
            order.PickedUpAt,
            order.DeliveredAt,
            order.CancelledAt,
            order.DeliveryFailedAt,
            order.ReturnStartedAt,
            order.ReturnedAt,
            order.FailureReason?.ToString(),
            order.DeliveryFailureNotes,
            order.StatusHistories
                .OrderBy(x => x.ChangedAt)
                .Select(x => new OrderStatusHistoryResponse(
                    x.Id,
                    x.FromStatus.ToString(),
                    x.ToStatus.ToString(),
                    x.Notes,
                    x.ChangedAt
                ))
                .ToList()
        );
        }
    }

}
