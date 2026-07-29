using FleetOps.Driver.Application.Abstractions;
using FleetOps.Driver.Domain.Common;
using FleetOps.Driver.Domain.Drivers;
using MediatR;

namespace FleetOps.Driver.Application.Drivers.Queries.GetDriverById
{
    public sealed class GetDriverByIdQueryHandler
        : IRequestHandler<GetDriverByIdQuery, Result<DriverDetailsResponse>>
    {
        private readonly IDriverRepository _driverRepository;

        public GetDriverByIdQueryHandler(IDriverRepository driverRepository)
        {
            _driverRepository = driverRepository;
        }

        public async Task<Result<DriverDetailsResponse>> Handle(GetDriverByIdQuery request,CancellationToken cancellationToken)
        {
            var driver = await _driverRepository.GetByIdAsync(
                request.DriverId,
                cancellationToken);

            if (driver is null)
                return DriverErrors.NotFound(request.DriverId);

            return new DriverDetailsResponse(
                driver.Id,
                driver.FullName,
                driver.PhoneNumber,
                driver.VehicleType,
                driver.VehiclePlateNumber,
                driver.Status,
                driver.CreatedAt,
                driver.WentOnlineAt,
                driver.WentOfflineAt,
                driver.SuspendedAt,
                driver.AssignedAt,
                driver.TripStartedAt,
                driver.ReturningStartedAt,
                driver.SuspensionReason,
                driver.SuspensionNotes);
        }
    }
}