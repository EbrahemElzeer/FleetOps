using FleetOps.Driver.Domain.Common;
using MediatR;

namespace FleetOps.Driver.Application.Drivers.Commands.ReleaseDriver
{
    public sealed record ReleaseDriverCommand(Guid DriverId) : IRequest<Result>;
}