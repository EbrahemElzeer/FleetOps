using FleetOps.Order.Application.Abstractions;
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
    }
}
