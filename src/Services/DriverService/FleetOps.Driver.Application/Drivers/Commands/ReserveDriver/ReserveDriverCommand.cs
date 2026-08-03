using FleetOps.Driver.Domain.Common;
using MediatR;

namespace FleetOps.Driver.Application.Drivers.Commands.ReserveDriver
{
    public sealed record ReserveDriverCommand(Guid DriverId) : IRequest<Result>;
}