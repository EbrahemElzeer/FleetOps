using FleetOps.Order.Domain.Sagas.Enums;
using FleetOps.Order.Domain.SeedWork;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FleetOps.Order.Domain.Sagas
{
    public sealed class AssignDriverSagaState : Entity
    {
        public Guid Id { get; private set; }
        public Guid OrderId { get; private set; }
        public Guid DriverId { get; private set; }
        public AssignDriverSagaStatus Status { get; private set; }

        public int RetryCount { get; private set; }
        public string? LastError { get; private set; }

        public DateTime CreatedAt { get; private set; }
        public DateTime UpdatedAt { get; private set; }

        private AssignDriverSagaState()
        {
        }

        public AssignDriverSagaState(Guid orderId, Guid driverId)
        {
            Id = Guid.NewGuid();
            OrderId = orderId;
            DriverId = driverId;
            Status = AssignDriverSagaStatus.Started;
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }

        public void MarkDriverReserved()
        {
            Status = AssignDriverSagaStatus.DriverReserved;
            UpdatedAt = DateTime.UtcNow;
        }

        public void MarkCompleted()
        {
            Status = AssignDriverSagaStatus.Completed;
            UpdatedAt = DateTime.UtcNow;
        }

        public void MarkCompensationPending(string error)
        {
            Status = AssignDriverSagaStatus.CompensationPending;
            LastError = error;
            UpdatedAt = DateTime.UtcNow;
        }

        public void MarkCompensated()
        {
            Status = AssignDriverSagaStatus.Compensated;
            UpdatedAt = DateTime.UtcNow;
        }

        public void MarkFailed(string error)
        {
            Status = AssignDriverSagaStatus.Failed;
            LastError = error;
            UpdatedAt = DateTime.UtcNow;
        }

        public void IncrementRetry(string? error = null)
        {
            RetryCount++;

            if (!string.IsNullOrWhiteSpace(error))
                LastError = error;

            UpdatedAt = DateTime.UtcNow;
        }
    }
}
