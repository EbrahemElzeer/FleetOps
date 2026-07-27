using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FleetOps.Driver.Domain.Drivers.Enums
{
    public enum DriverSuspensionReason
    {
        PolicyViolation = 1,
        ExpiredLicense = 2,
        VehicleIssue = 3,
        CustomerComplaint = 4,
        SafetyConcern = 5,
        OperationalDecision = 6,
        Other = 7
    }
}
