using FleetOps.Order.Application.Orders.Commands.CreateOrder;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FleetOps.Order.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly ISender _sender;

        public OrdersController(ISender sender)
        {
            _sender = sender;
        }

        [HttpPost]
        public async Task<IActionResult> Create(
         CreateOrderCommand command,
         CancellationToken cancellationToken)
        {
            var response = await _sender.Send(command, cancellationToken);

            return StatusCode(
        StatusCodes.Status201Created,
        response);

        }

    }
}
