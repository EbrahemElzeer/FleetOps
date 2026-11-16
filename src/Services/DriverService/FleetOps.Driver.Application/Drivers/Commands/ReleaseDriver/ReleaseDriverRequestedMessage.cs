using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FleetOps.Driver.Application.Drivers.Commands.ReleaseDriver
{
    public sealed record ReleaseDriverRequestedMessage(
     Guid SagaId,
     Guid OrderId,
     Guid DriverId);
}
