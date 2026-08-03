using FleetOps.Driver.Application.Abstractions;
using FleetOps.Driver.Domain.Common;
using FleetOps.Driver.Domain.Drivers;
using MediatR;

namespace FleetOps.Driver.Application.Drivers.Queries.CheckDriverEligibility
{
    public sealed class CheckDriverEligibilityQueryHandler: IRequestHandler<CheckDriverEligibilityQuery, Result<DriverEligibilityResponse>>
    {
        private readonly IDriverRepository _driverRepository;

        public CheckDriverEligibilityQueryHandler(IDriverRepository driverRepository)
        {
            _driverRepository = driverRepository;
        }

        public async Task<Result<DriverEligibilityResponse>> Handle(
            CheckDriverEligibilityQuery request,
            CancellationToken cancellationToken)
        {
            var driver = await _driverRepository.GetByIdAsync(
                request.DriverId,
                cancellationToken);

            if (driver is null)
                return DriverErrors.NotFound(request.DriverId);

            var eligibilityResult = driver.CanBeAssigned();

            if (eligibilityResult.IsFailure)
            {
                return new DriverEligibilityResponse(
                    driver.Id,
                    false,
                    driver.Status,
                    eligibilityResult.Errors[0].Description);
            }

            return new DriverEligibilityResponse(
                driver.Id,
                true,
                driver.Status,
                null);
        }
    }
}