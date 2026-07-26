using FleetOps.Order.Application.Abstractions;
using FleetOps.Order.Application.Orders.Commands.AssignDriver;
using FleetOps.Order.Domain.Orders;
using FleetOps.Order.Domain.Orders.Enums;
using FluentAssertions;
using Moq;
using Xunit;

namespace FleetOps.Order.Tests.Application.OrdersTests.CommandsTests.AssignDriverTests;

public class AssignDriverCommandHandlerTests
{
    private readonly Mock<IOrderRepository> _orderRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly AssignDriverCommandHandler _handler;

    public AssignDriverCommandHandlerTests()
    {
        _orderRepositoryMock = new Mock<IOrderRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();

        _handler = new AssignDriverCommandHandler(
            _orderRepositoryMock.Object,
            _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_WhenOrderExistsAndRequestIsValid_ShouldAssignDriver()
    {
        // Arrange
        var order = CreatePendingOrder();
        var driverId = Guid.NewGuid();

        var command = new AssignDriverCommand(
            order.Id,
            driverId);

        SetupOrderRepository(command.OrderId, order);

        // Act
        var result = await _handler.Handle(
            command,
            CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        order.DriverId.Should().Be(driverId);
        order.Status.Should().Be(OrderStatus.Assigned);
        order.AssignedAt.Should().NotBeNull();

        VerifyOrderWasSaved();
    }

    [Fact]
    public async Task Handle_WhenOrderDoesNotExist_ShouldReturnNotFoundWithoutSaving()
    {
        // Arrange
        var orderId = Guid.NewGuid();

        var command = new AssignDriverCommand(
            orderId,
            Guid.NewGuid());

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
    public async Task Handle_WhenDriverIdIsEmpty_ShouldReturnFailureWithoutSaving()
    {
        // Arrange
        var order = CreatePendingOrder();

        var command = new AssignDriverCommand(
            order.Id,
            Guid.Empty);

        SetupOrderRepository(command.OrderId, order);

        // Act
        var result = await _handler.Handle(
            command,
            CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(
            OrderErrors.DriverIdRequired);

        order.DriverId.Should().BeNull();
        order.Status.Should().Be(OrderStatus.Pending);
        order.AssignedAt.Should().BeNull();

        VerifyOrderWasNotSaved();
    }

    [Fact]
    public async Task Handle_WhenOrderIsNotPending_ShouldReturnFailureWithoutSaving()
    {
        // Arrange
        var assignedDriverId = Guid.NewGuid();
        var anotherDriverId = Guid.NewGuid();

        var order = CreateAssignedOrder(assignedDriverId);

        var command = new AssignDriverCommand(
            order.Id,
            anotherDriverId);

        SetupOrderRepository(command.OrderId, order);

        // Act
        var result = await _handler.Handle(
            command,
            CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(
            OrderErrors.CannotAssignDriver(OrderStatus.Assigned));

        order.DriverId.Should().Be(assignedDriverId);
        order.Status.Should().Be(OrderStatus.Assigned);

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
