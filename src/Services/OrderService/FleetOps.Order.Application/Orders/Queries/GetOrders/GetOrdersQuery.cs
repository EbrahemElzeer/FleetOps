using FleetOps.Order.Domain.Common;
using FleetOps.Order.Application.Common.Pagination;
using MediatR;
using FleetOps.Order.Domain.Orders.Enums;

namespace FleetOps.Order.Application.Orders.Queries.GetOrders
{
    public sealed class GetOrdersQuery:PaginationRequest,IRequest<Result<PagedResponse<OrderListItemResponse>>>
    {
        public OrderStatus? Status { get; init; }

        public string? PickupGovernorate { get; init; }

        public string? DeliveryGovernorate { get; init; }
    }
}
