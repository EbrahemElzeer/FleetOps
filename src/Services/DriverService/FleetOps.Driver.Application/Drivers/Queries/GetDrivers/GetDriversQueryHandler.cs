using FleetOps.Driver.Application.Abstractions;
using FleetOps.Driver.Application.Common.Pagination;
using FleetOps.Driver.Domain.Common;
using MediatR;

namespace FleetOps.Driver.Application.Drivers.Queries.GetDrivers
{
    public sealed class GetDriversQueryHandler
        : IRequestHandler<GetDriversQuery, Result<PagedResponse<DriverListItemResponse>>>
    {
        private readonly IDriverRepository _driverRepository;

        public GetDriversQueryHandler(IDriverRepository driverRepository)
        {
            _driverRepository = driverRepository;
        }

        public async Task<Result<PagedResponse<DriverListItemResponse>>> Handle(
            GetDriversQuery request,
            CancellationToken cancellationToken)
        {
            var response = await _driverRepository.GetDriversAsync(
                request.Status,
                request,
                cancellationToken);

            return response;
        }
    }
}