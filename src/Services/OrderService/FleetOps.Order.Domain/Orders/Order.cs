using FleetOps.Order.Domain.Common;
using FleetOps.Order.Domain.Orders.Enums;
using FleetOps.Order.Domain.SeedWork;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FleetOps.Order.Domain.Orders
{
    public sealed class Order : AggregateRoot
    {
        private readonly List<OrderStatusHistory> _statusHistories = new();

        public TrackingNumber TrackingNumber { get; private set; } = null!;

        public string CustomerName { get; private set; } = string.Empty;
        public string CustomerPhone { get; private set; } = string.Empty;

        public OrderLocation PickupLocation { get; private set; } = null!;

        public OrderLocation DeliveryLocation { get; private set; } = null!;

        public Guid? DriverId { get; private set; }

        public OrderStatus Status { get; private set; }

        public DateTime CreatedAt { get; private set; }
        public DateTime? AssignedAt { get; private set; }
        public DateTime? AcceptedAt { get; private set; }
        public DateTime? PickedUpAt { get; private set; }
        public DateTime? DeliveredAt { get; private set; }
        public DateTime? CancelledAt { get; private set; }

        public DateTime? DeliveryFailedAt { get; private set; }
        public DateTime? ReturnStartedAt { get; private set; }
        public DateTime? ReturnedAt { get; private set; }

        public DeliveryFailureReason? FailureReason { get; private set; }

        public string? DeliveryFailureNotes { get; private set; }
        public IReadOnlyCollection<OrderStatusHistory> StatusHistories => _statusHistories.AsReadOnly();

        private Order() { }


        private Order(
       string customerName,
       string customerPhone,
       OrderLocation pickupLocation,
       OrderLocation deliveryLocation)
       : base(Guid.NewGuid())
        {
            TrackingNumber = TrackingNumber.Create();

            CustomerName = customerName.Trim();
            CustomerPhone = customerPhone.Trim();

            PickupLocation = pickupLocation;
            DeliveryLocation = deliveryLocation;

            Status = OrderStatus.Pending;
            CreatedAt = DateTime.UtcNow;
        }

        public static Result<Order> Create(
            string customerName,
            string customerPhone,
            OrderLocation? pickupLocation,
            OrderLocation? deliveryLocation)
        {
            var errors = new List<Error>();

            if (string.IsNullOrWhiteSpace(customerName))
                errors.Add(OrderErrors.CustomerNameRequired);

            if (string.IsNullOrWhiteSpace(customerPhone))
                errors.Add(OrderErrors.CustomerPhoneRequired);

            if (pickupLocation is null)
                errors.Add(OrderErrors.PickupLocationRequired);

            if (deliveryLocation is null)
                errors.Add(OrderErrors.DeliveryLocationRequired);

            if (errors.Count > 0)
                return Result<Order>.Failure(errors);

            return new Order(
                customerName,
                customerPhone,
                pickupLocation!,
                deliveryLocation!);
        }

        public Result AssignDriver(Guid driverId)
        {
            if (driverId == Guid.Empty) return OrderErrors.DriverIdRequired;
            if (Status != OrderStatus.Pending) return OrderErrors.CannotAssignDriver(Status);

            var oldStatus = Status;

            DriverId = driverId;
            Status = OrderStatus.Assigned;
            AssignedAt = DateTime.UtcNow;

            AddStatusHistory(oldStatus, Status, "Driver assigned manually.");
            return Result.Success;
        }


        public Result AcceptByDriver(Guid driverId)
        {
            var driverValidationResult = EnsureAssignedDriver(driverId);
            if (driverValidationResult.IsFailure) return driverValidationResult;
           
            if (Status != OrderStatus.Assigned) return OrderErrors.CannotAccept(Status);

            var oldStatus = Status;

            Status = OrderStatus.DriverAccepted;
            AcceptedAt = DateTime.UtcNow;

            AddStatusHistory(oldStatus, Status, "Driver accepted the order.");
            return Result.Success;
        }

        public Result MarkAsPickedUp(Guid driverId)
        {
            var driverValidationResult = EnsureAssignedDriver(driverId);
            if (driverValidationResult.IsFailure) return driverValidationResult;
       

            if (Status != OrderStatus.DriverAccepted) return OrderErrors.CannotMarkAsPickedUp(Status);

            var oldStatus = Status;
            Status = OrderStatus.PickedUp;
            PickedUpAt = DateTime.UtcNow;

            AddStatusHistory(oldStatus, Status, "Order picked up by driver.");
            return Result.Success;
        }

        public Result MarkAsDelivered(Guid driverId)
        {
            var driverValidationResult = EnsureAssignedDriver(driverId);

            if (Status != OrderStatus.PickedUp) return OrderErrors.CannotMarkAsDelivered(Status);

            var oldStatus = Status;

            Status = OrderStatus.Delivered;
            DeliveredAt = DateTime.UtcNow;
            AddStatusHistory(oldStatus, Status, "Order delivered.");
            return Result.Success;
        }
        public Result Cancel(string? reason = null)
        {
            if (Status is not OrderStatus.Pending
                and not OrderStatus.Assigned
                and not OrderStatus.DriverAccepted) return OrderErrors.CannotCancel(Status);
          
            var oldStatus = Status;

            Status = OrderStatus.Cancelled;
            CancelledAt = DateTime.UtcNow;

            AddStatusHistory(
                oldStatus,
                Status,
                string.IsNullOrWhiteSpace(reason)
                    ? "Order cancelled."
                    : reason.Trim());

            return Result.Success;
        }



        public Result MarkDeliveryFailed(Guid driverId,DeliveryFailureReason reason,string? notes = null)
        {
            var driverValidationResult = EnsureAssignedDriver(driverId);
            if (driverValidationResult.IsFailure) return driverValidationResult;

            if (Status != OrderStatus.PickedUp) return OrderErrors.CannotMarkDeliveryFailed(Status);
            if (!Enum.IsDefined(reason)) return OrderErrors.InvalidDeliveryFailureReason;

            if (reason == DeliveryFailureReason.Other && string.IsNullOrWhiteSpace(notes))
                return OrderErrors.DeliveryFailureNotesRequired;

            var oldStatus = Status;
            Status = OrderStatus.DeliveryFailed;
            FailureReason = reason;
            DeliveryFailureNotes = string.IsNullOrWhiteSpace(notes)
                ? null
                : notes.Trim();

            DeliveryFailedAt = DateTime.UtcNow;

            AddStatusHistory(
                oldStatus,
                Status,
                BuildDeliveryFailureHistoryNote(reason, notes));

            return Result.Success;
        }

        public Result StartReturnToSender()
        {
            if (Status != OrderStatus.DeliveryFailed) return OrderErrors.CannotStartReturnToSender(Status);
          
            var oldStatus = Status;

            Status = OrderStatus.ReturningToSender;
            ReturnStartedAt = DateTime.UtcNow;

            AddStatusHistory(
                oldStatus,
                Status,
                "Return to sender started.");
            return Result.Success;
        }

        private static string BuildDeliveryFailureHistoryNote(DeliveryFailureReason reason, string? notes)
        {
            var message = $"Delivery failed. Reason: {reason}.";

            if (!string.IsNullOrWhiteSpace(notes))
                message += $" Notes: {notes.Trim()}";

            return message;
        }

        public Result MarkAsReturned(Guid driverId)
        {
            var driverValidationResult = EnsureAssignedDriver(driverId);

            if (driverValidationResult.IsFailure)
                return driverValidationResult;

            if (Status != OrderStatus.ReturningToSender)
                return OrderErrors.CannotMarkAsReturned(Status);

            var oldStatus = Status;

            Status = OrderStatus.Returned;
            ReturnedAt = DateTime.UtcNow;

            AddStatusHistory(
                oldStatus,
                Status,
                "Order returned to sender.");

            return Result.Success;
        }




        private Result  EnsureAssignedDriver(Guid driverId)
        {
            if (driverId == Guid.Empty) return OrderErrors.DriverIdRequired;
           
            if (DriverId is null) return OrderErrors.OrderHasNoAssignedDriver;

            if (DriverId.Value != driverId) return OrderErrors.AssignedToAnotherDriver;
            return Result.Success;
        }


        private void AddStatusHistory(OrderStatus fromStatus, OrderStatus toStatus, string? notes)
        {
            _statusHistories.Add(new OrderStatusHistory(Id, fromStatus, toStatus, notes));
        }

      
    }
}
