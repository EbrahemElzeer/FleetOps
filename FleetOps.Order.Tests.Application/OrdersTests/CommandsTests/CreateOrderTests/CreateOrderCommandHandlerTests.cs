using FleetOps.Order.Application.Abstractions;
using FleetOps.Order.Application.Orders.Commands.CreateOrder;
using FleetOps.Order.Domain.Orders;
using FleetOps.Order.Domain.Orders.Enums;
using FluentAssertions;
using Moq;
using Xunit;

namespace FleetOps.Order.Tests.Application.OrdersTests.CommandsTests.CreateOrderTests;

public class CreateOrderCommandHandlerTests
{
    private readonly Mock<IOrderRepository> _orderRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly CreateOrderCommandHandler _handler;

    public CreateOrderCommandHandlerTests()
    {
        _orderRepositoryMock = new Mock<IOrderRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();

        _handler = new CreateOrderCommandHandler(
            _unitOfWorkMock.Object,
            _orderRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_WhenRequestIsValid_ShouldCreateOrder()
    {
        // Arrange
        var command = CreateValidCommand();

        // Act
        var result = await _handler.Handle(
            command,
            CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        result.Value.Id.Should().NotBeEmpty();
        result.Value.TrackingNumber.Should().NotBeNullOrWhiteSpace();
        result.Value.Status.Should().Be(OrderStatus.Pending.ToString());
        result.Value.CreatedAt.Should().NotBe(default);

        _orderRepositoryMock.Verify(
            repository => repository.AddAsync(
                It.IsAny<Domain.Orders.Order>(),
                It.IsAny<CancellationToken>()),
            Times.Once);

        _unitOfWorkMock.Verify(
            unitOfWork => unitOfWork.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WhenPickupLocationIsInvalid_ShouldReturnFailureWithoutSaving()
    {
        // Arrange
        var invalidPickupLocation = CreateLocation(
            country: string.Empty);

        var command = CreateValidCommand(
            pickupLocation: invalidPickupLocation);

        // Act
        var result = await _handler.Handle(
            command,
            CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().NotBeEmpty();

        VerifyOrderWasNotSaved();
    }

    [Fact]
    public async Task Handle_WhenDeliveryLocationIsInvalid_ShouldReturnFailureWithoutSaving()
    {
        // Arrange
        var invalidDeliveryLocation = CreateLocation(
            latitude: 90.1m);

        var command = CreateValidCommand(
            deliveryLocation: invalidDeliveryLocation);

        // Act
        var result = await _handler.Handle(
            command,
            CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().NotBeEmpty();

        VerifyOrderWasNotSaved();
    }

    [Fact]
    public async Task Handle_WhenCustomerNameIsEmpty_ShouldReturnFailureWithoutSaving()
    {
        // Arrange
        var command = CreateValidCommand(
            customerName: string.Empty);

        // Act
        var result = await _handler.Handle(
            command,
            CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(
            OrderErrors.CustomerNameRequired);

        VerifyOrderWasNotSaved();
    }

    [Fact]
    public async Task Handle_WhenCustomerPhoneIsEmpty_ShouldReturnFailureWithoutSaving()
    {
        // Arrange
        var command = CreateValidCommand(
            customerPhone: string.Empty);

        // Act
        var result = await _handler.Handle(
            command,
            CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(
            OrderErrors.CustomerPhoneRequired);

        VerifyOrderWasNotSaved();
    }

    private static CreateOrderCommand CreateValidCommand(
        string customerName = "Ahmed Mohamed",
        string customerPhone = "01012345678",
        CreateOrderLocationDto? pickupLocation = null,
        CreateOrderLocationDto? deliveryLocation = null)
    {
        return new CreateOrderCommand(
            CustomerName: customerName,
            CustomerPhone: customerPhone,
            PickupLocation: pickupLocation ?? CreateLocation(),
            DeliveryLocation: deliveryLocation ?? CreateLocation(
                governorate: "Giza",
                city: "Dokki",
                area: "Mesaha",
                street: "Tahrir Street",
                buildingNumber: "10",
                landmark: "Dokki Square",
                latitude: 30.0384m,
                longitude: 31.2122m));
    }

    private static CreateOrderLocationDto CreateLocation(
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
        return new CreateOrderLocationDto(
            Country: country,
            Governorate: governorate,
            City: city,
            Area: area,
            Street: street,
            BuildingNumber: buildingNumber,
            Landmark: landmark,
            Latitude: latitude,
            Longitude: longitude);
    }

    private void VerifyOrderWasNotSaved()
    {
        _orderRepositoryMock.Verify(
            repository => repository.AddAsync(
                It.IsAny<Domain.Orders.Order>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

        _unitOfWorkMock.Verify(
            unitOfWork => unitOfWork.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
