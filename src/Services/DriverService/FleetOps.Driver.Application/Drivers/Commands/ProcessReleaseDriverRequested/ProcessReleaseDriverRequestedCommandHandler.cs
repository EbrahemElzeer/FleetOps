using FleetOps.Driver.Application.Abstractions;
using FleetOps.Driver.Domain.Common;
using FleetOps.Driver.Domain.Drivers;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FleetOps.Driver.Application.Drivers.Commands.ProcessReleaseDriverRequested
{
    public sealed class ProcessReleaseDriverRequestedCommandHandler : IRequestHandler<ProcessReleaseDriverRequestedCommand, Result>
    {
        private readonly IInboxRepository _inboxRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IDriverRepository _driverRepository;

        public ProcessReleaseDriverRequestedCommandHandler(IInboxRepository inboxRepository,IUnitOfWork unitOfWork,IDriverRepository driverRepository)
        {
            _inboxRepository = inboxRepository;
            _unitOfWork = unitOfWork;
            _driverRepository = driverRepository;
        }

        public async Task<Result> Handle(ProcessReleaseDriverRequestedCommand request, CancellationToken cancellationToken)
        {
            var alreadyProcessed = await _inboxRepository.ExistsAsync(request.MessageId,cancellationToken);
            if (alreadyProcessed) return Result.Success;

         var driver=   await _driverRepository.GetByIdAsync(request.DriverId, cancellationToken);
            if (driver is null) return DriverErrors.NotFound(request.DriverId);

            var result = driver.ReleaseReservation(request.OrderId);
            if (result.IsFailure)
                return result;
            await _inboxRepository.AddAsync(new Domain.Inbox.InboxMessage(request.MessageId), cancellationToken);

           await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success;


        }
    }
}
