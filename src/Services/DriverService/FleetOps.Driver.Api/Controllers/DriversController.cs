using FleetOps.Driver.Application.Common.Pagination;
using FleetOps.Driver.Application.Drivers.Commands.CreateDriver;
using FleetOps.Driver.Application.Drivers.Commands.GoOffline;
using FleetOps.Driver.Application.Drivers.Commands.GoOnline;
using FleetOps.Driver.Application.Drivers.Queries.GetDriverById;
using FleetOps.Driver.Application.Drivers.Queries.GetDrivers;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FleetOps.Driver.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DriversController : ApiControllerBase
    {
        private readonly ISender _sender;

        public DriversController(ISender sender)
        {
           _sender = sender;
        }

        [HttpPost]
        public async Task<ActionResult<CreateDriverResponse>> Create(CreateDriverDto request, CancellationToken cancellationToken)
        {
            var result = await _sender.Send(new CreateDriverCommand(request), cancellationToken);
            return HandleResult(result);
        }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<DriverDetailsResponse>> GetById(Guid id,CancellationToken cancellationToken)
        {
            var result = await _sender.Send( new GetDriverByIdQuery(id),cancellationToken);
               
            return HandleResult(result);
        }

        [HttpGet]
        public async Task<ActionResult<PagedResponse<DriverListItemResponse>>> GetDrivers([FromQuery] GetDriversQuery query,CancellationToken cancellationToken)
        {
            var result = await _sender.Send(query, cancellationToken);

            return HandleResult(result);
        }
        [HttpPut("{id:guid}/online")]
        public async Task<ActionResult> GoOnline(Guid id,CancellationToken cancellationToken)
        {
            var result = await _sender.Send(
                new GoOnlineCommand(id),
                cancellationToken);

            return HandleResult(result);
        }

        [HttpPut("{id:guid}/offline")]
        public async Task<ActionResult> GoOffline(Guid id,CancellationToken cancellationToken)
        {
            var result = await _sender.Send(
                new GoOfflineCommand(id),
                cancellationToken);

            return HandleResult(result);
        }
    }
}
