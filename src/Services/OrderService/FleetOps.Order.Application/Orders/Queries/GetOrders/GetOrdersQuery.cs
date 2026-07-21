using FleetOps.Order.Application.Common;
using FleetOps.Order.Application.Common.Pagination;
using FleetOps.Order.Domain.Orders;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FleetOps.Order.Application.Orders.Queries.GetOrders
{
    public sealed class GetOrdersQuery:PaginationRequest,IRequest<Result<PagedResponse<OrderListItemResponse>>>
    {
        public OrderStatus? Status { get; init; }

        public string? PickupGovernorate { get; init; }

        public string? DeliveryGovernorate { get; init; }
    }
}
