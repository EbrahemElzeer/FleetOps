using FleetOps.Driver.Domain.Inbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FleetOps.Driver.Application.Abstractions
{
    public interface IInboxRepository
    {
        Task<bool> ExistsAsync(Guid messageId,CancellationToken cancellationToken);
        Task AddAsync(InboxMessage message,CancellationToken cancellationToken);    
    }
}
