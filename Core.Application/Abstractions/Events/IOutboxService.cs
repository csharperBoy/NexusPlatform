using Core.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Application.Abstractions.Events
{
    public interface IOutboxService<TDbContext>
         where TDbContext : Microsoft.EntityFrameworkCore.DbContext
    {
        Task AddEventsAsync(IEnumerable<IDomainEvent> domainEvents);
        Task<IEnumerable<OutboxMessage>> GetPendingMessagesAsync(int batchSize = 100);
        Task MarkAsProcessingAsync(System.Guid messageId);
        Task MarkAsCompletedAsync(System.Guid messageId);
        Task MarkAsFailedAsync(System.Guid messageId, string error);
        Task CleanupProcessedMessagesAsync(System.DateTime olderThan);
    }
}
