using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FleetOps.Order.Application.Orders.Queries.GetOrderById
{
    public sealed record OrderStatusHistoryResponse(
     Guid Id,
     string FromStatus,
     string ToStatus,
     string? Notes,
     DateTime ChangedAt
 );
}
