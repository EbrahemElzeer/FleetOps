using FleetOps.Driver.Domain.Common;
using MediatR;

namespace FleetOps.Driver.Application.Drivers.Commands.GoOffline
{
    public sealed record GoOfflineCommand(Guid DriverId) : IRequest<Result>;
}