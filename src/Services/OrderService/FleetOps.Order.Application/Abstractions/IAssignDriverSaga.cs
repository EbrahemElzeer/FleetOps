using FleetOps.Order.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FleetOps.Order.Application.Abstractions
{
    public interface IAssignDriverSaga
    {
        Task<Result> ExecuteAsync(
            Domain.Orders.Order order ,
            Guid driverId,
            CancellationToken cancellationToken = default);
    }
}
