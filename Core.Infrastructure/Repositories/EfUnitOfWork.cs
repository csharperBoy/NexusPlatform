using Core.Application.Abstractions;
using Core.Application.Abstractions.Events;
using Core.Domain.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Infrastructure.Repositories
{
    public class EfUnitOfWork<TContext> : IUnitOfWork<TContext>
       where TContext : DbContext
    {
        private readonly TContext _dbContext;
        private readonly IOutboxService<TContext> _outboxService;
        private readonly ILogger<EfUnitOfWork<TContext>> _logger;
        private IDbContextTransaction? _transaction;

        public EfUnitOfWork(
            TContext dbContext,
            IOutboxService<TContext> outboxService,
            ILogger<EfUnitOfWork<TContext>> logger)
        {
            _dbContext = dbContext;
            _outboxService = outboxService;
            _logger = logger;
        }

        public async Task BeginTransactionAsync()
        {
            if (_transaction != null)
            {
                _logger.LogWarning("Transaction already in progress");
                throw new InvalidOperationException("A transaction is already in progress.");
            }

            _logger.LogDebug("Beginning database transaction");
            _transaction = await _dbContext.Database.BeginTransactionAsync();
        }

        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var domainEvents = CollectDomainEvents();

            if (domainEvents.Any())
                await _outboxService.AddEventsAsync(domainEvents);

            var result = await _dbContext.SaveChangesAsync(cancellationToken);

            if (_transaction != null)
            {
                await _transaction.CommitAsync(cancellationToken);
                await _transaction.DisposeAsync();
                _transaction = null;
            }
            else
            {
                _logger.LogDebug("No active transaction; changes and outbox saved atomically by EF.");
            }

            _logger.LogInformation(
                "Saved {Count} changes with {EventCount} events in outbox for {DbContext}",
                result, domainEvents.Count, typeof(TContext).Name);

            return result;
        }

        private List<IDomainEvent> CollectDomainEvents()
        {
            var domainEvents = _dbContext.ChangeTracker.Entries<BaseEntity>()
                .SelectMany(x => x.Entity.DomainEvents)
                .ToList();

            _dbContext.ChangeTracker.Entries<BaseEntity>()
                .ToList()
                .ForEach(entity => entity.Entity.ClearDomainEvents());

            _logger.LogDebug("Collected {EventCount} domain events", domainEvents.Count);

            return domainEvents;
        }

        public async Task<int> SaveChangesWithoutCommitAsync(CancellationToken cancellationToken = default)
        {
            var domainEvents = CollectDomainEvents();

            if (domainEvents.Any())
                await _outboxService.AddEventsAsync(domainEvents);

            return await _dbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task RollbackAsync()
        {
            if (_transaction != null)
            {
                _logger.LogWarning("Rolling back database transaction");
                await _transaction.RollbackAsync();
                await _transaction.DisposeAsync();
                _transaction = null;
            }
            else
            {
                _logger.LogDebug("No transaction to rollback");
            }
        }

        public void Dispose()
        {
            _transaction?.Dispose();
            _dbContext.Dispose();
        }

        public async ValueTask DisposeAsync()
        {
            if (_transaction != null)
            {
                await _transaction.DisposeAsync();
                _transaction = null;
            }
            await _dbContext.DisposeAsync();
        }

        public TContext Context => _dbContext;
    }
}