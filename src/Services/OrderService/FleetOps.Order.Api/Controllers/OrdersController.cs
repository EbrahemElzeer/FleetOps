using FleetOps.Order.Application.Orders.Commands.CreateOrder;
using FleetOps.Order.Application.Orders.Queries.GetOrderById;
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


    }
}
