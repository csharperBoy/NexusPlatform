using Core.Application.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Infrastructure.Repositories
{
    public class EfUnitOfWork<TContext> : IUnitOfWork, IAsyncDisposable
         where TContext : DbContext
    {
        private readonly TContext _dbContext;
        private IDbContextTransaction? _transaction;

        public EfUnitOfWork(TContext dbContext)
        {
            _dbContext = dbContext;
        }

        /// <summary>
        /// شروع یک تراکنش جدید
        /// </summary>
        public async Task BeginTransactionAsync()
        {
            if (_transaction != null)
                throw new InvalidOperationException("A transaction is already in progress.");

            _transaction = await _dbContext.Database.BeginTransactionAsync();
        }

        /// <summary>
        /// ذخیره تغییرات و Commit تراکنش (در صورت وجود)
        /// </summary>
        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var result = await _dbContext.SaveChangesAsync(cancellationToken);

            if (_transaction != null)
            {
                await _transaction.CommitAsync(cancellationToken);
                await _transaction.DisposeAsync();
                _transaction = null;
            }

            return result;
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
                await _transaction.RollbackAsync();
                await _transaction.DisposeAsync();
                _transaction = null;
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
