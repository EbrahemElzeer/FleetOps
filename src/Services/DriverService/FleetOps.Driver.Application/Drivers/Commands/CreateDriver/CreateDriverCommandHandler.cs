using FleetOps.Driver.Application.Abstractions;
using FleetOps.Driver.Domain.Common;
using DriverAggregate = FleetOps.Driver.Domain.Drivers.Driver;
using MediatR;

namespace FleetOps.Driver.Application.Drivers.Commands.CreateDriver
{
    public sealed class CreateDriverCommandHandler
        : IRequestHandler<CreateDriverCommand, Result<CreateDriverResponse>>
    {
        private readonly IDriverRepository _driverRepository;
        private readonly IUnitOfWork _unitOfWork;

        public CreateDriverCommandHandler(IDriverRepository driverRepository,IUnitOfWork unitOfWork)           
        {
            _driverRepository = driverRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<CreateDriverResponse>> Handle(CreateDriverCommand request,CancellationToken cancellationToken)
          
        {
            var driverResult = DriverAggregate.Create(
                request.Dto.FullName,
                request.Dto.PhoneNumber,
                request.Dto.VehicleType,
                request.Dto.VehiclePlateNumber);

            if (driverResult.IsFailure)
                return Result<CreateDriverResponse>.Failure(driverResult.Errors);

            var driver = driverResult.Value;

            await _driverRepository.AddAsync(driver, cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new CreateDriverResponse(
                driver.Id,
                driver.FullName,
                driver.PhoneNumber,
                driver.VehicleType,
                driver.VehiclePlateNumber,
                driver.Status,
                driver.CreatedAt);
        }
    }
}