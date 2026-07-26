using FleetOps.Order.Application.Orders.Commands.CreateOrder;
using FleetOps.Order.Application.Orders.Queries.GetOrderById;
using FleetOps.Order.IntegrationTests.Infrastructure;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace FleetOps.Order.IntegrationTests.OrdersTests
{
    public sealed class GetOrderByIdEndpointTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly HttpClient _client;
        public GetOrderByIdEndpointTests(CustomWebApplicationFactory factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task GetById_WhenOrderDoesNotExist_ShouldReturnNotFound()
        {
            //Arrange
            var orderId = Guid.NewGuid();

            //Act
            var reaponse = await _client.GetAsync($"/api/orders/{orderId}");

            //Assert
            reaponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }


        [Fact]
        public async Task GetById_WhenOrderExists_ShouldReturnOrder()
        {
            // Arrange
            var createCommand = new CreateOrderCommand(
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

            var createResponse = await _client.PostAsJsonAsync(
                "/api/orders",
                createCommand);

            createResponse.IsSuccessStatusCode.Should().BeTrue();

            var createdOrder = await createResponse.Content
                .ReadFromJsonAsync<CreateOrderResponse>();

            createdOrder.Should().NotBeNull();

            // Act
            var response = await _client.GetAsync(
                $"/api/orders/{createdOrder!.Id}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var order = await response.Content
                .ReadFromJsonAsync<OrderDetailsResponse>();

            order.Should().NotBeNull();
            order!.Id.Should().Be(createdOrder.Id);
            order.CustomerName.Should().Be("Mohamed Ali");
            order.CustomerPhone.Should().Be("01012345678");
            order.TrackingNumber.Should().Be(createdOrder.TrackingNumber);
        }
    }
}
