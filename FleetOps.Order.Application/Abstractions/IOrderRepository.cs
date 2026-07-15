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

    }
}
