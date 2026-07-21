using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FleetOps.Order.Application.Common.Pagination
{
    public class PaginationRequest
    {
        private const int MaxPageSize = 100;

        private int _pageNumber = 1;
        private int _pageSize = 10;

        public int PageNumber
        {
            get => _pageNumber;
            init => _pageNumber = value < 1 ? 1 : value;
        }

        public int PageSize
        {
            get => _pageSize;
            init => _pageSize = value < 1
                ? 10
                : value > MaxPageSize
                    ? MaxPageSize
                    : value;
        }

        public int Skip => (PageNumber - 1) * PageSize;
    }
}
