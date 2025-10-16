using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Application.Abstractions
{
    public interface IUnitOfWork<TContext> : IDisposable, IAsyncDisposable
    where TContext : DbContext
    {
        /// <summary>
        /// شروع یک تراکنش جدید
        /// </summary>
        Task BeginTransactionAsync();

        /// <summary>
        /// ذخیره تغییرات و Commit تراکنش (در صورت وجود)
        /// </summary>
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// ذخیره تغییرات بدون Commit تراکنش
        /// </summary>
        Task<int> SaveChangesWithoutCommitAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Rollback تراکنش (در صورت وجود)
        /// </summary>
        Task RollbackAsync();

        /// <summary>
        /// دسترسی مستقیم به DbContext (در صورت نیاز)
        /// </summary>
        TContext Context { get; }
    }

}
