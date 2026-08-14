using FleetOps.Order.Application.Abstractions;
using FleetOps.Order.Domain.Sagas;
using FleetOps.Order.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FleetOps.Order.Infrastructure.Repositories
{
    public sealed class AssignDriverSagaRepository: IAssignDriverSagaRepository
    {
        private readonly OrderDbContext _dbContext;

        public AssignDriverSagaRepository(OrderDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task AddAsync(
            AssignDriverSagaState saga,
            CancellationToken cancellationToken = default)
        {
            await _dbContext.AssignDriverSagaState.AddAsync(
                saga,
                cancellationToken);
        }

        public async Task<AssignDriverSagaState?> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            return await _dbContext.AssignDriverSagaState
                .FirstOrDefaultAsync(
                    x => x.Id == id,
                    cancellationToken);
        }
    }
}
