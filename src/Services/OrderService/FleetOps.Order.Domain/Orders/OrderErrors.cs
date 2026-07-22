using FleetOps.Order.Domain.Common;
using FleetOps.Order.Domain.Orders.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FleetOps.Order.Domain.Orders
{
    public static class OrderErrors
    {
        public static readonly Error DriverIdRequired = Error.Validation(
        code: "Orders.DriverIdRequired",
        description: "Driver id is required.");

        public static Error CannotAssignDriver(OrderStatus currentStatus) =>
            Error.Conflict(
                code: "Orders.CannotAssignDriver",
                description:
                    $"An order with status '{currentStatus}' cannot be assigned. " +
                    $"Only pending orders can be assigned.");
      
        public static Error NotFound(Guid orderId)=>
              Error.NotFound(
              code: "Orders.NotFound",
              description: $"The order with ID '{orderId}' was not found.");

        public static readonly Error OrderHasNoAssignedDriver =
            Error.Conflict(
            code: "Orders.NoAssignedDriver",
            description: "The order is not assigned to any driver.");

        public static readonly Error AssignedToAnotherDriver =
            Error.Forbidden(
            code: "Orders.AssignedToAnotherDriver",
            description: "This order is assigned to another driver.");

        public static Error CannotAccept(OrderStatus currentStatus) =>
            Error.Conflict(
                code: "Orders.CannotAccept",
                description:
                    $"An order with status '{currentStatus}' cannot be accepted. " +
                    $"Only assigned orders can be accepted.");

        public static Error CannotMarkAsPickedUp(OrderStatus currentStatus) =>
           Error.Conflict(
           code: "Orders.CannotMarkAsPickedUp",
           description:
              $"An order with status '{currentStatus}' cannot be marked as picked up. " +
              $"The order must first be accepted by the assigned driver.");

        public static Error CannotMarkAsDelivered(OrderStatus currentStatus) =>
            Error.Conflict(
            code: "Orders.CannotMarkAsDelivered",
            description:
                $"An order with status '{currentStatus}' cannot be marked as delivered. " +
                $"The order must be picked up first.");
        public static readonly Error OrderIdRequired = 
            Error.Validation(
            code: "Orders.OrderIdRequired",
            description: "Order id is required.");
        public static Error CannotCancel(OrderStatus currentStatus) =>
          Error.Conflict(
          code: "Orders.CannotCancel",
          description: $"An order with status '{currentStatus}' cannot be cancelled.");

        public static Error CannotMarkDeliveryFailed(OrderStatus currentStatus) =>
           Error.Conflict(
              code: "Orders.CannotMarkDeliveryFailed",
              description:
                $"An order with status '{currentStatus}' cannot be marked as delivery failed. " +
                $"Only picked-up orders can be marked as delivery failed.");

        public static readonly Error InvalidDeliveryFailureReason =
            Error.Validation(
                code: "Orders.InvalidDeliveryFailureReason",
                description: "The delivery failure reason is invalid.");

        public static readonly Error DeliveryFailureNotesRequired =
            Error.Validation(
                code: "Orders.DeliveryFailureNotesRequired",
                description:"Notes are required when the delivery failure reason is Other.");
        public static Error CannotStartReturnToSender(OrderStatus currentStatus) =>
            Error.Conflict(
            code: "Orders.CannotStartReturnToSender",
            description:
               $"An order with status '{currentStatus}' cannot start returning to sender. " +
               $"Only delivery-failed orders can start the return process.");


        public static readonly Error CustomerNameRequired = Error.Validation(
         code: "Orders.CustomerNameRequired",
         description: "Customer name is required.");

        public static readonly Error CustomerPhoneRequired = Error.Validation(
            code: "Orders.CustomerPhoneRequired",
            description: "Customer phone is required.");

        public static readonly Error PickupLocationRequired = Error.Validation(
            code: "Orders.PickupLocationRequired",
            description: "Pickup location is required.");

        public static readonly Error DeliveryLocationRequired = Error.Validation(
            code: "Orders.DeliveryLocationRequired",
            description: "Delivery location is required.");

        public static Error CannotMarkAsReturned(OrderStatus currentStatus) =>
        Error.Conflict(
        code: "Orders.CannotMarkAsReturned",
        description:
            $"An order with status '{currentStatus}' cannot be marked as returned. " +
            "Only orders returning to sender can be marked as returned.");
    }
}
