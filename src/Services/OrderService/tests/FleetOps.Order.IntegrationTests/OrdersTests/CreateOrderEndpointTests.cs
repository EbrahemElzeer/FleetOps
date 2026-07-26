using FleetOps.Order.Application.Orders.Commands.CreateOrder;
using FleetOps.Order.IntegrationTests.Infrastructure;
using FluentAssertions;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace FleetOps.Order.IntegrationTests.OrdersTests;

public sealed class CreateOrderEndpointTests  : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public CreateOrderEndpointTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Create_WhenRequestIsValid_ShouldCreateOrder()
    {
        // Arrange
        var command = new CreateOrderCommand(
            CustomerName: "Mohamed Ali",
            CustomerPhone: "01012345678",
            PickupLocation: new CreateOrderLocationDto(
                Country: "Egypt",
                Governorate: "Cairo",
                City: "Nasr City",
                Area: "Abbas El Akkad",
                Street: "Mostafa El Nahas",
                BuildingNumber: "25",
                Landmark: "City Stars",
                Latitude: 30.0561m,
                Longitude: 31.3301m),
            DeliveryLocation: new CreateOrderLocationDto(
                Country: "Egypt",
                Governorate: "Giza",
                City: "Dokki",
                Area: "Dokki",
                Street: "Tahrir Street",
                BuildingNumber: "10",
                Landmark: "Dokki Metro",
                Latitude: 30.0384m,
                Longitude: 31.2122m));

        // Act
        var response = await _client.PostAsJsonAsync(
            "/api/orders",
            command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content
            .ReadFromJsonAsync<CreateOrderResponse>();

        result.Should().NotBeNull();
        result!.Id.Should().NotBeEmpty();
        result.TrackingNumber.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task Create_WhenCustomerNameIsEmpty_ShouldReturnBadRequest()
    {
        // Arrange
        var command = new CreateOrderCommand(
            CustomerName: string.Empty,
            CustomerPhone: "01012345678",
            PickupLocation: new CreateOrderLocationDto(
                Country: "Egypt",
                Governorate: "Cairo",
                City: "Nasr City",
                Area: "Abbas El Akkad",
                Street: "Mostafa El Nahas",
                BuildingNumber: "25",
                Landmark: "City Stars",
                Latitude: 30.0561m,
                Longitude: 31.3301m),
            DeliveryLocation: new CreateOrderLocationDto(
                Country: "Egypt",
                Governorate: "Giza",
                City: "Dokki",
                Area: "Dokki",
                Street: "Tahrir Street",
                BuildingNumber: "10",
                Landmark: "Dokki Metro",
                Latitude: 30.0384m,
                Longitude: 31.2122m));

        // Act
        var response = await _client.PostAsJsonAsync(
            "/api/orders",
            command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }


    [Fact]
    public async Task Create_WhenCustomerPhoneIsEmpty_ShouldReturnBadRequest()
    {
        // Arrange
        var command = new CreateOrderCommand(
            CustomerName: "Mohamed Ali",
            CustomerPhone: string.Empty,
            PickupLocation: new CreateOrderLocationDto(
                Country: "Egypt",
                Governorate: "Cairo",
                City: "Nasr City",
                Area: "Abbas El Akkad",
                Street: "Mostafa El Nahas",
                BuildingNumber: "25",
                Landmark: "City Stars",
                Latitude: 30.0561m,
                Longitude: 31.3301m),
            DeliveryLocation: new CreateOrderLocationDto(
                Country: "Egypt",
                Governorate: "Giza",
                City: "Dokki",
                Area: "Dokki",
                Street: "Tahrir Street",
                BuildingNumber: "10",
                Landmark: "Dokki Metro",
                Latitude: 30.0384m,
                Longitude: 31.2122m));

        // Act
        var response = await _client.PostAsJsonAsync(
            "/api/orders",
            command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var problem = await response.Content
            .ReadFromJsonAsync<ValidationProblemResponse>();

        problem.Should().NotBeNull();

        problem!.Errors.Should()
            .ContainKey("Orders.CustomerPhoneRequired");

        problem.Errors["Orders.CustomerPhoneRequired"]
            .Should()
            .Contain("Customer phone is required.");
    }
    private sealed class ValidationProblemResponse
    {
        public Dictionary<string, string[]> Errors { get; init; } = [];
    }
}