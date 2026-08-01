using FleetOps.Driver.Domain.Common;
using MediatR;

namespace FleetOps.Driver.Application.Drivers.Commands.ActivateDriver
{
    public sealed record ActivateDriverCommand(Guid DriverId) : IRequest<Result>;
}