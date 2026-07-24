using FleetOps.Order.Domain.Orders;
using FleetOps.Order.Domain.Orders.Enums;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace FleetOps.Order.Tests.Domain.OrderTests
{
    public class OrderTests
    {
        [Fact]
        public void Create_WithValidData_ShouldCreatePendingOrder()
        {
            // Arrange
            var pickupLocation = CreateLocation();
            var deliveryLocation = CreateLocation();

            var beforeCreation = DateTime.UtcNow;

            // Act
            var result = Order.Domain.Orders.Order.Create(
                customerName: "Ahmed Mohamed",
                customerPhone: "01012345678",
                pickupLocation,
                deliveryLocation);

            var afterCreation = DateTime.UtcNow;

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().NotBeNull();

            var order = result.Value;

            order.Id.Should().NotBe(Guid.Empty);
            order.CustomerName.Should().Be("Ahmed Mohamed");
            order.CustomerPhone.Should().Be("01012345678");

            order.PickupLocation.Should().Be(pickupLocation);
            order.DeliveryLocation.Should().Be(deliveryLocation);

            order.Status.Should().Be(OrderStatus.Pending);
            order.DriverId.Should().BeNull();

            order.TrackingNumber.Should().NotBeNull();
            order.TrackingNumber.Value.Should().StartWith("TRK-");

            order.CreatedAt.Should().BeOnOrAfter(beforeCreation);
            order.CreatedAt.Should().BeOnOrBefore(afterCreation);

            order.AssignedAt.Should().BeNull();
            order.AcceptedAt.Should().BeNull();
            order.PickedUpAt.Should().BeNull();
            order.DeliveredAt.Should().BeNull();
            order.CancelledAt.Should().BeNull();
            order.DeliveryFailedAt.Should().BeNull();
            order.ReturnStartedAt.Should().BeNull();
            order.ReturnedAt.Should().BeNull();

            order.FailureReason.Should().BeNull();
            order.DeliveryFailureNotes.Should().BeNull();
            order.StatusHistories.Should().BeEmpty();
        }

        private static OrderLocation CreateLocation()
        {
            return OrderLocation.Create(
                country: "Egypt",
                governorate: "Cairo",
                city: "Nasr City",
                area: "Abbas El Akkad",
                street: "Mostafa El Nahas",
                buildingNumber: "25",
                landmark: "City Stars",
                latitude: 30.0566m,
                longitude: 31.3301m).Value;
        }

        [Fact]
        public void Create_WithSpacesInCustomerData_ShouldTrimValues()
        {
            // Arrange
            var pickupLocation = CreateLocation();
            var deliveryLocation = CreateLocation();

            // Act
            var result =Order.Domain.Orders.Order.Create(
                customerName: "  Ahmed Mohamed  ",
                customerPhone: "  01012345678  ",
                pickupLocation,
                deliveryLocation);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.CustomerName.Should().Be("Ahmed Mohamed");
            result.Value.CustomerPhone.Should().Be("01012345678");
        }
        [Fact]
        public void Create_WhenCustomerNameIsEmpty_ShouldReturnCustomerNameRequiredError()
        {
            var pickupLocation = CreateLocation();
            var deliveryLocation = CreateLocation();

           var  result = Order.Domain.Orders.Order.Create(
                customerName: "",
                customerPhone: "01012345678",
                pickupLocation,
                deliveryLocation);

            result.IsFailure.Should().BeTrue();
            result.Errors.Should().Contain(OrderErrors.CustomerNameRequired);
        }
        [Fact]
        public void Create_WhenCustomerPhoneIsEmpty_ShouldReturnCustomerPhoneRequiredError()
        {
            var pickupLocation = CreateLocation();
            var deliveryLocation = CreateLocation();

            var result = Order.Domain.Orders.Order.Create(
                  customerName: "Ahmed Mohamed",
                customerPhone: "",
                pickupLocation,
                deliveryLocation);

            result.IsFailure.Should().BeTrue();
            result.Errors.Should().Contain(OrderErrors.CustomerPhoneRequired);
        }

        [Fact]
        public void Create_WhenPickupLocationIsNull_ShouldReturnPickupLocationRequiredError()
        {
            var deliveryLocation = CreateLocation();

            var result = Order.Domain.Orders.Order.Create(
                  customerName: "Ahmed Mohamed",
                customerPhone: "01012345678",
                pickupLocation: null,
                deliveryLocation);

            result.IsFailure.Should().BeTrue();
            result.Errors.Should().Contain(OrderErrors.PickupLocationRequired);
        }

        [Fact]
        public void Create_WhenDeliveryLocationIsNull_ShouldReturnDeliveryLocationRequiredError()
        {
            var pickupLocation = CreateLocation();

            var result = Order.Domain.Orders.Order.Create(
                 customerName: "Ahmed Mohamed",
                customerPhone: "01012345678",
                pickupLocation,
                deliveryLocation: null);

            result.IsFailure.Should().BeTrue();
            result.Errors.Should().Contain(OrderErrors.DeliveryLocationRequired);
        }

        [Fact]
        public void Create_WhenAllRequiredValuesAreInvalid_ShouldReturnAllErrors()
        {
            var result = Order.Domain.Orders.Order.Create(
                customerName: "",
                customerPhone: "",
                pickupLocation: null,
                deliveryLocation: null);

            result.IsFailure.Should().BeTrue();

            result.Errors.Should().Contain(OrderErrors.CustomerNameRequired);
            result.Errors.Should().Contain(OrderErrors.CustomerPhoneRequired);
            result.Errors.Should().Contain(OrderErrors.PickupLocationRequired);
            result.Errors.Should().Contain(OrderErrors.DeliveryLocationRequired);

            result.Errors.Should().HaveCount(4);
        }


        [Fact]
        public void AssignDriver_WhenOrderIsPending_ShouldAssignDriverSuccessfully()
        {
            // Arrange
            var order = CreateOrder();
            var driverId = Guid.NewGuid();

            var beforeAssignment = DateTime.UtcNow;

            // Act
            var result = order.AssignDriver(driverId);

            var afterAssignment = DateTime.UtcNow;

            // Assert
            result.IsSuccess.Should().BeTrue();

            order.DriverId.Should().Be(driverId);
            order.Status.Should().Be(OrderStatus.Assigned);

            order.AssignedAt.Should().NotBeNull();
            order.AssignedAt.Should().BeOnOrAfter(beforeAssignment);
            order.AssignedAt.Should().BeOnOrBefore(afterAssignment);

            order.StatusHistories.Should().ContainSingle();

            var history = order.StatusHistories.Single();

            history.FromStatus.Should().Be(OrderStatus.Pending);
            history.ToStatus.Should().Be(OrderStatus.Assigned);
            history.Notes.Should().Be("Driver assigned manually.");
        }


        [Fact]
        public void AssignDriver_WhenDriverIdIsEmpty_ShouldReturnDriverIdRequiredError()
        {
            // Arrange
            var order = CreateOrder();

            // Act
            var result = order.AssignDriver(Guid.Empty);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Errors.Should().Contain(OrderErrors.DriverIdRequired);

            order.DriverId.Should().BeNull();
            order.Status.Should().Be(OrderStatus.Pending);
            order.AssignedAt.Should().BeNull();
            order.StatusHistories.Should().BeEmpty();
        }

        [Fact]
        public void AssignDriver_WhenOrderIsNotPending_ShouldReturnCannotAssignDriverError()
        {
            // Arrange
            var order = CreateOrder();
            var firstDriverId = Guid.NewGuid();

            order.AssignDriver(firstDriverId);

            var secondDriverId = Guid.NewGuid();

            // Act
            var result = order.AssignDriver(secondDriverId);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Errors.Should().Contain(
                OrderErrors.CannotAssignDriver(OrderStatus.Assigned));

            order.DriverId.Should().Be(firstDriverId);
            order.Status.Should().Be(OrderStatus.Assigned);
            order.StatusHistories.Should().ContainSingle();
        }


        private static FleetOps.Order.Domain.Orders.Order CreateOrder()
        {
            var pickupLocation = CreateLocation();
            var deliveryLocation = CreateLocation();

            return FleetOps.Order.Domain.Orders.Order.Create(
                customerName: "Ahmed Mohamed",
                customerPhone: "01012345678",
                pickupLocation,
                deliveryLocation).Value;
        }

        [Fact]
        public void AcceptByDriver_WhenOrderIsAssignedToSameDriver_ShouldAcceptSuccessfully()
        {
            // Arrange
            var order = CreateOrder();
            var driverId = Guid.NewGuid();

            order.AssignDriver(driverId);

            var beforeAcceptance = DateTime.UtcNow;

            // Act
            var result = order.AcceptByDriver(driverId);

            var afterAcceptance = DateTime.UtcNow;

            // Assert
            result.IsSuccess.Should().BeTrue();

            order.Status.Should().Be(OrderStatus.DriverAccepted);
            order.AcceptedAt.Should().NotBeNull();
            order.AcceptedAt.Should().BeOnOrAfter(beforeAcceptance);
            order.AcceptedAt.Should().BeOnOrBefore(afterAcceptance);

            order.StatusHistories.Should().HaveCount(2);

            var history = order.StatusHistories.Last();

            history.FromStatus.Should().Be(OrderStatus.Assigned);
            history.ToStatus.Should().Be(OrderStatus.DriverAccepted);
            history.Notes.Should().Be("Driver accepted the order.");
        }

        [Fact]
        public void AcceptByDriver_WhenDriverIdIsEmpty_ShouldReturnDriverIdRequiredError()
        {
            // Arrange
            var order = CreateOrder();
            var assignedDriverId = Guid.NewGuid();

            order.AssignDriver(assignedDriverId);

            // Act
            var result = order.AcceptByDriver(Guid.Empty);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Errors.Should().Contain(OrderErrors.DriverIdRequired);

            order.Status.Should().Be(OrderStatus.Assigned);
            order.AcceptedAt.Should().BeNull();
            order.StatusHistories.Should().ContainSingle();
        }

        [Fact]
        public void AcceptByDriver_WhenOrderHasNoAssignedDriver_ShouldReturnNoAssignedDriverError()
        {
            // Arrange
            var order = CreateOrder();

            // Act
            var result = order.AcceptByDriver(Guid.NewGuid());

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Errors.Should().Contain(OrderErrors.OrderHasNoAssignedDriver);

            order.Status.Should().Be(OrderStatus.Pending);
            order.AcceptedAt.Should().BeNull();
            order.StatusHistories.Should().BeEmpty();
        }

        [Fact]
        public void AcceptByDriver_WhenOrderIsAssignedToAnotherDriver_ShouldReturnForbiddenError()
        {
            // Arrange
            var order = CreateOrder();
            var assignedDriverId = Guid.NewGuid();
            var anotherDriverId = Guid.NewGuid();

            order.AssignDriver(assignedDriverId);

            // Act
            var result = order.AcceptByDriver(anotherDriverId);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Errors.Should().Contain(OrderErrors.AssignedToAnotherDriver);

            order.Status.Should().Be(OrderStatus.Assigned);
            order.AcceptedAt.Should().BeNull();
            order.StatusHistories.Should().ContainSingle();
        }

        [Fact]
        public void AcceptByDriver_WhenOrderIsAlreadyAccepted_ShouldReturnCannotAcceptError()
        {
            // Arrange
            var order = CreateOrder();
            var driverId = Guid.NewGuid();

            order.AssignDriver(driverId);
            order.AcceptByDriver(driverId);

            // Act
            var result = order.AcceptByDriver(driverId);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Errors.Should().Contain(
                OrderErrors.CannotAccept(OrderStatus.DriverAccepted));

            order.Status.Should().Be(OrderStatus.DriverAccepted);
            order.StatusHistories.Should().HaveCount(2);
        }

        [Fact]
        public void MarkAsPickedUp_WhenOrderIsAcceptedBySameDriver_ShouldSucceed()
        {
            // Arrange
            var order = CreateOrder();
            var driverId = Guid.NewGuid();

            order.AssignDriver(driverId);
            order.AcceptByDriver(driverId);

            var beforePickup = DateTime.UtcNow;

            // Act
            var result = order.MarkAsPickedUp(driverId);

            var afterPickup = DateTime.UtcNow;

            // Assert
            result.IsSuccess.Should().BeTrue();

            order.Status.Should().Be(OrderStatus.PickedUp);
            order.PickedUpAt.Should().NotBeNull();
            order.PickedUpAt.Should().BeOnOrAfter(beforePickup);
            order.PickedUpAt.Should().BeOnOrBefore(afterPickup);

            order.StatusHistories.Should().HaveCount(3);

            var history = order.StatusHistories.Last();

            history.FromStatus.Should().Be(OrderStatus.DriverAccepted);
            history.ToStatus.Should().Be(OrderStatus.PickedUp);
            history.Notes.Should().Be("Order picked up by driver.");
        }

        [Fact]
        public void MarkAsPickedUp_WhenDriverIdIsEmpty_ShouldReturnDriverIdRequiredError()
        {
            // Arrange
            var order = CreateOrder();
            var driverId = Guid.NewGuid();

            order.AssignDriver(driverId);
            order.AcceptByDriver(driverId);

            // Act
            var result = order.MarkAsPickedUp(Guid.Empty);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Errors.Should().Contain(OrderErrors.DriverIdRequired);

            order.Status.Should().Be(OrderStatus.DriverAccepted);
            order.PickedUpAt.Should().BeNull();
            order.StatusHistories.Should().HaveCount(2);
        }

        [Fact]
        public void MarkAsPickedUp_WhenOrderHasNoAssignedDriver_ShouldReturnNoAssignedDriverError()
        {
            // Arrange
            var order = CreateOrder();

            // Act
            var result = order.MarkAsPickedUp(Guid.NewGuid());

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Errors.Should().Contain(OrderErrors.OrderHasNoAssignedDriver);

            order.Status.Should().Be(OrderStatus.Pending);
            order.PickedUpAt.Should().BeNull();
            order.StatusHistories.Should().BeEmpty();
        }

        [Fact]
        public void MarkAsPickedUp_WhenCalledByAnotherDriver_ShouldReturnForbiddenError()
        {
            // Arrange
            var order = CreateOrder();
            var assignedDriverId = Guid.NewGuid();
            var anotherDriverId = Guid.NewGuid();

            order.AssignDriver(assignedDriverId);
            order.AcceptByDriver(assignedDriverId);

            // Act
            var result = order.MarkAsPickedUp(anotherDriverId);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Errors.Should().Contain(OrderErrors.AssignedToAnotherDriver);

            order.Status.Should().Be(OrderStatus.DriverAccepted);
            order.PickedUpAt.Should().BeNull();
            order.StatusHistories.Should().HaveCount(2);
        }

        [Fact]
        public void MarkAsPickedUp_WhenOrderIsNotAccepted_ShouldReturnCannotMarkAsPickedUpError()
        {
            // Arrange
            var order = CreateOrder();
            var driverId = Guid.NewGuid();

            order.AssignDriver(driverId);

            // Act
            var result = order.MarkAsPickedUp(driverId);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Errors.Should().Contain(
                OrderErrors.CannotMarkAsPickedUp(OrderStatus.Assigned));

            order.Status.Should().Be(OrderStatus.Assigned);
            order.PickedUpAt.Should().BeNull();
            order.StatusHistories.Should().ContainSingle();
        }

        [Fact]
        public void MarkAsPickedUp_WhenOrderIsAlreadyPickedUp_ShouldReturnCannotMarkAsPickedUpError()
        {
            // Arrange
            var order = CreateOrder();
            var driverId = Guid.NewGuid();

            order.AssignDriver(driverId);
            order.AcceptByDriver(driverId);
            order.MarkAsPickedUp(driverId);

            // Act
            var result = order.MarkAsPickedUp(driverId);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Errors.Should().Contain(
                OrderErrors.CannotMarkAsPickedUp(OrderStatus.PickedUp));

            order.Status.Should().Be(OrderStatus.PickedUp);
            order.StatusHistories.Should().HaveCount(3);
        }


        [Fact]
        public void MarkAsDelivered_WhenOrderIsPickedUpBySameDriver_ShouldSucceed()
        {
            // Arrange
            var order = CreateOrder();
            var driverId = Guid.NewGuid();

            order.AssignDriver(driverId);
            order.AcceptByDriver(driverId);
            order.MarkAsPickedUp(driverId);

            var beforeDelivery = DateTime.UtcNow;

            // Act
            var result = order.MarkAsDelivered(driverId);

            var afterDelivery = DateTime.UtcNow;

            // Assert
            result.IsSuccess.Should().BeTrue();

            order.Status.Should().Be(OrderStatus.Delivered);
            order.DeliveredAt.Should().NotBeNull();
            order.DeliveredAt.Should().BeOnOrAfter(beforeDelivery);
            order.DeliveredAt.Should().BeOnOrBefore(afterDelivery);

            order.StatusHistories.Should().HaveCount(4);

            var history = order.StatusHistories.Last();

            history.FromStatus.Should().Be(OrderStatus.PickedUp);
            history.ToStatus.Should().Be(OrderStatus.Delivered);
            history.Notes.Should().Be("Order delivered.");
        }

        [Fact]
        public void MarkAsDelivered_WhenDriverIdIsEmpty_ShouldReturnDriverIdRequiredError()
        {
            // Arrange
            var order = CreateOrder();
            var driverId = Guid.NewGuid();

            order.AssignDriver(driverId);
            order.AcceptByDriver(driverId);
            order.MarkAsPickedUp(driverId);

            // Act
            var result = order.MarkAsDelivered(Guid.Empty);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Errors.Should().Contain(OrderErrors.DriverIdRequired);

            order.Status.Should().Be(OrderStatus.PickedUp);
            order.DeliveredAt.Should().BeNull();
            order.StatusHistories.Should().HaveCount(3);
        }

        [Fact]
        public void MarkAsDelivered_WhenOrderIsNotPickedUp_ShouldReturnCannotMarkAsDeliveredError()
        {
            // Arrange
            var order = CreateOrder();
            var driverId = Guid.NewGuid();

            order.AssignDriver(driverId);
            order.AcceptByDriver(driverId);

            // Act
            var result = order.MarkAsDelivered(driverId);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Errors.Should().Contain(
                OrderErrors.CannotMarkAsDelivered(OrderStatus.DriverAccepted));

            order.Status.Should().Be(OrderStatus.DriverAccepted);
            order.DeliveredAt.Should().BeNull();
            order.StatusHistories.Should().HaveCount(2);
        }

        [Fact]
        public void MarkAsDelivered_WhenOrderIsAlreadyDelivered_ShouldReturnCannotMarkAsDeliveredError()
        {
            // Arrange
            var order = CreateOrder();
            var driverId = Guid.NewGuid();

            order.AssignDriver(driverId);
            order.AcceptByDriver(driverId);
            order.MarkAsPickedUp(driverId);
            order.MarkAsDelivered(driverId);

            // Act
            var result = order.MarkAsDelivered(driverId);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Errors.Should().Contain(
                OrderErrors.CannotMarkAsDelivered(OrderStatus.Delivered));

            order.Status.Should().Be(OrderStatus.Delivered);
            order.StatusHistories.Should().HaveCount(4);
        }


        [Fact]
        public void Cancel_WhenOrderIsPending_ShouldCancelSuccessfully()
        {
            // Arrange
            var order = CreateOrder();

            var beforeCancellation = DateTime.UtcNow;

            // Act
            var result = order.Cancel();

            var afterCancellation = DateTime.UtcNow;

            // Assert
            result.IsSuccess.Should().BeTrue();

            order.Status.Should().Be(OrderStatus.Cancelled);
            order.CancelledAt.Should().NotBeNull();
            order.CancelledAt.Should().BeOnOrAfter(beforeCancellation);
            order.CancelledAt.Should().BeOnOrBefore(afterCancellation);

            order.StatusHistories.Should().ContainSingle();

            var history = order.StatusHistories.Single();

            history.FromStatus.Should().Be(OrderStatus.Pending);
            history.ToStatus.Should().Be(OrderStatus.Cancelled);
            history.Notes.Should().Be("Order cancelled.");
        }

        [Fact]
        public void Cancel_WithReason_ShouldTrimAndStoreReasonInHistory()
        {
            // Arrange
            var order = CreateOrder();

            // Act
            var result = order.Cancel("  Customer requested cancellation.  ");

            // Assert
            result.IsSuccess.Should().BeTrue();

            var history = order.StatusHistories.Single();

            history.Notes.Should().Be("Customer requested cancellation.");
        }

        [Fact]
        public void Cancel_WhenOrderIsAssigned_ShouldCancelSuccessfully()
        {
            // Arrange
            var order = CreateOrder();

            order.AssignDriver(Guid.NewGuid());

            // Act
            var result = order.Cancel();

            // Assert
            result.IsSuccess.Should().BeTrue();
            order.Status.Should().Be(OrderStatus.Cancelled);
            order.StatusHistories.Should().HaveCount(2);

            var history = order.StatusHistories.Last();

            history.FromStatus.Should().Be(OrderStatus.Assigned);
            history.ToStatus.Should().Be(OrderStatus.Cancelled);
        }

        [Fact]
        public void Cancel_WhenOrderIsDriverAccepted_ShouldCancelSuccessfully()
        {
            // Arrange
            var order = CreateOrder();
            var driverId = Guid.NewGuid();

            order.AssignDriver(driverId);
            order.AcceptByDriver(driverId);

            // Act
            var result = order.Cancel();

            // Assert
            result.IsSuccess.Should().BeTrue();
            order.Status.Should().Be(OrderStatus.Cancelled);
            order.StatusHistories.Should().HaveCount(3);

            var history = order.StatusHistories.Last();

            history.FromStatus.Should().Be(OrderStatus.DriverAccepted);
            history.ToStatus.Should().Be(OrderStatus.Cancelled);
        }

        [Fact]
        public void Cancel_WhenOrderIsPickedUp_ShouldReturnCannotCancelError()
        {
            // Arrange
            var order = CreateOrder();
            var driverId = Guid.NewGuid();

            order.AssignDriver(driverId);
            order.AcceptByDriver(driverId);
            order.MarkAsPickedUp(driverId);

            // Act
            var result = order.Cancel();

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Errors.Should().Contain(
                OrderErrors.CannotCancel(OrderStatus.PickedUp));

            order.Status.Should().Be(OrderStatus.PickedUp);
            order.CancelledAt.Should().BeNull();
            order.StatusHistories.Should().HaveCount(3);
        }


        [Fact]
        public void Cancel_WhenOrderIsAlreadyCancelled_ShouldReturnCannotCancelError()
        {
            // Arrange
            var order = CreateOrder();

            order.Cancel();

            // Act
            var result = order.Cancel();

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Errors.Should().Contain(
                OrderErrors.CannotCancel(OrderStatus.Cancelled));

            order.Status.Should().Be(OrderStatus.Cancelled);
            order.StatusHistories.Should().ContainSingle();
        }

        [Fact]
        public void MarkDeliveryFailed_WhenOrderIsPickedUp_ShouldSucceed()
        {
            // Arrange
            var order = CreateOrder();
            var driverId = Guid.NewGuid();

            order.AssignDriver(driverId);
            order.AcceptByDriver(driverId);
            order.MarkAsPickedUp(driverId);

            var beforeFailure = DateTime.UtcNow;

            // Act
            var result = order.MarkDeliveryFailed(
                driverId,
                DeliveryFailureReason.Other,
                "Customer location could not be reached.");

            var afterFailure = DateTime.UtcNow;

            // Assert
            result.IsSuccess.Should().BeTrue();

            order.Status.Should().Be(OrderStatus.DeliveryFailed);
            order.FailureReason.Should().Be(DeliveryFailureReason.Other);
            order.DeliveryFailureNotes.Should()
                .Be("Customer location could not be reached.");

            order.DeliveryFailedAt.Should().NotBeNull();
            order.DeliveryFailedAt.Should().BeOnOrAfter(beforeFailure);
            order.DeliveryFailedAt.Should().BeOnOrBefore(afterFailure);

            order.StatusHistories.Should().HaveCount(4);

            var history = order.StatusHistories.Last();

            history.FromStatus.Should().Be(OrderStatus.PickedUp);
            history.ToStatus.Should().Be(OrderStatus.DeliveryFailed);
            history.Notes.Should().Be(
                "Delivery failed. Reason: Other. Notes: Customer location could not be reached.");
        }


        [Fact]
        public void MarkDeliveryFailed_WithNotesContainingSpaces_ShouldTrimNotes()
        {
            // Arrange
            var order = CreateOrder();
            var driverId = Guid.NewGuid();

            order.AssignDriver(driverId);
            order.AcceptByDriver(driverId);
            order.MarkAsPickedUp(driverId);

            // Act
            var result = order.MarkDeliveryFailed(
                driverId,
                DeliveryFailureReason.Other,
                "  Customer refused the order.  ");

            // Assert
            result.IsSuccess.Should().BeTrue();

            order.DeliveryFailureNotes.Should()
                .Be("Customer refused the order.");

            order.StatusHistories.Last().Notes.Should().Be(
                "Delivery failed. Reason: Other. Notes: Customer refused the order.");
        }


        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void MarkDeliveryFailed_WhenReasonIsOtherAndNotesAreMissing_ShouldReturnNotesRequiredError(
    string? notes)
        {
            // Arrange
            var order = CreateOrder();
            var driverId = Guid.NewGuid();

            order.AssignDriver(driverId);
            order.AcceptByDriver(driverId);
            order.MarkAsPickedUp(driverId);

            // Act
            var result = order.MarkDeliveryFailed(
                driverId,
                DeliveryFailureReason.Other,
                notes);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Errors.Should().Contain(
                OrderErrors.DeliveryFailureNotesRequired);

            order.Status.Should().Be(OrderStatus.PickedUp);
            order.FailureReason.Should().BeNull();
            order.DeliveryFailureNotes.Should().BeNull();
            order.DeliveryFailedAt.Should().BeNull();
            order.StatusHistories.Should().HaveCount(3);
        }

        [Fact]
        public void MarkDeliveryFailed_WhenReasonIsInvalid_ShouldReturnInvalidReasonError()
        {
            // Arrange
            var order = CreateOrder();
            var driverId = Guid.NewGuid();

            order.AssignDriver(driverId);
            order.AcceptByDriver(driverId);
            order.MarkAsPickedUp(driverId);

            var invalidReason = (DeliveryFailureReason)999;

            // Act
            var result = order.MarkDeliveryFailed(
                driverId,
                invalidReason,
                "Invalid reason test.");

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Errors.Should().Contain(
                OrderErrors.InvalidDeliveryFailureReason);

            order.Status.Should().Be(OrderStatus.PickedUp);
            order.FailureReason.Should().BeNull();
            order.DeliveryFailedAt.Should().BeNull();
            order.StatusHistories.Should().HaveCount(3);
        }


        [Fact]
        public void MarkDeliveryFailed_WhenOrderIsNotPickedUp_ShouldReturnCannotMarkDeliveryFailedError()
        {
            // Arrange
            var order = CreateOrder();
            var driverId = Guid.NewGuid();

            order.AssignDriver(driverId);
            order.AcceptByDriver(driverId);

            // Act
            var result = order.MarkDeliveryFailed(
                driverId,
                DeliveryFailureReason.Other,
                "Customer unavailable.");

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Errors.Should().Contain(
                OrderErrors.CannotMarkDeliveryFailed(
                    OrderStatus.DriverAccepted));

            order.Status.Should().Be(OrderStatus.DriverAccepted);
            order.DeliveryFailedAt.Should().BeNull();
            order.StatusHistories.Should().HaveCount(2);
        }

        [Fact]
        public void MarkDeliveryFailed_WhenCalledByAnotherDriver_ShouldReturnForbiddenError()
        {
            // Arrange
            var order = CreateOrder();
            var assignedDriverId = Guid.NewGuid();
            var anotherDriverId = Guid.NewGuid();

            order.AssignDriver(assignedDriverId);
            order.AcceptByDriver(assignedDriverId);
            order.MarkAsPickedUp(assignedDriverId);

            // Act
            var result = order.MarkDeliveryFailed(
                anotherDriverId,
                DeliveryFailureReason.Other,
                "Customer unavailable.");

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Errors.Should().Contain(
                OrderErrors.AssignedToAnotherDriver);

            order.Status.Should().Be(OrderStatus.PickedUp);
            order.DeliveryFailedAt.Should().BeNull();
            order.StatusHistories.Should().HaveCount(3);
        }

        [Fact]
        public void MarkDeliveryFailed_WhenDriverIdIsEmpty_ShouldReturnDriverIdRequiredError()
        {
            // Arrange
            var order = CreateOrder();
            var driverId = Guid.NewGuid();

            order.AssignDriver(driverId);
            order.AcceptByDriver(driverId);
            order.MarkAsPickedUp(driverId);

            // Act
            var result = order.MarkDeliveryFailed(
                Guid.Empty,
                DeliveryFailureReason.Other,
                "Customer unavailable.");

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Errors.Should().Contain(
                OrderErrors.DriverIdRequired);

            order.Status.Should().Be(OrderStatus.PickedUp);
            order.DeliveryFailedAt.Should().BeNull();
            order.StatusHistories.Should().HaveCount(3);
        }


        [Fact]
        public void StartReturnToSender_WhenOrderIsDeliveryFailed_ShouldSucceed()
        {
            // Arrange
            var order = CreateOrder();
            var driverId = Guid.NewGuid();

            order.AssignDriver(driverId);
            order.AcceptByDriver(driverId);
            order.MarkAsPickedUp(driverId);
            order.MarkDeliveryFailed(
                driverId,
                DeliveryFailureReason.Other,
                "Customer unavailable.");

            var beforeReturn = DateTime.UtcNow;

            // Act
            var result = order.StartReturnToSender();

            var afterReturn = DateTime.UtcNow;

            // Assert
            result.IsSuccess.Should().BeTrue();

            order.Status.Should().Be(OrderStatus.ReturningToSender);
            order.ReturnStartedAt.Should().NotBeNull();
            order.ReturnStartedAt.Should().BeOnOrAfter(beforeReturn);
            order.ReturnStartedAt.Should().BeOnOrBefore(afterReturn);

            order.StatusHistories.Should().HaveCount(5);

            var history = order.StatusHistories.Last();

            history.FromStatus.Should().Be(OrderStatus.DeliveryFailed);
            history.ToStatus.Should().Be(OrderStatus.ReturningToSender);
            history.Notes.Should().Be("Return to sender started.");
        }

        [Fact]
        public void StartReturnToSender_WhenReturnAlreadyStarted_ShouldReturnCannotStartReturnError()
        {
            // Arrange
            var order = CreateOrder();
            var driverId = Guid.NewGuid();

            order.AssignDriver(driverId);
            order.AcceptByDriver(driverId);
            order.MarkAsPickedUp(driverId);
            order.MarkDeliveryFailed(
                driverId,
                DeliveryFailureReason.Other,
                "Customer unavailable.");

            order.StartReturnToSender();

            // Act
            var result = order.StartReturnToSender();

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Errors.Should().Contain(
                OrderErrors.CannotStartReturnToSender(
                    OrderStatus.ReturningToSender));

            order.Status.Should().Be(OrderStatus.ReturningToSender);
            order.StatusHistories.Should().HaveCount(5);
        }

        [Fact]
        public void StartReturnToSender_WhenOrderIsNotDeliveryFailed_ShouldReturnCannotStartReturnError()
        {
            // Arrange
            var order = CreateOrder();

            // Act
            var result = order.StartReturnToSender();

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Errors.Should().Contain(
                OrderErrors.CannotStartReturnToSender(OrderStatus.Pending));

            order.Status.Should().Be(OrderStatus.Pending);
            order.ReturnStartedAt.Should().BeNull();
            order.StatusHistories.Should().BeEmpty();
        }


        [Fact]
        public void MarkAsReturned_WhenOrderIsReturningToSender_ShouldSucceed()
        {
            // Arrange
            var order = CreateOrder();
            var driverId = Guid.NewGuid();

            order.AssignDriver(driverId);
            order.AcceptByDriver(driverId);
            order.MarkAsPickedUp(driverId);
            order.MarkDeliveryFailed(
                driverId,
                DeliveryFailureReason.Other,
                "Customer unavailable.");
            order.StartReturnToSender();

            var beforeReturn = DateTime.UtcNow;

            // Act
            var result = order.MarkAsReturned(driverId);

            var afterReturn = DateTime.UtcNow;

            // Assert
            result.IsSuccess.Should().BeTrue();

            order.Status.Should().Be(OrderStatus.Returned);
            order.ReturnedAt.Should().NotBeNull();
            order.ReturnedAt.Should().BeOnOrAfter(beforeReturn);
            order.ReturnedAt.Should().BeOnOrBefore(afterReturn);

            order.StatusHistories.Should().HaveCount(6);

            var history = order.StatusHistories.Last();

            history.FromStatus.Should().Be(OrderStatus.ReturningToSender);
            history.ToStatus.Should().Be(OrderStatus.Returned);
            history.Notes.Should().Be("Order returned to sender.");
        }

        [Fact]
        public void MarkAsReturned_WhenDriverIdIsEmpty_ShouldReturnDriverIdRequiredError()
        {
            // Arrange
            var order = CreateOrder();
            var driverId = Guid.NewGuid();

            order.AssignDriver(driverId);
            order.AcceptByDriver(driverId);
            order.MarkAsPickedUp(driverId);
            order.MarkDeliveryFailed(
                driverId,
                DeliveryFailureReason.Other,
                "Customer unavailable.");
            order.StartReturnToSender();

            // Act
            var result = order.MarkAsReturned(Guid.Empty);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Errors.Should().Contain(OrderErrors.DriverIdRequired);

            order.Status.Should().Be(OrderStatus.ReturningToSender);
            order.ReturnedAt.Should().BeNull();
            order.StatusHistories.Should().HaveCount(5);
        }


        [Fact]
        public void MarkAsReturned_WhenCalledByAnotherDriver_ShouldReturnForbiddenError()
        {
            // Arrange
            var order = CreateOrder();
            var assignedDriverId = Guid.NewGuid();
            var anotherDriverId = Guid.NewGuid();

            order.AssignDriver(assignedDriverId);
            order.AcceptByDriver(assignedDriverId);
            order.MarkAsPickedUp(assignedDriverId);
            order.MarkDeliveryFailed(
                assignedDriverId,
                DeliveryFailureReason.Other,
                "Customer unavailable.");
            order.StartReturnToSender();

            // Act
            var result = order.MarkAsReturned(anotherDriverId);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Errors.Should().Contain(OrderErrors.AssignedToAnotherDriver);

            order.Status.Should().Be(OrderStatus.ReturningToSender);
            order.ReturnedAt.Should().BeNull();
            order.StatusHistories.Should().HaveCount(5);
        }

        [Fact]
        public void MarkAsReturned_WhenOrderIsNotReturningToSender_ShouldReturnCannotMarkAsReturnedError()
        {
            // Arrange
            var order = CreateOrder();
            var driverId = Guid.NewGuid();

            order.AssignDriver(driverId);

            // Act
            var result = order.MarkAsReturned(driverId);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Errors.Should().Contain(
                OrderErrors.CannotMarkAsReturned(OrderStatus.Assigned));

            order.Status.Should().Be(OrderStatus.Assigned);
            order.ReturnedAt.Should().BeNull();
            order.StatusHistories.Should().ContainSingle();
        }


        [Fact]
        public void MarkAsReturned_WhenOrderIsAlreadyReturned_ShouldReturnCannotMarkAsReturnedError()
        {
            // Arrange
            var order = CreateOrder();
            var driverId = Guid.NewGuid();

            order.AssignDriver(driverId);
            order.AcceptByDriver(driverId);
            order.MarkAsPickedUp(driverId);
            order.MarkDeliveryFailed(
                driverId,
                DeliveryFailureReason.Other,
                "Customer unavailable.");
            order.StartReturnToSender();
            order.MarkAsReturned(driverId);

            // Act
            var result = order.MarkAsReturned(driverId);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Errors.Should().Contain(
                OrderErrors.CannotMarkAsReturned(OrderStatus.Returned));

            order.Status.Should().Be(OrderStatus.Returned);
            order.StatusHistories.Should().HaveCount(6);
        }



    }
}
