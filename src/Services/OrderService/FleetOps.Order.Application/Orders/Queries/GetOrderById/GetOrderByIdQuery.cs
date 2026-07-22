using FleetOps.Order.Domain.Common;
using MediatR;

namespace FleetOps.Order.Application.Orders.Queries.GetOrderById
{
    public sealed record GetOrderByIdQuery(Guid Id) : IRequest<Result<OrderDetailsResponse>>;
}
