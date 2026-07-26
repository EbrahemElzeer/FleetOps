using FleetOps.Order.Application.Abstractions;
using FleetOps.Order.Application.Orders.Queries.GetOrderById;
using FleetOps.Order.Domain.Orders;
using FleetOps.Order.Domain.Orders.Enums;
using FluentAssertions;
using Moq;
using Xunit;

namespace FleetOps.Order.Tests.Application.OrdersTests.QueriesTests.GetOrderByIdTests;

public class GetOrderByIdQueryHandlerTests
{
    private readonly Mock<IOrderRepository> _orderRepositoryMock;
    private readonly GetOrderByIdQueryHandler _handler;

    public GetOrderByIdQueryHandlerTests()
    {
        _orderRepositoryMock = new Mock<IOrderRepository>();

        _handler = new GetOrderByIdQueryHandler(
            _orderRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_WhenOrderExists_ShouldReturnOrderDetails()
    {
        // Arrange
        var order = CreatePendingOrder();

        var query = new GetOrderByIdQuery(order.Id);

        SetupOrderRepository(query.Id, order);

        // Act
        var result = await _handler.Handle(
            query,
            CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        result.Value.Id.Should().Be(order.Id);
        result.Value.TrackingNumber.Should().Be(
            order.TrackingNumber.Value);

        result.Value.CustomerName.Should().Be(
            order.CustomerName);

        result.Value.CustomerPhone.Should().Be(
            order.CustomerPhone);

        result.Value.DriverId.Should().BeNull();
        result.Value.Status.Should().Be(
            OrderStatus.Pending.ToString());

        result.Value.CreatedAt.Should().Be(
            order.CreatedAt);

        result.Value.PickupLocation.Country.Should().Be(
            order.PickupLocation.Country);

        result.Value.PickupLocation.Governorate.Should().Be(
            order.PickupLocation.Governorate);

        result.Value.PickupLocation.City.Should().Be(
            order.PickupLocation.City);

        result.Value.PickupLocation.FormattedAddress.Should().Be(
            order.PickupLocation.GetFormattedAddress());

        result.Value.DeliveryLocation.Country.Should().Be(
            order.DeliveryLocation.Country);

        result.Value.DeliveryLocation.Governorate.Should().Be(
            order.DeliveryLocation.Governorate);

        result.Value.DeliveryLocation.City.Should().Be(
            order.DeliveryLocation.City);

        result.Value.DeliveryLocation.FormattedAddress.Should().Be(
            order.DeliveryLocation.GetFormattedAddress());

        result.Value.AssignedAt.Should().BeNull();
        result.Value.AcceptedAt.Should().BeNull();
        result.Value.PickedUpAt.Should().BeNull();
        result.Value.DeliveredAt.Should().BeNull();
        result.Value.CancelledAt.Should().BeNull();
        result.Value.DeliveryFailedAt.Should().BeNull();
        result.Value.ReturnStartedAt.Should().BeNull();
        result.Value.ReturnedAt.Should().BeNull();
        result.Value.FailureReason.Should().BeNull();
        result.Value.DeliveryFailureNotes.Should().BeNull();

        _orderRepositoryMock.Verify(
            repository => repository.GetByIdAsync(
                query.Id,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WhenOrderDoesNotExist_ShouldReturnNotFound()
    {
        // Arrange
        var orderId = Guid.NewGuid();

        var query = new GetOrderByIdQuery(orderId);

        SetupOrderRepository(orderId, null);

        // Act
        var result = await _handler.Handle(
            query,
            CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();

        result.Errors.Should().Contain(
            OrderErrors.NotFound(orderId));

        _orderRepositoryMock.Verify(
            repository => repository.GetByIdAsync(
                orderId,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
    [Fact]
    public async Task Handle_WhenOrderHasStatusHistory_ShouldReturnHistoriesOrderedByChangedAt()
    {
        // Arrange
        var driverId = Guid.NewGuid();
        var order = CreateAssignedOrder(driverId);

        var query = new GetOrderByIdQuery(order.Id);

        SetupOrderRepository(query.Id, order);

        // Act
        var result = await _handler.Handle(
            query,
            CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        result.Value.StatusHistories.Should().NotBeEmpty();

        result.Value.StatusHistories
            .Should()
            .BeInAscendingOrder(history => history.ChangedAt);

        result.Value.StatusHistories.Should().Contain(
            history =>
                history.FromStatus == OrderStatus.Pending.ToString() &&
                history.ToStatus == OrderStatus.Assigned.ToString());
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