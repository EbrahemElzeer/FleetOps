using FleetOps.Driver.Domain.Common;
using MediatR;

namespace FleetOps.Driver.Application.Drivers.Commands.SuspendDriver
{
    public sealed record SuspendDriverCommand(Guid DriverId,SuspendDriverDto Dto) : IRequest<Result>;
}