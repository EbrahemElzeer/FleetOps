using FleetOps.Driver.Domain.Common;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FleetOps.Driver.Application.Drivers.Commands.ProcessReleaseDriverRequested
{
    public sealed record ProcessReleaseDriverRequestedCommand(Guid DriverId, Guid OrderId, Guid MessageId) : IRequest<Result>
    {
    }

}
