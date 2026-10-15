using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FleetOps.Order.Application.IntegrationEvents.Drivers
{
    namespace FleetOps.Order.Application.IntegrationEvents.Drivers
    {
        public sealed record ReleaseDriverRequested(
            Guid SagaId,
            Guid OrderId,
            Guid DriverId);
    }
}
