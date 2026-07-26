using FleetOps.Order.Application.Orders.Commands.AcceptOrder;
using FleetOps.Order.Application.Orders.Commands.AssignDriver;
using FleetOps.Order.Application.Orders.Commands.CreateOrder;
using FleetOps.Order.Application.Orders.Commands.MarkAsReturned;
using FleetOps.Order.Application.Orders.Commands.MarkDeliveryFailed;
using FleetOps.Order.Application.Orders.Commands.MarkOrderAsPickedUp;
using FleetOps.Order.Application.Orders.Queries.GetOrderById;
using FleetOps.Order.Domain.Orders.Enums;
using FleetOps.Order.IntegrationTests.Infrastructure;
using FluentAssertions;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace FleetOps.Order.IntegrationTests.OrdersTests;

public sealed class MarkAsReturnedEndpointTests: IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public MarkAsReturnedEndpointTests(
        CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task MarkAsReturned_WhenOrderDoesNotExist_ShouldReturnNotFound()
    {
        // Arrange
        var orderId = Guid.NewGuid();

        var request = new MarkAsReturnedRequest(
            DriverId: Guid.NewGuid());

        // Act
        var response = await _client.PutAsJsonAsync(
            $"/api/orders/{orderId}/mark-as-returned",
            request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task MarkAsReturned_WhenOrderIsReturningToSender_ShouldMarkAsReturnedSuccessfully()
    {
        // Arrange
        var returningOrder = await CreateReturningOrderAsync();

        var request = new MarkAsReturnedRequest(
            DriverId: returningOrder.DriverId);

        // Act
        var response = await _client.PutAsJsonAsync(
            $"/api/orders/{returningOrder.OrderId}/mark-as-returned",
            request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var order = await GetOrderByIdAsync(returningOrder.OrderId);

        order.DriverId.Should().Be(returningOrder.DriverId);
        order.Status.Should().Be(OrderStatus.Returned.ToString());
    }

    [Fact]
    public async Task MarkAsReturned_WhenDriverIdIsEmpty_ShouldReturnBadRequest()
    {
        // Arrange
        var returningOrder = await CreateReturningOrderAsync();

        var request = new MarkAsReturnedRequest(
            DriverId: Guid.Empty);

        // Act
        var response = await _client.PutAsJsonAsync(
            $"/api/orders/{returningOrder.OrderId}/mark-as-returned",
            request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var order = await GetOrderByIdAsync(returningOrder.OrderId);

        order.DriverId.Should().Be(returningOrder.DriverId);
        order.Status.Should().Be(
            OrderStatus.ReturningToSender.ToString());
    }

    [Fact]
    public async Task MarkAsReturned_WhenDriverIsNotAssignedDriver_ShouldReturnForbidden()
    {
        // Arrange
        var returningOrder = await CreateReturningOrderAsync();
        var otherDriverId = Guid.NewGuid();

        var request = new MarkAsReturnedRequest(
            DriverId: otherDriverId);

        // Act
        var response = await _client.PutAsJsonAsync(
            $"/api/orders/{returningOrder.OrderId}/mark-as-returned",
            request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);

        var order = await GetOrderByIdAsync(returningOrder.OrderId);

        order.DriverId.Should().Be(returningOrder.DriverId);
        order.Status.Should().Be(
            OrderStatus.ReturningToSender.ToString());
    }

    [Fact]
    public async Task MarkAsReturned_WhenOrderIsPending_ShouldReturnConflict()
    {
        // Arrange
        var createdOrder = await CreateOrderAsync();

        var request = new MarkAsReturnedRequest(
            DriverId: Guid.NewGuid());

        // Act
        var response = await _client.PutAsJsonAsync(
            $"/api/orders/{createdOrder.Id}/mark-as-returned",
            request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);

        var order = await GetOrderByIdAsync(createdOrder.Id);

        order.DriverId.Should().BeNull();
        order.Status.Should().Be(OrderStatus.Pending.ToString());
    }

    [Fact]
    public async Task MarkAsReturned_WhenOrderIsAssigned_ShouldReturnConflict()
    {
        // Arrange
        var createdOrder = await CreateOrderAsync();
        var driverId = Guid.NewGuid();

        await AssignDriverAsync(createdOrder.Id, driverId);

        var request = new MarkAsReturnedRequest(
            DriverId: driverId);

        // Act
        var response = await _client.PutAsJsonAsync(
            $"/api/orders/{createdOrder.Id}/mark-as-returned",
            request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);

        var order = await GetOrderByIdAsync(createdOrder.Id);

        order.DriverId.Should().Be(driverId);
        order.Status.Should().Be(OrderStatus.Assigned.ToString());
    }

    [Fact]
    public async Task MarkAsReturned_WhenOrderIsPickedUp_ShouldReturnConflict()
    {
        // Arrange
        var pickedUpOrder = await CreatePickedUpOrderAsync();

        var request = new MarkAsReturnedRequest(
            DriverId: pickedUpOrder.DriverId);

        // Act
        var response = await _client.PutAsJsonAsync(
            $"/api/orders/{pickedUpOrder.OrderId}/mark-as-returned",
            request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);

        var order = await GetOrderByIdAsync(pickedUpOrder.OrderId);

        order.DriverId.Should().Be(pickedUpOrder.DriverId);
        order.Status.Should().Be(OrderStatus.PickedUp.ToString());
    }

    [Fact]
    public async Task MarkAsReturned_WhenOrderIsDeliveryFailed_ShouldReturnConflict()
    {
        // Arrange
        var failedOrder = await CreateDeliveryFailedOrderAsync();

        var request = new MarkAsReturnedRequest(
            DriverId: failedOrder.DriverId);

        // Act
        var response = await _client.PutAsJsonAsync(
            $"/api/orders/{failedOrder.OrderId}/mark-as-returned",
            request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);

        var order = await GetOrderByIdAsync(failedOrder.OrderId);

        order.DriverId.Should().Be(failedOrder.DriverId);
        order.Status.Should().Be(
            OrderStatus.DeliveryFailed.ToString());
    }

    [Fact]
    public async Task MarkAsReturned_WhenOrderIsAlreadyReturned_ShouldReturnConflict()
    {
        // Arrange
        var returningOrder = await CreateReturningOrderAsync();

        var firstRequest = new MarkAsReturnedRequest(
            DriverId: returningOrder.DriverId);

        var firstResponse = await _client.PutAsJsonAsync(
            $"/api/orders/{returningOrder.OrderId}/mark-as-returned",
            firstRequest);

        firstResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var secondRequest = new MarkAsReturnedRequest(
            DriverId: returningOrder.DriverId);

        // Act
        var secondResponse = await _client.PutAsJsonAsync(
            $"/api/orders/{returningOrder.OrderId}/mark-as-returned",
            secondRequest);

        // Assert
        secondResponse.StatusCode.Should().Be(
            HttpStatusCode.Conflict);

        var order = await GetOrderByIdAsync(returningOrder.OrderId);

        order.DriverId.Should().Be(returningOrder.DriverId);
        order.Status.Should().Be(OrderStatus.Returned.ToString());
    }

    private async Task<(Guid OrderId, Guid DriverId)>
        CreateReturningOrderAsync()
    {
        var failedOrder = await CreateDeliveryFailedOrderAsync();

        var response = await _client.PutAsync(
            $"/api/orders/{failedOrder.OrderId}/start-return",
            content: null);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        return failedOrder;
    }

    private async Task<(Guid OrderId, Guid DriverId)>
        CreateDeliveryFailedOrderAsync()
    {
        var pickedUpOrder = await CreatePickedUpOrderAsync();

        var request = new MarkDeliveryFailedRequest(
            DriverId: pickedUpOrder.DriverId,
            FailureReason: DeliveryFailureReason.CustomerUnavailable,
            Notes: "Customer was unavailable.");

        var response = await _client.PutAsJsonAsync(
            $"/api/orders/{pickedUpOrder.OrderId}/delivery-failed",
            request);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        return pickedUpOrder;
    }

    private async Task<(Guid OrderId, Guid DriverId)>
        CreatePickedUpOrderAsync()
    {
        var createdOrder = await CreateOrderAsync();
        var driverId = Guid.NewGuid();

        await AssignDriverAsync(createdOrder.Id, driverId);
        await AcceptOrderAsync(createdOrder.Id, driverId);
        await MarkAsPickedUpAsync(createdOrder.Id, driverId);

        return (createdOrder.Id, driverId);
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