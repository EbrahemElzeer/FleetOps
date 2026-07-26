using FleetOps.Order.Application.Abstractions;
using FleetOps.Order.Application.Common.Pagination;
using FleetOps.Order.Application.Orders.Queries.GetOrders;
using FleetOps.Order.Domain.Orders.Enums;
using FluentAssertions;
using Moq;
using Xunit;

namespace FleetOps.Order.Tests.Application.OrdersTests.QueriesTests.GetOrdersTests;

public class GetOrdersQueryHandlerTests
{
    private readonly Mock<IOrderRepository> _orderRepositoryMock;
    private readonly GetOrdersQueryHandler _handler;

    public GetOrdersQueryHandlerTests()
    {
        _orderRepositoryMock = new Mock<IOrderRepository>();

        _handler = new GetOrdersQueryHandler(
            _orderRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_WhenCalled_ShouldReturnPagedOrders()
    {
        // Arrange
        var query = new GetOrdersQuery
        {
            Status = OrderStatus.Pending,
            PickupGovernorate = "Cairo",
            DeliveryGovernorate = "Giza",
            PageNumber = 1,
            PageSize = 10
        };

        var orders = new List<OrderListItemResponse>
        {
            new(
                Id: Guid.NewGuid(),
                TrackingNumber: "TRK-001",
                CustomerName: "Ahmed Mohamed",
                CustomerPhone: "01012345678",
                PickupGovernorate: "Cairo",
                PickupArea: "Nasr City",
                DeliveryGovernorate: "Giza",
                DeliveryArea: "Dokki",
                DriverId: null,
                Status: OrderStatus.Pending.ToString(),
                CreatedAt: DateTime.UtcNow)
        };

        var pagedResponse = new PagedResponse<OrderListItemResponse>(
            orders,
            pageNumber: 1,
            pageSize: 10,
            totalCount: 1);

        _orderRepositoryMock
            .Setup(repository => repository.GetFilteredPagedAsync(
                query.Status,
                query.PickupGovernorate,
                query.DeliveryGovernorate,
                query,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagedResponse);

        // Act
        var result = await _handler.Handle(
            query,
            CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeSameAs(pagedResponse);

        result.Value.Items.Should().HaveCount(1);
        result.Value.TotalCount.Should().Be(1);
        result.Value.PageNumber.Should().Be(1);
        result.Value.PageSize.Should().Be(10);

        _orderRepositoryMock.Verify(
            repository => repository.GetFilteredPagedAsync(
                query.Status,
                query.PickupGovernorate,
                query.DeliveryGovernorate,
                query,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WhenNoOrdersMatch_ShouldReturnEmptyPagedResponse()
    {
        // Arrange
        var query = new GetOrdersQuery
        {
            Status = OrderStatus.Delivered,
            PickupGovernorate = "Cairo",
            DeliveryGovernorate = "Alexandria",
            PageNumber = 1,
            PageSize = 10
        };

        var pagedResponse = new PagedResponse<OrderListItemResponse>(
            [],
            pageNumber: 1,
            pageSize: 10,
            totalCount: 0);

        _orderRepositoryMock
            .Setup(repository => repository.GetFilteredPagedAsync(
                query.Status,
                query.PickupGovernorate,
                query.DeliveryGovernorate,
                query,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagedResponse);

        // Act
        var result = await _handler.Handle(
            query,
            CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().BeEmpty();
        result.Value.TotalCount.Should().Be(0);

        _orderRepositoryMock.Verify(
            repository => repository.GetFilteredPagedAsync(
                query.Status,
                query.PickupGovernorate,
                query.DeliveryGovernorate,
                query,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WhenFiltersAreNull_ShouldPassNullFiltersToRepository()
    {
        // Arrange
        var query = new GetOrdersQuery
        {
            Status = null,
            PickupGovernorate = null,
            DeliveryGovernorate = null,
            PageNumber = 1,
            PageSize = 20
        };

        var pagedResponse = new PagedResponse<OrderListItemResponse>(
            [],
            pageNumber: 1,
            pageSize: 20,
            totalCount: 0);

        _orderRepositoryMock
            .Setup(repository => repository.GetFilteredPagedAsync(
                null,
                null,
                null,
                query,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagedResponse);

        // Act
        var result = await _handler.Handle(
            query,
            CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        _orderRepositoryMock.Verify(
            repository => repository.GetFilteredPagedAsync(
                null,
                null,
                null,
                query,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}