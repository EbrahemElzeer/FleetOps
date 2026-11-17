using FleetOps.Driver.Application.Abstractions;
using FleetOps.Driver.Domain.Inbox;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FleetOps.Driver.Infrastructure.Persistence.Repositories
{
    public class InboxRepository : IInboxRepository
    {
        private readonly DriverDbContext _context;

        public InboxRepository(DriverDbContext context)
        {
            _context = context;
        }
        public async Task AddAsync(InboxMessage message, CancellationToken cancellationToken)
        {
            await _context.InboxMessages.AddAsync(message, cancellationToken);
        }

        public async Task<bool> ExistsAsync(Guid messageId, CancellationToken cancellationToken)
        {
            return await _context.InboxMessages.AnyAsync(m => m.MessageId == messageId, cancellationToken);
        }
    }
}
