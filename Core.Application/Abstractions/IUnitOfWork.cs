using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Application.Abstractions
{
    public interface IUnitOfWork
    {
        Task BeginTransactionAsync();

        /// <summary>
        /// ذخیره تغییرات و Commit تراکنش (در صورت وجود)
        /// </summary>
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
        Task<int> SaveChangesWithoutCommitAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Rollback تراکنش (در صورت وجود)
        /// </summary>
        Task RollbackAsync();

        /// <summary>
        /// Dispose همزمان DbContext و Transaction
        /// </summary>
        void Dispose();

        /// <summary>
        /// نسخه async Dispose
        /// </summary>
        ValueTask DisposeAsync();
    }
}
