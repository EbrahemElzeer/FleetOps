using FleetOps.Order.Application.Common.Pagination;
using FleetOps.Order.Application.Orders.Commands.AcceptOrder;
using FleetOps.Order.Application.Orders.Commands.AssignDriver;
using FleetOps.Order.Application.Orders.Commands.CreateOrder;
using FleetOps.Order.Application.Orders.Commands.MarkAsReturned;
using FleetOps.Order.Application.Orders.Commands.MarkDeliveryFailed;
using FleetOps.Order.Application.Orders.Commands.MarkOrderAsDelivered;
using FleetOps.Order.Application.Orders.Commands.MarkOrderAsPickedUp;
using FleetOps.Order.Application.Orders.Commands.StartReturnToSender;
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
        public async Task<ActionResult> AssignDriver(Guid id, AssignDriverRequest request,CancellationToken ct)
        {
            var command = new AssignDriverCommand(id, request.DriverId);

            var result = await _sender.Send(command, ct);

            return HandleResult(result);
        }

        [HttpPut("{id:guid}/accept")]
        public async Task<ActionResult> AcceptOrder(Guid id, AcceptOrderRequest request, CancellationToken ct)
        {
            var command = new AcceptOrderCommand(id, request.DriverId);

            var result = await _sender.Send(command, ct);

            return HandleResult(result);
        }


        [HttpPut("{id:guid}/pickup")]
        public async Task<ActionResult> MarkAsPickedUp(Guid id,MarkOrderAsPickedUpRequest request,CancellationToken ct)
        {
            var command = new MarkOrderAsPickedUpCommand(id, request.DriverId);

            var result = await _sender.Send(command, ct);

            return HandleResult(result);
        }

        [HttpPut("{id:guid}/deliver")]
        public async Task<ActionResult> MarkAsDelivered(Guid id, MarkOrderAsDeliveredRequest request, CancellationToken ct)
        {
            var command = new MarkOrderAsDeliveredCommand(id, request.DriverId);

            var result = await _sender.Send(command, ct);

            return HandleResult(result);
        }

        [HttpPut("{id:guid}/delivery-failed")]
        public async Task<ActionResult> MarkAsDeliveryFailed(Guid id, MarkDeliveryFailedRequest request, CancellationToken ct)
        {
            var command = new MarkDeliveryFailedCommand(id, request.DriverId,request.FailureReason,request.Notes);

            var result = await _sender.Send(command, ct);

            return HandleResult(result);
        }

        [HttpPut("{id:guid}/start-return")]
        public async Task<ActionResult> StartReturnToSender(Guid id,CancellationToken ct)
        {
            var command = new StartReturnToSenderCommand(id);

            var result = await _sender.Send(command, ct);

            return HandleResult(result);
        }

        [HttpPut("{id:guid}/mark-as-returned")]
        public async Task<ActionResult> MarkAsReturned(Guid id, MarkAsReturnedRequest request,CancellationToken ct)
        {
            var command = new MarkAsReturnedCommand( id,request.DriverId);

            var result = await _sender.Send(command, ct);

            return HandleResult(result);
        }
    }
}
