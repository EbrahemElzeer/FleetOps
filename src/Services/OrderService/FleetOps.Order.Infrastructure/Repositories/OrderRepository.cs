using FleetOps.Order.Application.Abstractions;
using FleetOps.Order.Application.Common.Pagination;
using FleetOps.Order.Application.Orders.Queries.GetOrders;
using FleetOps.Order.Domain.Orders;
using FleetOps.Order.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FleetOps.Order.Infrastructure.Repositories
{

    public sealed class OrderRepository : IOrderRepository
    {
        private readonly OrderDbContext _context;

        public OrderRepository(OrderDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(Domain.Orders.Order order, CancellationToken ct = default)
        {
            await _context.Orders.AddAsync(order, ct);
        }

        public async Task<Domain.Orders.Order?> GetByIdAsync(Guid id, CancellationToken ct = default)
        {
            return await _context.Orders.Include(x => x.StatusHistories).FirstOrDefaultAsync(x => x.Id == id, ct);
        }

       

    public async    Task<PagedResponse<OrderListItemResponse>> GetFilteredPagedAsync(OrderStatus? status, string? pickupGovernorate, string? deliveryGovernorate, PaginationRequest pagination, CancellationToken cancellationToken = default)
        {
            var query = _context.Orders
                .AsNoTracking()
                .AsQueryable();

            if (status.HasValue)
            {
                query = query.Where(order =>
                    order.Status == status.Value);
            }

            if (!string.IsNullOrWhiteSpace(pickupGovernorate))
            {
                var governorate = pickupGovernorate.Trim();

                query = query.Where(order =>
                    order.PickupLocation.Governorate == governorate);
            }

            if (!string.IsNullOrWhiteSpace(deliveryGovernorate))
            {
                var governorate = deliveryGovernorate.Trim();

                query = query.Where(order =>
                    order.DeliveryLocation.Governorate == governorate);
            }

            var totalCount = await query.CountAsync(cancellationToken);

            var items = await query
                .OrderByDescending(order => order.CreatedAt)
                .Skip(pagination.Skip)
                .Take(pagination.PageSize)
                .Select(order => new OrderListItemResponse(
                    order.Id,
                    order.TrackingNumber.Value,
                    order.CustomerName,
                    order.CustomerPhone,
                    order.PickupLocation.Governorate,
                    order.PickupLocation.Area,
                    order.DeliveryLocation.Governorate,
                    order.DeliveryLocation.Area,
                    order.DriverId,
                    order.Status.ToString(),
                    order.CreatedAt))
                .ToListAsync(cancellationToken);

            return new PagedResponse<OrderListItemResponse>(
                items,
                pagination.PageNumber,
                pagination.PageSize,
                totalCount);
        }
    }
}
