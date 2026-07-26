using FleetOps.Order.Application.Orders.Commands.AcceptOrder;
using FleetOps.Order.Application.Orders.Commands.AssignDriver;
using FleetOps.Order.Application.Orders.Commands.CreateOrder;
using FleetOps.Order.Application.Orders.Queries.GetOrderById;
using FleetOps.Order.Domain.Orders.Enums;
using FleetOps.Order.IntegrationTests.Infrastructure;
using FluentAssertions;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace FleetOps.Order.IntegrationTests.OrdersTests;

public sealed class AcceptOrderEndpointTests: IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public AcceptOrderEndpointTests(
        CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task AcceptOrder_WhenOrderDoesNotExist_ShouldReturnNotFound()
    {
        // Arrange
        var orderId = Guid.NewGuid();

        var request = new AcceptOrderRequest(
            DriverId: Guid.NewGuid());

        // Act
        var response = await _client.PutAsJsonAsync(
            $"/api/orders/{orderId}/accept",
            request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task AcceptOrder_WhenOrderIsAssignedToDriver_ShouldAcceptSuccessfully()
    {
        // Arrange
        var createdOrder = await CreateOrderAsync();
        var driverId = Guid.NewGuid();

        await AssignDriverAsync(createdOrder.Id, driverId);

        var request = new AcceptOrderRequest(
            DriverId: driverId);

        // Act
        var response = await _client.PutAsJsonAsync(
            $"/api/orders/{createdOrder.Id}/accept",
            request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var order = await GetOrderByIdAsync(createdOrder.Id);

        order.DriverId.Should().Be(driverId);
        order.Status.Should().Be(OrderStatus.DriverAccepted.ToString());
    }

    [Fact]
    public async Task AcceptOrder_WhenDriverIdIsEmpty_ShouldReturnBadRequest()
    {
        // Arrange
        var createdOrder = await CreateOrderAsync();
        var assignedDriverId = Guid.NewGuid();

        await AssignDriverAsync(
            createdOrder.Id,
            assignedDriverId);

        var request = new AcceptOrderRequest(
            DriverId: Guid.Empty);

        // Act
        var response = await _client.PutAsJsonAsync(
            $"/api/orders/{createdOrder.Id}/accept",
            request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var order = await GetOrderByIdAsync(createdOrder.Id);

        order.DriverId.Should().Be(assignedDriverId);
        order.Status.Should().Be(OrderStatus.Assigned.ToString());
    }

    [Fact]
    public async Task AcceptOrder_WhenDriverIsNotAssignedDriver_ShouldReturnForbidden()
    {
        // Arrange
        var createdOrder = await CreateOrderAsync();

        var assignedDriverId = Guid.NewGuid();
        var otherDriverId = Guid.NewGuid();

        await AssignDriverAsync(
            createdOrder.Id,
            assignedDriverId);

        var request = new AcceptOrderRequest(
            DriverId: otherDriverId);

        // Act
        var response = await _client.PutAsJsonAsync(
            $"/api/orders/{createdOrder.Id}/accept",
            request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);

        var order = await GetOrderByIdAsync(createdOrder.Id);

        order.DriverId.Should().Be(assignedDriverId);
        order.Status.Should().Be(OrderStatus.Assigned.ToString());
    }

    [Fact]
    public async Task AcceptOrder_WhenOrderIsPending_ShouldReturnConflict()
    {
        // Arrange
        var createdOrder = await CreateOrderAsync();

        var request = new AcceptOrderRequest(
            DriverId: Guid.NewGuid());

        // Act
        var response = await _client.PutAsJsonAsync(
            $"/api/orders/{createdOrder.Id}/accept",
            request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);

        var order = await GetOrderByIdAsync(createdOrder.Id);

        order.DriverId.Should().BeNull();
        order.Status.Should().Be(OrderStatus.Pending.ToString());
    }

    [Fact]
    public async Task AcceptOrder_WhenOrderIsAlreadyAccepted_ShouldReturnConflict()
    {
        // Arrange
        var createdOrder = await CreateOrderAsync();
        var driverId = Guid.NewGuid();

        await AssignDriverAsync(createdOrder.Id, driverId);

        var firstRequest = new AcceptOrderRequest(
            DriverId: driverId);

        var firstResponse = await _client.PutAsJsonAsync(
            $"/api/orders/{createdOrder.Id}/accept",
            firstRequest);

        firstResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var secondRequest = new AcceptOrderRequest(
            DriverId: driverId);

        // Act
        var response = await _client.PutAsJsonAsync(
            $"/api/orders/{createdOrder.Id}/accept",
            secondRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);

        var order = await GetOrderByIdAsync(createdOrder.Id);

        order.DriverId.Should().Be(driverId);
        order.Status.Should().Be(OrderStatus.DriverAccepted.ToString());
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
        createdOrder!.Id.Should().NotBeEmpty();

        return createdOrder;
    }

    private async Task AssignDriverAsync(
        Guid orderId,
        Guid driverId)
    {
        var request = new AssignDriverRequest(
            DriverId: driverId);

        var response = await _client.PutAsJsonAsync(
            $"/api/orders/{orderId}/assign-driver",
            request);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    private async Task<OrderDetailsResponse> GetOrderByIdAsync(
        Guid orderId)
    {
        var response = await _client.GetAsync(
            $"/api/orders/{orderId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var order = await response.Content
            .ReadFromJsonAsync<OrderDetailsResponse>();

        order.Should().NotBeNull();

        return order;
    }
}