using FleetOps.Order.Application.Abstractions;
using FleetOps.Order.Application.IntegrationEvents.Drivers.FleetOps.Order.Application.IntegrationEvents.Drivers;
using FleetOps.Order.Domain.Common;
using FleetOps.Order.Domain.Orders;
using FleetOps.Order.Domain.Outbox;
using FleetOps.Order.Domain.Sagas;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace FleetOps.Order.Application.Sagas.AssignDriver
{
    public sealed class AssignDriverSaga : IAssignDriverSaga
    {
        private readonly IAssignDriverSagaRepository _sagaRepository;
        private readonly IDriverReservationService _driverReservationService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IOutboxRepository _outboxRepository;
        public AssignDriverSaga(
                IAssignDriverSagaRepository sagaRepository,
                IDriverReservationService driverReservationService,
                IUnitOfWork unitOfWork,
                IOutboxRepository outboxRepository)
          {
            _sagaRepository = sagaRepository;
            _driverReservationService = driverReservationService;
            _unitOfWork = unitOfWork;
            _outboxRepository = outboxRepository;
          }



        public async Task<Result> ExecuteAsync(Domain.Orders.Order order, Guid driverId, CancellationToken cancellationToken = default)
        {
            var saga = new AssignDriverSagaState(order.Id, driverId);

            await _sagaRepository.AddAsync( saga,cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var reservation = await _driverReservationService.ReserveDriverAsync(
              driverId,
              cancellationToken);

            if (!reservation.Exists)
            {
                saga.MarkFailed("Driver was not found.");

                await _unitOfWork.SaveChangesAsync(cancellationToken);

                return OrderErrors.DriverNotFound(driverId);
            }

            if (!reservation.IsReserved)
            {
                saga.MarkFailed(reservation.Reason);

                await _unitOfWork.SaveChangesAsync(cancellationToken);

                return OrderErrors.DriverNotEligible(
                    driverId,
                    reservation.Reason);
            }

            saga.MarkDriverReserved();

            await _unitOfWork.SaveChangesAsync(cancellationToken);


            var result = order.AssignDriver(driverId);

            if (result.IsFailure)
            {
                var errorMessage = string.Join(
                    " | ",
                    result.Errors.Select(x => x.Description));

                saga.MarkCompensationPending(errorMessage);

                var integrationEvent = new ReleaseDriverRequested(
                    saga.Id,
                    order.Id,
                    driverId);

                var content = JsonSerializer.Serialize(integrationEvent);

                var outboxMessage = new OutboxMessage(
                    nameof(ReleaseDriverRequested),
                    content,
                    DateTime.UtcNow);

                await _outboxRepository.AddAsync(
                    outboxMessage,
                    cancellationToken);

                await _unitOfWork.SaveChangesAsync(cancellationToken);

                return result;
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            saga.MarkCompleted();

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success;


        }
    }
}