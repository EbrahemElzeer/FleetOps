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


        public Order(
            string customerName,
            string customerPhone,
            OrderLocation pickupLocation,
            OrderLocation deliveryLocation) : base(Guid.NewGuid())
        {
            if (string.IsNullOrWhiteSpace(customerName))
                throw new ArgumentException("Customer name is required.", nameof(customerName));

            if (string.IsNullOrWhiteSpace(customerPhone))
                throw new ArgumentException("Customer phone is required.", nameof(customerPhone));


            ArgumentNullException.ThrowIfNull(pickupLocation);
            ArgumentNullException.ThrowIfNull(deliveryLocation);

            TrackingNumber = TrackingNumber.Create();

            CustomerName = customerName.Trim();
            CustomerPhone = customerPhone.Trim();

            PickupLocation = pickupLocation;
            DeliveryLocation = deliveryLocation;

            Status = OrderStatus.Pending;
            CreatedAt = DateTime.UtcNow;
        }

        public void AssignDriver(Guid driverId)
        {

            if (driverId == Guid.Empty) throw new ArgumentException("Driver id is required",nameof(driverId));
            if (Status != OrderStatus.Pending) throw new InvalidOperationException("Only pending orders can be assigned.");

            var oldStatus = Status;

            DriverId = driverId;
            Status = OrderStatus.Assigned;
            AssignedAt = DateTime.UtcNow;

            AddStatusHistory(oldStatus, Status, "Driver assigned manually.");
        }


        public void AcceptByDriver(Guid driverId)
        {
            EnsureAssignedDriver(driverId);

            if (Status != OrderStatus.Assigned)
                throw new InvalidOperationException("Only assigned orders can be accepted.");

            var oldStatus = Status;

            Status = OrderStatus.DriverAccepted;
            AcceptedAt = DateTime.UtcNow;

            AddStatusHistory(oldStatus, Status, "Driver accepted the order.");
        }

        public void MarkAsPickedUp(Guid driverId)
        {
            EnsureAssignedDriver(driverId);

            if (Status != OrderStatus.DriverAccepted)
                throw new InvalidOperationException("Order must be accepted before pickup.");

            var oldStatus = Status;

            Status = OrderStatus.PickedUp;
            PickedUpAt = DateTime.UtcNow;

            AddStatusHistory(oldStatus, Status, "Order picked up by driver.");
        }

        public void MarkAsDelivered(Guid driverId)
        {
            EnsureAssignedDriver(driverId);

            if (Status != OrderStatus.PickedUp )
                throw new InvalidOperationException("Order must be picked up before delivery.");

            var oldStatus = Status;

            Status = OrderStatus.Delivered;
            DeliveredAt = DateTime.UtcNow;

            AddStatusHistory(oldStatus, Status, "Order delivered.");
        }
        public void Cancel(string? reason = null)
        {
            if (Status is not OrderStatus.Pending
                and not OrderStatus.Assigned
                and not OrderStatus.DriverAccepted)
            {
                throw new InvalidOperationException(
                    $"Order cannot be cancelled while its status is {Status}.");
            }

            var oldStatus = Status;

            Status = OrderStatus.Cancelled;
            CancelledAt = DateTime.UtcNow;

            AddStatusHistory(
                oldStatus,
                Status,
                string.IsNullOrWhiteSpace(reason)
                    ? "Order cancelled."
                    : reason.Trim());
        }



        public void MarkDeliveryFailed(
    Guid driverId,
    DeliveryFailureReason reason,
    string? notes = null)
        {
            EnsureAssignedDriver(driverId);

            if (Status != OrderStatus.PickedUp)
                throw new InvalidOperationException(
                    "Only picked-up orders can be marked as delivery failed.");

            if (!Enum.IsDefined(reason))
                throw new ArgumentOutOfRangeException(
                    nameof(reason),
                    "Invalid delivery failure reason.");

            if (reason == DeliveryFailureReason.Other &&
                string.IsNullOrWhiteSpace(notes))
            {
                throw new ArgumentException(
                    "Notes are required when failure reason is Other.",
                    nameof(notes));
            }

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
        }

        public void StartReturnToSender()
        {
            if (Status != OrderStatus.DeliveryFailed)
                throw new InvalidOperationException(
                    "Only delivery-failed orders can be returned to sender.");

            var oldStatus = Status;

            Status = OrderStatus.ReturningToSender;
            ReturnStartedAt = DateTime.UtcNow;

            AddStatusHistory(
                oldStatus,
                Status,
                "Return to sender started.");
        }

        private static string BuildDeliveryFailureHistoryNote(DeliveryFailureReason reason, string? notes)
        {
            var message = $"Delivery failed. Reason: {reason}.";

            if (!string.IsNullOrWhiteSpace(notes))
                message += $" Notes: {notes.Trim()}";

            return message;
        }

        public void MarkAsReturned(Guid driverId)
        {
            EnsureAssignedDriver(driverId);

            if (Status != OrderStatus.ReturningToSender)
                throw new InvalidOperationException(
                    "Only orders returning to sender can be marked as returned.");

            var oldStatus = Status;

            Status = OrderStatus.Returned;
            ReturnedAt = DateTime.UtcNow;

            AddStatusHistory(
                oldStatus,
                Status,
                "Order returned to sender.");
        }




        private void EnsureAssignedDriver(Guid driverId)
        {
            if (driverId == Guid.Empty)
                throw new ArgumentException(
                      "Driver id is required.",
                       nameof(driverId));

            if (DriverId is null)
                throw new InvalidOperationException("Order is not assigned to any driver.");

            if (DriverId.Value != driverId)
                throw new InvalidOperationException("This order is assigned to another driver.");
        }


        private void AddStatusHistory(OrderStatus fromStatus, OrderStatus toStatus, string? notes)
        {
            _statusHistories.Add(new OrderStatusHistory(Id, fromStatus, toStatus, notes));
        }

      
    }
}
