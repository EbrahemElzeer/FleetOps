using FleetOps.Driver.Application.Abstractions;
using FleetOps.Driver.Domain.Common;
using FleetOps.Driver.Domain.Drivers;
using MediatR;

namespace FleetOps.Driver.Application.Drivers.Commands.ReserveDriver
{
    public sealed class ReserveDriverCommandHandler: IRequestHandler<ReserveDriverCommand, Result>
    {
        private readonly IDriverRepository _driverRepository;
        private readonly IUnitOfWork _unitOfWork;

        public ReserveDriverCommandHandler(
            IDriverRepository driverRepository,
            IUnitOfWork unitOfWork)
        {
            _driverRepository = driverRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result> Handle(
            ReserveDriverCommand request,
            CancellationToken cancellationToken)
        {
            var driver = await _driverRepository.GetByIdAsync(
                request.DriverId,
                cancellationToken);

            if (driver is null)
                return DriverErrors.NotFound(request.DriverId);

            var result = driver.MarkAsAssigned();

            if (result.IsFailure)
                return result;

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success;
        }
    }
}