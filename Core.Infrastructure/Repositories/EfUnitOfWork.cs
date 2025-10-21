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
        private readonly IEventBus _eventBus;
        private IDbContextTransaction? _transaction;
        private List<IDomainEvent> _domainEvents = new();
        private readonly ILogger<EfUnitOfWork<TContext>> _logger;
        public EfUnitOfWork(TContext dbContext, IEventBus eventBus, ILogger<EfUnitOfWork<TContext>> logger)
        {
            _dbContext = dbContext;
            _eventBus = eventBus;
            _logger = logger;
        }


        /// <summary>
        /// شروع یک تراکنش جدید
        /// </summary>
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

        /// <summary>
        /// ذخیره تغییرات و Commit تراکنش (در صورت وجود)
        /// </summary>
        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            // 1. جمع‌آوری events از entities
            CollectDomainEvents();

            // 2. ذخیره تغییرات در دیتابیس
            var result = await _dbContext.SaveChangesAsync(cancellationToken);

            // 3. commit تراکنش
            if (_transaction != null)
            {
                await _transaction.CommitAsync(cancellationToken);
                await _transaction.DisposeAsync();
                _transaction = null;
            }

            // 4. فقط بعد از موفقیت‌آمیز بودن commit، events رو publish کنیم
            try
            {
                await PublishDomainEvents();
            }
            catch (Exception ex)
            {
                // اگر event publishing fail شد، سیستم نباید crash کند
                // چون داده‌ها قبلاً commit شده‌اند
                _logger.LogError(ex, "Error publishing domain events after transaction commit");
                // می‌توانید به سیستم monitoring/reporting اطلاع دهید
            }

            return result;
        }
        private void CollectDomainEvents()
        {
            _domainEvents = _dbContext.ChangeTracker.Entries<BaseEntity>()
          .SelectMany(x => x.Entity.DomainEvents)
          .ToList();

            // پاک کردن events از entities
            _dbContext.ChangeTracker.Entries<BaseEntity>()
                .ToList()
                .ForEach(entity => entity.Entity.ClearDomainEvents());

            _logger.LogDebug("Collected {EventCount} domain events", _domainEvents.Count);
        }

        private async Task PublishDomainEvents()
        {
            if (_domainEvents.Any())
            {
                _logger.LogInformation("Publishing {EventCount} domain events", _domainEvents.Count);

                foreach (var domainEvent in _domainEvents)
                {
                    _logger.LogDebug("Publishing event: {EventType}", domainEvent.GetType().Name);
                    await _eventBus.PublishAsync(domainEvent);
                }
                _domainEvents.Clear();
            }
        }
        public async Task<int> SaveChangesWithoutCommitAsync(CancellationToken cancellationToken = default)
        {
            var result = await _dbContext.SaveChangesAsync(cancellationToken);

            return result;
        }
        /// <summary>
        /// Rollback تراکنش (در صورت وجود)
        /// </summary>
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

        /// <summary>
        /// Dispose همزمان DbContext و Transaction
        /// </summary>
        public void Dispose()
        {
            _transaction?.Dispose();
            _dbContext.Dispose();
        }

        /// <summary>
        /// نسخه async Dispose
        /// </summary>
        public async ValueTask DisposeAsync()
        {
            if (_transaction != null)
            {
                await _transaction.DisposeAsync();
                _transaction = null;
            }
            await _dbContext.DisposeAsync();
        }

        /// <summary>
        /// دسترسی به DbContext برای Repositoryها
        /// </summary>
        public TContext Context => _dbContext;
    }
}
