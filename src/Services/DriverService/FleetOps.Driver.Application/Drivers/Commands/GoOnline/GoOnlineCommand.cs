using FleetOps.Driver.Domain.Common;
using MediatR;

namespace FleetOps.Driver.Application.Drivers.Commands.GoOnline
{
    public sealed record GoOnlineCommand(Guid DriverId) : IRequest<Result>;
}