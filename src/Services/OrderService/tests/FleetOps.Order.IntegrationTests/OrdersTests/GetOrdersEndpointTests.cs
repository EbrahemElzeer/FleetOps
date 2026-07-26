using FleetOps.Order.Application.Common.Pagination;
using FleetOps.Order.Application.Orders.Commands.CreateOrder;
using FleetOps.Order.Application.Orders.Queries.GetOrders;
using FleetOps.Order.IntegrationTests.Infrastructure;
using FluentAssertions;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace FleetOps.Order.IntegrationTests.OrdersTests;

public sealed class GetOrdersEndpointTests: IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public GetOrdersEndpointTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetOrders_WhenNoOrdersMatch_ShouldReturnEmptyPagedResponse()
    {
        // Arrange
        var governorate = $"NotFound-{Guid.NewGuid()}";

        // Act
        var response = await _client.GetAsync(
            $"/api/orders?pickupGovernorate={Uri.EscapeDataString(governorate)}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content
            .ReadFromJsonAsync<PagedResponse<OrderListItemResponse>>();

        result.Should().NotBeNull();
        result!.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
        result.TotalPages.Should().Be(0);
        result.HasPreviousPage.Should().BeFalse();
        result.HasNextPage.Should().BeFalse();
    }

    [Fact]
    public async Task GetOrders_WhenOrderExists_ShouldReturnOrder()
    {
        // Arrange
        var pickupGovernorate = $"Cairo-{Guid.NewGuid()}";

        var createdOrder = await CreateOrderAsync(
            customerName: "Mohamed Ali",
            customerPhone: "01012345678",
            pickupGovernorate: pickupGovernorate,
            deliveryGovernorate: "Giza");

        // Act
        var response = await _client.GetAsync(
            $"/api/orders?pickupGovernorate={Uri.EscapeDataString(pickupGovernorate)}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content
            .ReadFromJsonAsync<PagedResponse<OrderListItemResponse>>();

        result.Should().NotBeNull();
        result!.Items.Should().ContainSingle();

        result.Items.Should().Contain(order =>
            order.Id == createdOrder.Id);

        result.TotalCount.Should().Be(1);
        result.PageNumber.Should().Be(1);
        result.TotalPages.Should().Be(1);
        result.HasPreviousPage.Should().BeFalse();
        result.HasNextPage.Should().BeFalse();
    }

    [Fact]
    public async Task GetOrders_WhenPickupGovernorateIsProvided_ShouldReturnMatchingOrdersOnly()
    {
        // Arrange
        var matchingGovernorate = $"Cairo-{Guid.NewGuid()}";
        var otherGovernorate = $"Alexandria-{Guid.NewGuid()}";

        var matchingOrder = await CreateOrderAsync(
            customerName: "Customer One",
            customerPhone: "01011111111",
            pickupGovernorate: matchingGovernorate,
            deliveryGovernorate: "Giza");

        await CreateOrderAsync(
            customerName: "Customer Two",
            customerPhone: "01022222222",
            pickupGovernorate: otherGovernorate,
            deliveryGovernorate: "Giza");

        // Act
        var response = await _client.GetAsync(
            $"/api/orders?pickupGovernorate={Uri.EscapeDataString(matchingGovernorate)}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content
            .ReadFromJsonAsync<PagedResponse<OrderListItemResponse>>();

        result.Should().NotBeNull();
        result!.Items.Should().ContainSingle();
        result.TotalCount.Should().Be(1);

        result.Items.Single().Id.Should().Be(matchingOrder.Id);
    }

    [Fact]
    public async Task GetOrders_WhenDeliveryGovernorateIsProvided_ShouldReturnMatchingOrdersOnly()
    {
        // Arrange
        var matchingGovernorate = $"Giza-{Guid.NewGuid()}";
        var otherGovernorate = $"Cairo-{Guid.NewGuid()}";

        var matchingOrder = await CreateOrderAsync(
            customerName: "Customer One",
            customerPhone: "01033333333",
            pickupGovernorate: "Cairo",
            deliveryGovernorate: matchingGovernorate);

        await CreateOrderAsync(
            customerName: "Customer Two",
            customerPhone: "01044444444",
            pickupGovernorate: "Cairo",
            deliveryGovernorate: otherGovernorate);

        // Act
        var response = await _client.GetAsync(
            $"/api/orders?deliveryGovernorate={Uri.EscapeDataString(matchingGovernorate)}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content
            .ReadFromJsonAsync<PagedResponse<OrderListItemResponse>>();

        result.Should().NotBeNull();
        result!.Items.Should().ContainSingle();
        result.TotalCount.Should().Be(1);

        result.Items.Single().Id.Should().Be(matchingOrder.Id);
    }

    [Fact]
    public async Task GetOrders_WhenPageSizeIsTwo_ShouldReturnFirstPageWithTwoOrders()
    {
        // Arrange
        var pickupGovernorate = $"Pagination-{Guid.NewGuid()}";

        await CreateOrderAsync(
            "Customer One",
            "01055555551",
            pickupGovernorate,
            "Giza");

        await CreateOrderAsync(
            "Customer Two",
            "01055555552",
            pickupGovernorate,
            "Giza");

        await CreateOrderAsync(
            "Customer Three",
            "01055555553",
            pickupGovernorate,
            "Giza");

        // Act
        var response = await _client.GetAsync(
            $"/api/orders" +
            $"?pickupGovernorate={Uri.EscapeDataString(pickupGovernorate)}" +
            $"&pageNumber=1&pageSize=2");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content
            .ReadFromJsonAsync<PagedResponse<OrderListItemResponse>>();

        result.Should().NotBeNull();

        result!.Items.Should().HaveCount(2);
        result.PageNumber.Should().Be(1);
        result.PageSize.Should().Be(2);
        result.TotalCount.Should().Be(3);
        result.TotalPages.Should().Be(2);
        result.HasPreviousPage.Should().BeFalse();
        result.HasNextPage.Should().BeTrue();
    }

    [Fact]
    public async Task GetOrders_WhenRequestingSecondPage_ShouldReturnRemainingOrder()
    {
        // Arrange
        var pickupGovernorate = $"SecondPage-{Guid.NewGuid()}";

        await CreateOrderAsync(
            "Customer One",
            "01066666661",
            pickupGovernorate,
            "Giza");

        await CreateOrderAsync(
            "Customer Two",
            "01066666662",
            pickupGovernorate,
            "Giza");

        await CreateOrderAsync(
            "Customer Three",
            "01066666663",
            pickupGovernorate,
            "Giza");

        // Act
        var response = await _client.GetAsync(
            $"/api/orders" +
            $"?pickupGovernorate={Uri.EscapeDataString(pickupGovernorate)}" +
            $"&pageNumber=2&pageSize=2");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content
            .ReadFromJsonAsync<PagedResponse<OrderListItemResponse>>();

        result.Should().NotBeNull();

        result!.Items.Should().ContainSingle();
        result.PageNumber.Should().Be(2);
        result.PageSize.Should().Be(2);
        result.TotalCount.Should().Be(3);
        result.TotalPages.Should().Be(2);
        result.HasPreviousPage.Should().BeTrue();
        result.HasNextPage.Should().BeFalse();
    }

    private async Task<CreateOrderResponse> CreateOrderAsync(
        string customerName,
        string customerPhone,
        string pickupGovernorate,
        string deliveryGovernorate)
    {
        var command = new CreateOrderCommand(
            CustomerName: customerName,
            CustomerPhone: customerPhone,
            PickupLocation: new CreateOrderLocationDto(
                Country: "Egypt",
                Governorate: pickupGovernorate,
                City: "Nasr City",
                Area: "Nasr City",
                Street: "Abbas El Akkad",
                BuildingNumber: "10",
                Landmark: "City Stars",
                Latitude: 30.0561m,
                Longitude: 31.3301m),
            DeliveryLocation: new CreateOrderLocationDto(
                Country: "Egypt",
                Governorate: deliveryGovernorate,
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

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var createdOrder = await response.Content
            .ReadFromJsonAsync<CreateOrderResponse>();

        createdOrder.Should().NotBeNull();
        createdOrder!.Id.Should().NotBeEmpty();
        createdOrder.TrackingNumber.Should().NotBeNullOrWhiteSpace();

        return createdOrder;
    }
}