using FleetOps.Order.Application.Common.Pagination;
using FleetOps.Order.Application.Orders.Commands.AssignDriver;
using FleetOps.Order.Application.Orders.Commands.CreateOrder;
using FleetOps.Order.Application.Orders.Queries.GetOrderById;
using FleetOps.Order.Application.Orders.Queries.GetOrders;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FleetOps.Order.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ApiControllerBase
    {
        private readonly ISender _sender;

        public OrdersController(ISender sender)
        {
            _sender = sender;
        }

        [HttpPost]
        public async Task<ActionResult<CreateOrderResponse>> Create(CreateOrderCommand command,CancellationToken cancellationToken)
        {
            var response = await _sender.Send(command, cancellationToken);
            return HandleResult(response);
        }


        [HttpGet("{id:guid}")]
        public async Task<ActionResult<OrderDetailsResponse>> GetById(Guid id,CancellationToken ct)
        {
            var result = await _sender.Send(new GetOrderByIdQuery(id), ct);
            return HandleResult(result);
        }

        [HttpGet]
        public async Task<ActionResult<PagedResponse<OrderListItemResponse>>> GetOrders([FromQuery] GetOrdersQuery query,CancellationToken ct)
        {
            var result = await _sender.Send(query, ct);
            return HandleResult(result);
        }

        [HttpPut("{id:guid}/assign-driver")]
        public async Task<ActionResult> AssignDriver(Guid id,AssignDriverRequest request,CancellationToken ct)
        {
            var command = new AssignDriverCommand(id, request.DriverId);

            var result = await _sender.Send(command, ct);

            return HandleResult(result);
        }
    }
}
