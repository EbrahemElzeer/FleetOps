using FleetOps.Order.Application.Orders.Commands.AcceptOrder;
using FleetOps.Order.Application.Orders.Commands.AssignDriver;
using FleetOps.Order.Application.Orders.Commands.CreateOrder;
using FleetOps.Order.Application.Orders.Commands.MarkOrderAsDelivered;
using FleetOps.Order.Application.Orders.Commands.MarkOrderAsPickedUp;
using FleetOps.Order.Application.Orders.Queries.GetOrderById;
using FleetOps.Order.Domain.Orders.Enums;
using FleetOps.Order.IntegrationTests.Infrastructure;
using FluentAssertions;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace FleetOps.Order.IntegrationTests.OrdersTests;

public sealed class MarkOrderAsDeliveredEndpointTests: IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public MarkOrderAsDeliveredEndpointTests(
        CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task MarkAsDelivered_WhenOrderDoesNotExist_ShouldReturnNotFound()
    {
        // Arrange
        var orderId = Guid.NewGuid();

        var request = new MarkOrderAsDeliveredRequest(
            DriverId: Guid.NewGuid());

        // Act
        var response = await _client.PutAsJsonAsync(
            $"/api/orders/{orderId}/deliver",
            request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task MarkAsDelivered_WhenOrderIsPickedUp_ShouldMarkAsDeliveredSuccessfully()
    {
        // Arrange
        var createdOrder = await CreateOrderAsync();
        var driverId = Guid.NewGuid();

        await AssignDriverAsync(createdOrder.Id, driverId);
        await AcceptOrderAsync(createdOrder.Id, driverId);
        await MarkAsPickedUpAsync(createdOrder.Id, driverId);

        var request = new MarkOrderAsDeliveredRequest(
            DriverId: driverId);

        // Act
        var response = await _client.PutAsJsonAsync(
            $"/api/orders/{createdOrder.Id}/deliver",
            request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var order = await GetOrderByIdAsync(createdOrder.Id);

        order.DriverId.Should().Be(driverId);
        order.Status.Should().Be(OrderStatus.Delivered.ToString());
    }

    [Fact]
    public async Task MarkAsDelivered_WhenDriverIdIsEmpty_ShouldReturnBadRequest()
    {
        // Arrange
        var createdOrder = await CreateOrderAsync();
        var driverId = Guid.NewGuid();

        await AssignDriverAsync(createdOrder.Id, driverId);
        await AcceptOrderAsync(createdOrder.Id, driverId);
        await MarkAsPickedUpAsync(createdOrder.Id, driverId);

        var request = new MarkOrderAsDeliveredRequest(
            DriverId: Guid.Empty);

        // Act
        var response = await _client.PutAsJsonAsync(
            $"/api/orders/{createdOrder.Id}/deliver",
            request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var order = await GetOrderByIdAsync(createdOrder.Id);

        order.DriverId.Should().Be(driverId);
        order.Status.Should().Be(OrderStatus.PickedUp.ToString());
    }

    [Fact]
    public async Task MarkAsDelivered_WhenDriverIsNotAssignedDriver_ShouldReturnForbidden()
    {
        // Arrange
        var createdOrder = await CreateOrderAsync();

        var assignedDriverId = Guid.NewGuid();
        var otherDriverId = Guid.NewGuid();

        await AssignDriverAsync(
            createdOrder.Id,
            assignedDriverId);

        await AcceptOrderAsync(
            createdOrder.Id,
            assignedDriverId);

        await MarkAsPickedUpAsync(
            createdOrder.Id,
            assignedDriverId);

        var request = new MarkOrderAsDeliveredRequest(
            DriverId: otherDriverId);

        // Act
        var response = await _client.PutAsJsonAsync(
            $"/api/orders/{createdOrder.Id}/deliver",
            request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);

        var order = await GetOrderByIdAsync(createdOrder.Id);

        order.DriverId.Should().Be(assignedDriverId);
        order.Status.Should().Be(OrderStatus.PickedUp.ToString());
    }

    [Fact]
    public async Task MarkAsDelivered_WhenOrderIsPending_ShouldReturnConflict()
    {
        // Arrange
        var createdOrder = await CreateOrderAsync();

        var request = new MarkOrderAsDeliveredRequest(
            DriverId: Guid.NewGuid());

        // Act
        var response = await _client.PutAsJsonAsync(
            $"/api/orders/{createdOrder.Id}/deliver",
            request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);

        var order = await GetOrderByIdAsync(createdOrder.Id);

        order.DriverId.Should().BeNull();
        order.Status.Should().Be(OrderStatus.Pending.ToString());
    }

    [Fact]
    public async Task MarkAsDelivered_WhenOrderIsAssigned_ShouldReturnConflict()
    {
        // Arrange
        var createdOrder = await CreateOrderAsync();
        var driverId = Guid.NewGuid();

        await AssignDriverAsync(createdOrder.Id, driverId);

        var request = new MarkOrderAsDeliveredRequest(
            DriverId: driverId);

        // Act
        var response = await _client.PutAsJsonAsync(
            $"/api/orders/{createdOrder.Id}/deliver",
            request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);

        var order = await GetOrderByIdAsync(createdOrder.Id);

        order.DriverId.Should().Be(driverId);
        order.Status.Should().Be(OrderStatus.Assigned.ToString());
    }

    [Fact]
    public async Task MarkAsDelivered_WhenOrderIsAcceptedButNotPickedUp_ShouldReturnConflict()
    {
        // Arrange
        var createdOrder = await CreateOrderAsync();
        var driverId = Guid.NewGuid();

        await AssignDriverAsync(createdOrder.Id, driverId);
        await AcceptOrderAsync(createdOrder.Id, driverId);

        var request = new MarkOrderAsDeliveredRequest(
            DriverId: driverId);

        // Act
        var response = await _client.PutAsJsonAsync(
            $"/api/orders/{createdOrder.Id}/deliver",
            request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);

        var order = await GetOrderByIdAsync(createdOrder.Id);

        order.DriverId.Should().Be(driverId);
        order.Status.Should().Be(OrderStatus.DriverAccepted.ToString());
    }

    [Fact]
    public async Task MarkAsDelivered_WhenOrderIsAlreadyDelivered_ShouldReturnConflict()
    {
        // Arrange
        var createdOrder = await CreateOrderAsync();
        var driverId = Guid.NewGuid();

        await AssignDriverAsync(createdOrder.Id, driverId);
        await AcceptOrderAsync(createdOrder.Id, driverId);
        await MarkAsPickedUpAsync(createdOrder.Id, driverId);

        var firstRequest = new MarkOrderAsDeliveredRequest(
            DriverId: driverId);

        var firstResponse = await _client.PutAsJsonAsync(
            $"/api/orders/{createdOrder.Id}/deliver",
            firstRequest);

        firstResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var secondRequest = new MarkOrderAsDeliveredRequest(
            DriverId: driverId);

        // Act
        var response = await _client.PutAsJsonAsync(
            $"/api/orders/{createdOrder.Id}/deliver",
            secondRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);

        var order = await GetOrderByIdAsync(createdOrder.Id);

        order.DriverId.Should().Be(driverId);
        order.Status.Should().Be(OrderStatus.Delivered.ToString());
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

    private async Task AcceptOrderAsync(
        Guid orderId,
        Guid driverId)
    {
        var request = new AcceptOrderRequest(
            DriverId: driverId);

        var response = await _client.PutAsJsonAsync(
            $"/api/orders/{orderId}/accept",
            request);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    private async Task MarkAsPickedUpAsync(
        Guid orderId,
        Guid driverId)
    {
        var request = new MarkOrderAsPickedUpRequest(
            DriverId: driverId);

        var response = await _client.PutAsJsonAsync(
            $"/api/orders/{orderId}/pickup",
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

        return order!;
    }
}