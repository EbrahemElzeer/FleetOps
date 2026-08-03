using FleetOps.Driver.Api.Contracts;
using FleetOps.Driver.Application.Common.Pagination;
using FleetOps.Driver.Application.Drivers.Commands.ActivateDriver;
using FleetOps.Driver.Application.Drivers.Commands.CreateDriver;
using FleetOps.Driver.Application.Drivers.Commands.GoOffline;
using FleetOps.Driver.Application.Drivers.Commands.GoOnline;
using FleetOps.Driver.Application.Drivers.Commands.ReserveDriver;
using FleetOps.Driver.Application.Drivers.Commands.SuspendDriver;
using FleetOps.Driver.Application.Drivers.Queries.CheckDriverEligibility;
using FleetOps.Driver.Application.Drivers.Queries.GetDriverById;
using FleetOps.Driver.Application.Drivers.Queries.GetDrivers;
using FleetOps.Driver.Domain.Drivers.Enums;
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

        [HttpGet("suspension-reasons")]
        public ActionResult<IReadOnlyList<EnumLookupResponse>> GetSuspensionReasons()
        {
            var reasons = Enum.GetValues<DriverSuspensionReason>()
                .Select(reason => new EnumLookupResponse(
                    (int)reason,
                    reason.ToString()))
                .ToList();

            return Ok(reasons);
        }


        [HttpPut("{id:guid}/suspend")]
        public async Task<ActionResult> Suspend(Guid id,SuspendDriverDto request,CancellationToken cancellationToken)
        {
            var result = await _sender.Send(
                new SuspendDriverCommand(id, request),
                cancellationToken);

            return HandleResult(result);
        }

        [HttpPut("{id:guid}/activate")]
        public async Task<ActionResult> Activate(Guid id,CancellationToken cancellationToken)
        {
            var result = await _sender.Send(
                new ActivateDriverCommand(id),
                cancellationToken);

            return HandleResult(result);
        }

        [HttpGet("{id:guid}/eligibility")]
        public async Task<ActionResult<DriverEligibilityResponse>> CheckEligibility(Guid id,CancellationToken cancellationToken)
        {
            var result = await _sender.Send(
                new CheckDriverEligibilityQuery(id),
                cancellationToken);

            return HandleResult(result);
        }

        [HttpPut("{id:guid}/reserve")]
        public async Task<ActionResult> Reserve(Guid id,CancellationToken cancellationToken)
        {
            var result = await _sender.Send(
                new ReserveDriverCommand(id),
                cancellationToken);

            return HandleResult(result);
        }
    }
}
