using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FleetOps.Driver.Domain.Drivers.Enums
{
    public enum DriverStatus
    {
        Offline = 1,
        Available = 2,
        Assigned = 3,
        OnTrip = 4,
        Returning = 5,
        Suspended = 6
    }
}
