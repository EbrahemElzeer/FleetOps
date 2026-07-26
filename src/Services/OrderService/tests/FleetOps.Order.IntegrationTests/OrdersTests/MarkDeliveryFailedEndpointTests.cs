using FleetOps.Order.Application.Orders.Commands.AcceptOrder;
using FleetOps.Order.Application.Orders.Commands.AssignDriver;
using FleetOps.Order.Application.Orders.Commands.CreateOrder;
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

public sealed class MarkDeliveryFailedEndpointTests: IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public MarkDeliveryFailedEndpointTests(
        CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task MarkDeliveryFailed_WhenOrderDoesNotExist_ShouldReturnNotFound()
    {
        // Arrange
        var orderId = Guid.NewGuid();

        var request = new MarkDeliveryFailedRequest(
            DriverId: Guid.NewGuid(),
            FailureReason: DeliveryFailureReason.CustomerUnavailable,
            Notes: null);

        // Act
        var response = await _client.PutAsJsonAsync(
            $"/api/orders/{orderId}/delivery-failed",
            request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task MarkDeliveryFailed_WhenOrderIsPickedUp_ShouldMarkAsDeliveryFailedSuccessfully()
    {
        // Arrange
        var createdOrder = await CreatePickedUpOrderAsync();
        var driverId = createdOrder.DriverId;

        var request = new MarkDeliveryFailedRequest(
            DriverId: driverId,
            FailureReason: DeliveryFailureReason.CustomerUnavailable,
            Notes: "Customer did not answer the phone.");

        // Act
        var response = await _client.PutAsJsonAsync(
            $"/api/orders/{createdOrder.OrderId}/delivery-failed",
            request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var order = await GetOrderByIdAsync(createdOrder.OrderId);

        order.DriverId.Should().Be(driverId);
        order.Status.Should().Be(OrderStatus.DeliveryFailed.ToString());
        order.FailureReason.Should().Be(
            DeliveryFailureReason.CustomerUnavailable.ToString());

        order.DeliveryFailureNotes.Should()
            .Be("Customer did not answer the phone.");
    }

    [Fact]
    public async Task MarkDeliveryFailed_WhenDriverIdIsEmpty_ShouldReturnBadRequest()
    {
        // Arrange
        var createdOrder = await CreatePickedUpOrderAsync();

        var request = new MarkDeliveryFailedRequest(
            DriverId: Guid.Empty,
            FailureReason: DeliveryFailureReason.CustomerUnavailable,
            Notes: null);

        // Act
        var response = await _client.PutAsJsonAsync(
            $"/api/orders/{createdOrder.OrderId}/delivery-failed",
            request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var order = await GetOrderByIdAsync(createdOrder.OrderId);

        order.DriverId.Should().Be(createdOrder.DriverId);
        order.Status.Should().Be(OrderStatus.PickedUp.ToString());
    }

    [Fact]
    public async Task MarkDeliveryFailed_WhenDriverIsNotAssignedDriver_ShouldReturnForbidden()
    {
        // Arrange
        var createdOrder = await CreatePickedUpOrderAsync();
        var otherDriverId = Guid.NewGuid();

        var request = new MarkDeliveryFailedRequest(
            DriverId: otherDriverId,
            FailureReason: DeliveryFailureReason.CustomerUnavailable,
            Notes: null);

        // Act
        var response = await _client.PutAsJsonAsync(
            $"/api/orders/{createdOrder.OrderId}/delivery-failed",
            request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);

        var order = await GetOrderByIdAsync(createdOrder.OrderId);

        order.DriverId.Should().Be(createdOrder.DriverId);
        order.Status.Should().Be(OrderStatus.PickedUp.ToString());
    }

    [Fact]
    public async Task MarkDeliveryFailed_WhenOrderIsPending_ShouldReturnConflict()
    {
        // Arrange
        var createdOrder = await CreateOrderAsync();

        var request = new MarkDeliveryFailedRequest(
            DriverId: Guid.NewGuid(),
            FailureReason: DeliveryFailureReason.CustomerUnavailable,
            Notes: null);

        // Act
        var response = await _client.PutAsJsonAsync(
            $"/api/orders/{createdOrder.Id}/delivery-failed",
            request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);

        var order = await GetOrderByIdAsync(createdOrder.Id);

        order.DriverId.Should().BeNull();
        order.Status.Should().Be(OrderStatus.Pending.ToString());
    }

    [Fact]
    public async Task MarkDeliveryFailed_WhenOrderIsAssigned_ShouldReturnConflict()
    {
        // Arrange
        var createdOrder = await CreateOrderAsync();
        var driverId = Guid.NewGuid();

        await AssignDriverAsync(createdOrder.Id, driverId);

        var request = new MarkDeliveryFailedRequest(
            DriverId: driverId,
            FailureReason: DeliveryFailureReason.CustomerUnavailable,
            Notes: null);

        // Act
        var response = await _client.PutAsJsonAsync(
            $"/api/orders/{createdOrder.Id}/delivery-failed",
            request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);

        var order = await GetOrderByIdAsync(createdOrder.Id);

        order.DriverId.Should().Be(driverId);
        order.Status.Should().Be(OrderStatus.Assigned.ToString());
    }

    [Fact]
    public async Task MarkDeliveryFailed_WhenOrderIsAcceptedButNotPickedUp_ShouldReturnConflict()
    {
        // Arrange
        var createdOrder = await CreateOrderAsync();
        var driverId = Guid.NewGuid();

        await AssignDriverAsync(createdOrder.Id, driverId);
        await AcceptOrderAsync(createdOrder.Id, driverId);

        var request = new MarkDeliveryFailedRequest(
            DriverId: driverId,
            FailureReason: DeliveryFailureReason.CustomerUnavailable,
            Notes: null);

        // Act
        var response = await _client.PutAsJsonAsync(
            $"/api/orders/{createdOrder.Id}/delivery-failed",
            request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);

        var order = await GetOrderByIdAsync(createdOrder.Id);

        order.DriverId.Should().Be(driverId);
        order.Status.Should().Be(OrderStatus.DriverAccepted.ToString());
    }

    [Fact]
    public async Task MarkDeliveryFailed_WhenReasonIsOtherAndNotesAreEmpty_ShouldReturnBadRequest()
    {
        // Arrange
        var createdOrder = await CreatePickedUpOrderAsync();

        var request = new MarkDeliveryFailedRequest(
            DriverId: createdOrder.DriverId,
            FailureReason: DeliveryFailureReason.Other,
            Notes: null);

        // Act
        var response = await _client.PutAsJsonAsync(
            $"/api/orders/{createdOrder.OrderId}/delivery-failed",
            request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var order = await GetOrderByIdAsync(createdOrder.OrderId);

        order.Status.Should().Be(OrderStatus.PickedUp.ToString());
        order.FailureReason.Should().BeNull();
        order.DeliveryFailureNotes.Should().BeNull();
    }

    [Fact]
    public async Task MarkDeliveryFailed_WhenReasonIsOtherAndNotesAreProvided_ShouldMarkAsDeliveryFailedSuccessfully()
    {
        // Arrange
        var createdOrder = await CreatePickedUpOrderAsync();

        var request = new MarkDeliveryFailedRequest(
            DriverId: createdOrder.DriverId,
            FailureReason: DeliveryFailureReason.Other,
            Notes: "The delivery location was inaccessible.");

        // Act
        var response = await _client.PutAsJsonAsync(
            $"/api/orders/{createdOrder.OrderId}/delivery-failed",
            request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var order = await GetOrderByIdAsync(createdOrder.OrderId);

        order.Status.Should().Be(OrderStatus.DeliveryFailed.ToString());
        order.FailureReason.Should().Be(
            DeliveryFailureReason.Other.ToString());

        order.DeliveryFailureNotes.Should()
            .Be("The delivery location was inaccessible.");
    }

    [Fact]
    public async Task MarkDeliveryFailed_WhenOrderIsAlreadyDeliveryFailed_ShouldReturnConflict()
    {
        // Arrange
        var createdOrder = await CreatePickedUpOrderAsync();

        var firstRequest = new MarkDeliveryFailedRequest(
            DriverId: createdOrder.DriverId,
            FailureReason: DeliveryFailureReason.CustomerUnavailable,
            Notes: null);

        var firstResponse = await _client.PutAsJsonAsync(
            $"/api/orders/{createdOrder.OrderId}/delivery-failed",
            firstRequest);

        firstResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var secondRequest = new MarkDeliveryFailedRequest(
            DriverId: createdOrder.DriverId,
            FailureReason: DeliveryFailureReason.WrongAddress,
            Notes: null);

        // Act
        var response = await _client.PutAsJsonAsync(
            $"/api/orders/{createdOrder.OrderId}/delivery-failed",
            secondRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);

        var order = await GetOrderByIdAsync(createdOrder.OrderId);

        order.Status.Should().Be(OrderStatus.DeliveryFailed.ToString());
        order.FailureReason.Should().Be(
            DeliveryFailureReason.CustomerUnavailable.ToString());
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