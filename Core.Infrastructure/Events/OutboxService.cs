using Core.Application.Abstractions.Events;
using Core.Domain.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Infrastructure.Events
{
    public class OutboxService<TDbContext> : IOutboxService<TDbContext>
        where TDbContext : DbContext
    {
        private readonly TDbContext _dbContext;
        private readonly ILogger<OutboxService<TDbContext>> _logger;

        public OutboxService(TDbContext dbContext, ILogger<OutboxService<TDbContext>> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task AddEventsAsync(IEnumerable<IDomainEvent> domainEvents)
        {
            if (domainEvents == null || !domainEvents.Any())
                return;

            var outboxMessages = domainEvents.Select(e => new OutboxMessage(e));
            await _dbContext.Set<OutboxMessage>().AddRangeAsync(outboxMessages);

            _logger.LogDebug("Added {Count} events to outbox for {DbContext}",
                outboxMessages.Count(), typeof(TDbContext).Name);
        }

        public async Task<IEnumerable<OutboxMessage>> GetPendingMessagesAsync(int batchSize = 100)
        {
            return await _dbContext.Set<OutboxMessage>()
                .Where(x => x.Status == OutboxMessageStatus.Pending)
                .OrderBy(x => x.OccurredOn)
                .Take(batchSize)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task MarkAsProcessingAsync(Guid messageId)
        {
            var message = await _dbContext.Set<OutboxMessage>().FindAsync(messageId);
            if (message != null)
            {
                message.MarkAsProcessing();
                await _dbContext.SaveChangesAsync();
            }
        }

        public async Task MarkAsCompletedAsync(Guid messageId)
        {
            var message = await _dbContext.Set<OutboxMessage>().FindAsync(messageId);
            if (message != null)
            {
                message.MarkAsCompleted();
                await _dbContext.SaveChangesAsync();
            }
        }

        public async Task MarkAsFailedAsync(Guid messageId, string error)
        {
            var message = await _dbContext.Set<OutboxMessage>().FindAsync(messageId);
            if (message != null)
            {
                message.MarkAsFailed(error);
                await _dbContext.SaveChangesAsync();
            }
        }

        public async Task CleanupProcessedMessagesAsync(DateTime olderThan)
        {
            var deletedCount = await _dbContext.Set<OutboxMessage>()
                .Where(x => x.Status == OutboxMessageStatus.Completed &&
                           x.ProcessedOn < olderThan)
                .ExecuteDeleteAsync();

            _logger.LogInformation("Cleaned up {Count} processed outbox messages older than {OlderThan}",
                deletedCount, olderThan);
        }
    }
}
