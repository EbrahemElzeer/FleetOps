using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FleetOps.Order.Domain.Sagas.Enums
{
    public enum AssignDriverSagaStatus
    {
        Started = 1,
        DriverReserved = 2,
        Completed = 3,
        CompensationPending = 4,
        Compensated = 5,
        Failed = 6
    }
}
