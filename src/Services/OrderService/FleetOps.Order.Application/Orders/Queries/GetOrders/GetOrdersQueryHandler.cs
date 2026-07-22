using FleetOps.Order.Application.Abstractions;
using FleetOps.Order.Application.Common.Pagination;
using FleetOps.Order.Domain.Common;
using MediatR;

namespace FleetOps.Order.Application.Orders.Queries.GetOrders
{
    public sealed class GetOrdersQueryHandler : IRequestHandler<GetOrdersQuery, Result<PagedResponse<OrderListItemResponse>>>
    {
        private readonly IOrderRepository _orderRepository;

        public GetOrdersQueryHandler(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }


        public async Task<Result<PagedResponse<OrderListItemResponse>>> Handle(GetOrdersQuery request, CancellationToken cancellationToken)
        {
            var response = await _orderRepository.GetFilteredPagedAsync(
          request.Status,
          request.PickupGovernorate,
          request.DeliveryGovernorate,
          request,
          cancellationToken);

            return response;
        }
    }
}
