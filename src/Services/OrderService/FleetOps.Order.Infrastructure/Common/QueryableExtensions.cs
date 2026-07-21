using FleetOps.Order.Application.Common.Pagination;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FleetOps.Order.Infrastructure.Common
{
    public static class QueryableExtensions
    {
        public static async Task<PagedResponse<T>> ToPagedResponseAsync<T>(
            this IQueryable<T> query,
            PaginationRequest pagination,
            CancellationToken cancellationToken = default)
        {
            var totalCount = await query.CountAsync(cancellationToken);

            var items = await query
                .Skip(pagination.Skip)
                .Take(pagination.PageSize)
                .ToListAsync(cancellationToken);

            return new PagedResponse<T>(
                items,
                pagination.PageNumber,
                pagination.PageSize,
                totalCount);
        }
    }
}
