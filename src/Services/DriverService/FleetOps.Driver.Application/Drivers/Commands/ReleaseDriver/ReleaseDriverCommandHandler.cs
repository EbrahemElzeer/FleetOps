using FleetOps.Driver.Application.Abstractions;
using FleetOps.Driver.Domain.Common;
using FleetOps.Driver.Domain.Drivers;
using MediatR;

namespace FleetOps.Driver.Application.Drivers.Commands.ReleaseDriver
{
    public sealed class ReleaseDriverCommandHandler
        : IRequestHandler<ReleaseDriverCommand, Result>
    {
        private readonly IDriverRepository _driverRepository;
        private readonly IUnitOfWork _unitOfWork;

        public ReleaseDriverCommandHandler(
            IDriverRepository driverRepository,
            IUnitOfWork unitOfWork)
        {
            _driverRepository = driverRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result> Handle(ReleaseDriverCommand request, CancellationToken cancellationToken)
        {
            var driver = await _driverRepository.GetByIdAsync(
                request.DriverId,
                cancellationToken);

            if (driver is null)
                return DriverErrors.NotFound(request.DriverId);

            var result = driver.MarkAsAvailable();

            if (result.IsFailure)
                return result;

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success;
        }
    }
}