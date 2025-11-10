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
    /*
     📌 EfUnitOfWork<TContext>
     -------------------------
     این کلاس پیاده‌سازی عمومی (Generic Implementation) برای الگوی **Unit of Work** با EF Core است.
     هدف آن مدیریت تراکنش‌ها، ذخیره‌سازی تغییرات، جمع‌آوری رویدادهای دامنه (Domain Events)
     و ثبت آن‌ها در Outbox برای پردازش غیرهمزمان است.

     ✅ نکات کلیدی:
     - Generic Parameters:
       • TContext → نوع DbContext که دیتابیس را مدیریت می‌کند.

     - وابستگی‌ها:
       • TContext _dbContext → کانتکست دیتابیس.
       • IOutboxService<TContext> _outboxService → سرویس Outbox برای ذخیره رویدادها.
       • ILogger<EfUnitOfWork<TContext>> _logger → لاگر برای ثبت رخدادها.
       • IDbContextTransaction? _transaction → تراکنش جاری (در صورت وجود).

     - متدها:
       • BeginTransactionAsync → شروع تراکنش جدید. اگر تراکنش فعال باشد، Exception پرتاب می‌شود.
       • SaveChangesAsync → 
         1. جمع‌آوری Domain Events از موجودیت‌ها.
         2. افزودن رویدادها به Outbox.
         3. ذخیره تغییرات در دیتابیس.
         4. Commit تراکنش (اگر فعال باشد).
         5. ثبت لاگ با تعداد تغییرات و رویدادها.
       • CollectDomainEvents → استخراج همه‌ی Domain Events از موجودیت‌های تغییر یافته و پاک‌سازی آن‌ها.
       • SaveChangesWithoutCommitAsync → ذخیره تغییرات و Outbox بدون Commit تراکنش (برای سناریوهای خاص).
       • RollbackAsync → بازگرداندن تراکنش در صورت وجود.
       • Dispose / DisposeAsync → آزادسازی منابع و تراکنش.
       • Context → دسترسی مستقیم به DbContext.

     🛠 جریان کار:
     1. سرویس Application یک عملیات آغاز می‌کند.
     2. BeginTransactionAsync فراخوانی می‌شود تا تراکنش شروع شود.
     3. تغییرات روی موجودیت‌ها اعمال می‌شوند.
     4. SaveChangesAsync فراخوانی می‌شود:
        • Domain Events جمع‌آوری و در Outbox ذخیره می‌شوند.
        • تغییرات در دیتابیس ذخیره می‌شوند.
        • تراکنش Commit می‌شود.
     5. اگر خطا رخ دهد، RollbackAsync فراخوانی می‌شود.
     6. در پایان، Dispose منابع را آزاد می‌کند.

     📌 نتیجه:
     این کلاس پایه‌ی مکانیزم **Unit of Work + Outbox Pattern Integration** در معماری ماژولار است
     و تضمین می‌کند که تغییرات و رویدادها به صورت اتمیک ذخیره شوند و سیستم قابلیت اطمینان بالایی داشته باشد.
    */

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
