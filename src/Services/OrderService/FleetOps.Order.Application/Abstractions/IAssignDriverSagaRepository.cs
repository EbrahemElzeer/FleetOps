using FleetOps.Order.Domain.Sagas;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FleetOps.Order.Application.Abstractions
{
    public interface IAssignDriverSagaRepository
    {
        Task AddAsync(
            AssignDriverSagaState saga,
            CancellationToken cancellationToken = default);

        Task<AssignDriverSagaState?> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default);
    }
}
