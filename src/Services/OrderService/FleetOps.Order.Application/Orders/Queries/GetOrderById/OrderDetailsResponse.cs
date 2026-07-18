using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FleetOps.Order.Application.Orders.Queries.GetOrderById
{
    public sealed record OrderDetailsResponse(
    Guid Id,
    string TrackingNumber,
    string CustomerName,
    string CustomerPhone,
    OrderLocationResponse PickupLocation,
    OrderLocationResponse DeliveryLocation,
    Guid? DriverId,
    string Status,
    DateTime CreatedAt,
    DateTime? AssignedAt,
    DateTime? AcceptedAt,
    DateTime? PickedUpAt,
    DateTime? DeliveredAt,
    DateTime? CancelledAt,
    DateTime? DeliveryFailedAt,
    DateTime? ReturnStartedAt,
    DateTime? ReturnedAt,
    string? FailureReason,
    string? DeliveryFailureNotes,
    IReadOnlyCollection<OrderStatusHistoryResponse> StatusHistories);

}
