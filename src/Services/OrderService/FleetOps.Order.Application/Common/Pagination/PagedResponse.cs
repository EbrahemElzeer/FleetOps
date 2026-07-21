using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FleetOps.Order.Application.Common.Pagination
{
    public sealed class PagedResponse<T>
    {
        public IReadOnlyList<T> Items { get; }

        public int PageNumber { get; }

        public int PageSize { get; }

        public int TotalCount { get; }

        public int TotalPages { get; }

        public bool HasPreviousPage => PageNumber > 1;

        public bool HasNextPage => PageNumber < TotalPages;

        public PagedResponse(
            IReadOnlyList<T> items,
            int pageNumber,
            int pageSize,
            int totalCount)
         {
            Items = items;
            PageNumber = pageNumber;
            PageSize = pageSize;
            TotalCount = totalCount;
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
         }
    }
}
