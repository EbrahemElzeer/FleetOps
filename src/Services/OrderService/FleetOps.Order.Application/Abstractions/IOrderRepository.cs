using FleetOps.Order.Application.Common.Pagination;
using FleetOps.Order.Application.Orders.Queries.GetOrders;
using FleetOps.Order.Domain.Orders.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FleetOps.Order.Application.Abstractions
{
    public interface IOrderRepository
    {
        Task AddAsync(Domain.Orders.Order order, CancellationToken ct = default);
        Task<Domain.Orders.Order?> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task<PagedResponse<OrderListItemResponse>> GetFilteredPagedAsync(
    OrderStatus? status,
    string? pickupGovernorate,
    string? deliveryGovernorate,
    PaginationRequest pagination,
    CancellationToken cancellationToken = default);
    }
}
