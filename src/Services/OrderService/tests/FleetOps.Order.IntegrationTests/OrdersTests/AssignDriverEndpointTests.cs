using FleetOps.Order.Application.Orders.Commands.AssignDriver;
using FleetOps.Order.Application.Orders.Commands.CreateOrder;
using FleetOps.Order.Application.Orders.Queries.GetOrderById;
using FleetOps.Order.Domain.Orders.Enums;
using FleetOps.Order.IntegrationTests.Infrastructure;
using FluentAssertions;
using System.Net;
using System.Net.Http.Json;
using Xunit;
using FleetOps.Order.Domain.Orders.Enums;
namespace FleetOps.Order.IntegrationTests.OrdersTests;

public sealed class AssignDriverEndpointTests: IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public AssignDriverEndpointTests(CustomWebApplicationFactory factory)
        
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task AssignDriver_WhenOrderDoesNotExist_ShouldReturnNotFound()
    {
        // Arrange
        var orderId = Guid.NewGuid();

        var request = new AssignDriverRequest(
            DriverId: Guid.NewGuid());

        // Act
        var response = await _client.PutAsJsonAsync(
            $"/api/orders/{orderId}/assign-driver",
            request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }


    [Fact]
    public async Task AssignDriver_WhenOrderIsPending_ShouldAssignDriverSuccessfully()
    {
        // Arrange
        var createdOrder = await CreateOrderAsync();
        var driverId = Guid.NewGuid();

        var request = new AssignDriverRequest(
            DriverId: driverId);

        // Act
        var response = await _client.PutAsJsonAsync(
            $"/api/orders/{createdOrder.Id}/assign-driver",
            request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var getResponse = await _client.GetAsync(
            $"/api/orders/{createdOrder.Id}");

        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var order = await getResponse.Content
            .ReadFromJsonAsync<OrderDetailsResponse>();

        order.Should().NotBeNull();
        order!.DriverId.Should().Be(driverId);
        order.Status.Should().Be(OrderStatus.Assigned.ToString());
    }


    [Fact]
    public async Task AssignDriver_WhenDriverIdIsEmpty_ShouldReturnBadRequest()
    {
        // Arrange
        var createdOrder = await CreateOrderAsync();

        var request = new AssignDriverRequest(
            DriverId: Guid.Empty);

        // Act
        var response = await _client.PutAsJsonAsync(
            $"/api/orders/{createdOrder.Id}/assign-driver",
            request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

   

    

    [Fact]
    public async Task AssignDriver_WhenOrderIsAlreadyAssigned_ShouldReturnConflict()
    {
        // Arrange
        var createdOrder = await CreateOrderAsync();

        var firstDriverId = Guid.NewGuid();
        var secondDriverId = Guid.NewGuid();

        var firstRequest = new AssignDriverRequest(
            DriverId: firstDriverId);

        var firstResponse = await _client.PutAsJsonAsync(
            $"/api/orders/{createdOrder.Id}/assign-driver",
            firstRequest);

        firstResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var secondRequest = new AssignDriverRequest(
            DriverId: secondDriverId);

        // Act
        var response = await _client.PutAsJsonAsync(
            $"/api/orders/{createdOrder.Id}/assign-driver",
            secondRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);

        var getResponse = await _client.GetAsync(
            $"/api/orders/{createdOrder.Id}");

        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var order = await getResponse.Content
            .ReadFromJsonAsync<OrderDetailsResponse>();

        order.Should().NotBeNull();

        order!.DriverId.Should().Be(firstDriverId);
        order.Status.Should().Be(OrderStatus.Assigned.ToString());
    }


    private async Task<CreateOrderResponse> CreateOrderAsync()
    {
        var command = new CreateOrderCommand(
            CustomerName: "Mohamed Ali",
            CustomerPhone: "01012345678",
            PickupLocation: new CreateOrderLocationDto(
                Country: "Egypt",
                Governorate: "Cairo",
                City: "Nasr City",
                Area: "Nasr City",
                Street: "Abbas El Akkad",
                BuildingNumber: "10",
                Landmark: "City Stars",
                Latitude: 30.0561m,
                Longitude: 31.3301m),
            DeliveryLocation: new CreateOrderLocationDto(
                Country: "Egypt",
                Governorate: "Giza",
                City: "Dokki",
                Area: "Dokki",
                Street: "Tahrir Street",
                BuildingNumber: "20",
                Landmark: "Dokki Metro",
                Latitude: 30.0384m,
                Longitude: 31.2122m));

        var response = await _client.PostAsJsonAsync(
            "/api/orders",
            command);

        response.IsSuccessStatusCode.Should().BeTrue();

        var createdOrder = await response.Content
            .ReadFromJsonAsync<CreateOrderResponse>();

        createdOrder.Should().NotBeNull();

        return createdOrder!;
    }
}