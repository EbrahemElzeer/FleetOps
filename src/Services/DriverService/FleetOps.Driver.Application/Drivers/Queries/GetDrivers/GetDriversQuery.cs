using FleetOps.Driver.Application.Common.Pagination;
using FleetOps.Driver.Domain.Common;
using FleetOps.Driver.Domain.Drivers.Enums;
using MediatR;

namespace FleetOps.Driver.Application.Drivers.Queries.GetDrivers
{
    public sealed class GetDriversQuery: PaginationRequest, IRequest<Result<PagedResponse<DriverListItemResponse>>>
    {
        public DriverStatus? Status { get; init; }
    }
}