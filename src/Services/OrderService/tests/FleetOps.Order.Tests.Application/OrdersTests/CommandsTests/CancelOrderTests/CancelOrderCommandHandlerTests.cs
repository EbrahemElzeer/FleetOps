using FleetOps.Order.Application.Abstractions;
using FleetOps.Order.Application.Orders.Commands.CancelOrder;
using FleetOps.Order.Domain.Orders;
using FleetOps.Order.Domain.Orders.Enums;
using FluentAssertions;
using Moq;
using Xunit;

namespace FleetOps.Order.Tests.Application.OrdersTests.CommandsTests.CancelOrderTests;

public class CancelOrderCommandHandlerTests
{
    private readonly Mock<IOrderRepository> _orderRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly CancelOrderCommandHandler _handler;

    public CancelOrderCommandHandlerTests()
    {
        _orderRepositoryMock = new Mock<IOrderRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();

        _handler = new CancelOrderCommandHandler(
            _unitOfWorkMock.Object,
            _orderRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_WhenOrderIsPending_ShouldCancelOrder()
    {
        // Arrange
        var order = CreatePendingOrder();

        var command = new CancelOrderCommand(
            order.Id,
            "Customer requested cancellation.");

        SetupOrderRepository(command.OrderId, order);

        // Act
        var result = await _handler.Handle(
            command,
            CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        order.Status.Should().Be(OrderStatus.Cancelled);
        order.CancelledAt.Should().NotBeNull();

        VerifyOrderWasSaved();
    }

    [Fact]
    public async Task Handle_WhenOrderIdIsEmpty_ShouldReturnOrderIdRequiredWithoutSaving()
    {
        // Arrange
        var command = new CancelOrderCommand(
            Guid.Empty,
            "Customer requested cancellation.");

        // Act
        var result = await _handler.Handle(
            command,
            CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(
            OrderErrors.OrderIdRequired);

        _orderRepositoryMock.Verify(
            repository => repository.GetByIdAsync(
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

        VerifyOrderWasNotSaved();
    }

    [Fact]
    public async Task Handle_WhenOrderDoesNotExist_ShouldReturnNotFoundWithoutSaving()
    {
        // Arrange
        var orderId = Guid.NewGuid();

        var command = new CancelOrderCommand(
            orderId,
            "Customer requested cancellation.");

        SetupOrderRepository(orderId, null);

        // Act
        var result = await _handler.Handle(
            command,
            CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(
            OrderErrors.NotFound(orderId));

        VerifyOrderWasNotSaved();
    }

    [Fact]
    public async Task Handle_WhenOrderIsDelivered_ShouldReturnFailureWithoutSaving()
    {
        // Arrange
        var driverId = Guid.NewGuid();
        var order = CreateDeliveredOrder(driverId);

        var command = new CancelOrderCommand(
            order.Id,
            "Customer requested cancellation.");

        SetupOrderRepository(command.OrderId, order);

        // Act
        var result = await _handler.Handle(
            command,
            CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(
            OrderErrors.CannotCancel(OrderStatus.Delivered));

        order.Status.Should().Be(OrderStatus.Delivered);
        order.CancelledAt.Should().BeNull();

        VerifyOrderWasNotSaved();
    }

    [Fact]
    public async Task Handle_WhenOrderIsAlreadyCancelled_ShouldReturnFailureWithoutSaving()
    {
        // Arrange
        var order = CreateCancelledOrder();

        var command = new CancelOrderCommand(
            order.Id,
            "Cancel again.");

        SetupOrderRepository(command.OrderId, order);

        // Act
        var result = await _handler.Handle(
            command,
            CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(
            OrderErrors.CannotCancel(OrderStatus.Cancelled));

        order.Status.Should().Be(OrderStatus.Cancelled);

        VerifyOrderWasNotSaved();
    }

    private void SetupOrderRepository(
        Guid orderId,
        Domain.Orders.Order? order)
    {
        _orderRepositoryMock
            .Setup(repository => repository.GetByIdAsync(
                orderId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);
    }

    private void VerifyOrderWasSaved()
    {
        _unitOfWorkMock.Verify(
            unitOfWork => unitOfWork.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    private void VerifyOrderWasNotSaved()
    {
        _unitOfWorkMock.Verify(
            unitOfWork => unitOfWork.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    private static Domain.Orders.Order CreateCancelledOrder()
    {
        var order = CreatePendingOrder();

        var result = order.Cancel(
            "Customer requested cancellation.");

        result.IsSuccess.Should().BeTrue();

        return order;
    }

    private static Domain.Orders.Order CreateDeliveredOrder(
        Guid driverId)
    {
        var order = CreatePickedUpOrder(driverId);

        var result = order.MarkAsDelivered(driverId);

        result.IsSuccess.Should().BeTrue();

        return order;
    }

    private static Domain.Orders.Order CreatePickedUpOrder(
        Guid driverId)
    {
        var order = CreateDriverAcceptedOrder(driverId);

        var result = order.MarkAsPickedUp(driverId);

        result.IsSuccess.Should().BeTrue();

        return order;
    }

    private static Domain.Orders.Order CreateDriverAcceptedOrder(
        Guid driverId)
    {
        var order = CreateAssignedOrder(driverId);

        var result = order.AcceptByDriver(driverId);

        result.IsSuccess.Should().BeTrue();

        return order;
    }

    private static Domain.Orders.Order CreateAssignedOrder(
        Guid driverId)
    {
        var order = CreatePendingOrder();

        var result = order.AssignDriver(driverId);

        result.IsSuccess.Should().BeTrue();

        return order;
    }

    private static Domain.Orders.Order CreatePendingOrder()
    {
        var pickupLocation = CreateLocation();

        var deliveryLocation = CreateLocation(
            governorate: "Giza",
            city: "Dokki",
            area: "Mesaha",
            street: "Tahrir Street",
            buildingNumber: "10",
            landmark: "Dokki Square",
            latitude: 30.0384m,
            longitude: 31.2122m);

        var result = Domain.Orders.Order.Create(
            customerName: "Ahmed Mohamed",
            customerPhone: "01012345678",
            pickupLocation,
            deliveryLocation);

        result.IsSuccess.Should().BeTrue();

        return result.Value;
    }

    private static OrderLocation CreateLocation(
        string country = "Egypt",
        string governorate = "Cairo",
        string city = "Nasr City",
        string area = "Abbas El Akkad",
        string street = "Mostafa El Nahas",
        string? buildingNumber = "25",
        string? landmark = "City Stars",
        decimal latitude = 30.0566m,
        decimal longitude = 31.3301m)
    {
        var result = OrderLocation.Create(
            country,
            governorate,
            city,
            area,
            street,
            buildingNumber,
            landmark,
            latitude,
            longitude);

        result.IsSuccess.Should().BeTrue();

        return result.Value;
    }
}