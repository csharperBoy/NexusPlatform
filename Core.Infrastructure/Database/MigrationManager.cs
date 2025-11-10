// Core/Infrastructure/Database/MigrationManager.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.DependencyInjection; // استفاده از Microsoft.Data.SqlClient
namespace Core.Infrastructure.Database
{
    /*
     📌 MigrationManager
     -------------------
     این کلاس پیاده‌سازی اینترفیس IMigrationManager است و مسئول مدیریت **Database Migrations**
     در EF Core می‌باشد. هدف آن اجرای خودکار Migrationها، مدیریت خطاها و افزایش قابلیت اطمینان
     در فرآیند به‌روزرسانی دیتابیس است.

     ✅ نکات کلیدی:
     - وابستگی‌ها:
       • IServiceProvider → برای ساخت Scope و Resolve کردن DbContext.
       • ILogger<MigrationManager> → برای لاگ‌گذاری وضعیت و خطاها.

     - MigrateAsync<TContext>:
       • اجرای Migrationها با مکانیزم Retry (حداکثر ۳ بار).
       • مدیریت خطاها با دسته‌بندی (ErrorType).
       • در صورت خطاهای موقت (Transient) → Retry با Delay افزایشی (Exponential Backoff).
       • در صورت خطاهای غیرقابل رفع (AlreadyExists, Permission) → عدم Retry.

     - AttemptMigration<TContext>:
       • بررسی اتصال به دیتابیس (CanConnectAsync).
       • در صورت عدم دسترسی → ایجاد دیتابیس جدید (EnsureCreatedAsync).
       • بررسی Migrationهای Pending.
       • اجرای MigrateAsync در صورت وجود Migrationهای جدید.
       • لاگ‌گذاری وضعیت قبل و بعد از اعمال Migrationها.

     - ClassifyError(Exception ex):
       • دسته‌بندی خطاها به Transient, AlreadyExists, Permission, Unknown.

     - ShouldRetry / CalculateDelay:
       • تصمیم‌گیری برای Retry بر اساس نوع خطا.
       • محاسبه Delay مناسب برای Retry.

     - HasPendingMigrationsAsync<TContext>:
       • بررسی وجود Migrationهای اعمال‌نشده.
       • در صورت عدم اتصال یا خطا → true برمی‌گرداند (یعنی دیتابیس نیاز به بررسی دارد).

     - FinalMigrationAttempt<TContext>:
       • آخرین تلاش برای اجرای Migrationها پس از شکست Retryها.
       • اگر باز هم خطا رخ دهد، لاگ Error ثبت می‌شود.

     - IsTransientError(Exception ex):
       • بررسی خطاهای موقت (مثل Timeout, Network, Deadlock).
       • بررسی کدهای خطای SQL Server که معمولاً موقت هستند.

     🛠 جریان کار:
     1. در زمان راه‌اندازی برنامه، MigrateAsync فراخوانی می‌شود.
     2. اگر دیتابیس در دسترس نباشد → ایجاد می‌شود.
     3. اگر Migrationهای Pending وجود داشته باشد → اعمال می‌شوند.
     4. در صورت خطا، Retry با Delay مناسب انجام می‌شود.
     5. در نهایت، اگر همه تلاش‌ها شکست بخورد → FinalMigrationAttempt اجرا می‌شود.

     📌 نتیجه:
     این کلاس پایه‌ی مکانیزم **Resilient Database Migration** در معماری ماژولار است
     و تضمین می‌کند که دیتابیس همیشه با مدل دامنه همگام باشد، حتی در شرایط خطاهای موقت.
    */

    public class MigrationManager : IMigrationManager
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<MigrationManager> _logger;

        public MigrationManager(IServiceProvider serviceProvider, ILogger<MigrationManager> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        public async Task MigrateAsync<TContext>(CancellationToken cancellationToken = default) where TContext : DbContext
        {
            const int maxRetries = 3;
            int retryCount = 0;

            while (retryCount < maxRetries)
            {
                try
                {
                    await AttemptMigration<TContext>(cancellationToken);
                    return;
                }
                catch (Exception ex)
                {
                    retryCount++;
                    var errorType = ClassifyError(ex);
                    var shouldRetry = ShouldRetry(errorType, retryCount, maxRetries);

                    if (shouldRetry)
                    {
                        var delay = CalculateDelay(errorType, retryCount);
                        _logger.LogWarning(ex,
                            "⚠️ {ErrorType} error during migration (Attempt {RetryCount}/{MaxRetries}). Retrying in {Delay}s",
                            errorType, retryCount, maxRetries, delay.TotalSeconds);

                        await Task.Delay(delay, cancellationToken);
                    }
                    else
                    {
                        _logger.LogError(ex, "❌ {ErrorType} error - no more retries for {DbContext}",
                            errorType, typeof(TContext).Name);
                        break;
                    }
                }
            }

            await FinalMigrationAttempt<TContext>(cancellationToken);
        }

        private async Task AttemptMigration<TContext>(CancellationToken cancellationToken) where TContext : DbContext
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<TContext>();

            _logger.LogInformation("🔄 Checking migrations for {DbContext}...", typeof(TContext).Name);

            if (!await context.Database.CanConnectAsync(cancellationToken))
            {
                _logger.LogWarning("Database not accessible. Attempting to create...");
                await context.Database.EnsureCreatedAsync(cancellationToken);
                _logger.LogInformation("✅ Database created successfully.");
                return;
            }

            var pendingMigrations = await context.Database.GetPendingMigrationsAsync(cancellationToken);
            var pendingList = pendingMigrations.ToList();

            if (!pendingList.Any())
            {
                _logger.LogInformation("✅ No pending migrations for {DbContext}.", typeof(TContext).Name);
                return;
            }

            _logger.LogInformation("📦 Found {Count} pending migrations for {DbContext}",
                pendingList.Count, typeof(TContext).Name);

            await context.Database.MigrateAsync(cancellationToken);

            _logger.LogInformation("🎉 Successfully applied {Count} migrations for {DbContext}",
                pendingList.Count, typeof(TContext).Name);
        }

        // 📌 دسته‌بندی خطاها
        private ErrorType ClassifyError(Exception ex)
        {
            if (IsTransientError(ex))
                return ErrorType.Transient;
            else if (ex.Message.Contains("There is already an object ", StringComparison.OrdinalIgnoreCase))
                return ErrorType.AlreadyExists;
            else if (ex.Message.Contains("permission", StringComparison.OrdinalIgnoreCase))
                return ErrorType.Permission;
            else
                return ErrorType.Unknown;
        }

        private bool ShouldRetry(ErrorType errorType, int retryCount, int maxRetries)
        {
            return errorType switch
            {
                ErrorType.Transient => retryCount < maxRetries,
                ErrorType.AlreadyExists => false,
                ErrorType.Permission => retryCount < 1,
                ErrorType.Unknown => retryCount < maxRetries - 1,
                _ => retryCount < maxRetries
            };
        }

        private TimeSpan CalculateDelay(ErrorType errorType, int retryCount)
        {
            return errorType switch
            {
                ErrorType.Transient => TimeSpan.FromSeconds(Math.Pow(2, retryCount)),
                ErrorType.Permission => TimeSpan.FromSeconds(5),
                ErrorType.Unknown => TimeSpan.FromSeconds(Math.Pow(3, retryCount)),
                _ => TimeSpan.FromSeconds(Math.Pow(2, retryCount))
            };
        }

        private enum ErrorType
        {
            Transient,
            AlreadyExists,
            Permission,
            Unknown
        }

        public async Task<bool> HasPendingMigrationsAsync<TContext>(CancellationToken cancellationToken = default) where TContext : DbContext
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<TContext>();

                if (!await context.Database.CanConnectAsync(cancellationToken))
                    return true;

                var pendingMigrations = await context.Database.GetPendingMigrationsAsync(cancellationToken);
                return pendingMigrations.Any();
            }
            catch
            {
                return true;
            }
        }

        private async Task FinalMigrationAttempt<TContext>(CancellationToken cancellationToken) where TContext : DbContext
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<TContext>();
                await context.Database.MigrateAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "💥 Final migration attempt failed for {DbContext}", typeof(TContext).Name);
            }
        }

        private static bool IsTransientError(Exception ex)
        {
            if (ex is SqlException sqlEx)
            {
                int[] transientErrors = { -2, 20, 64, 233, 1205, 11001, 4060, 18456, 40197, 40501 };
                return transientErrors.Contains(sqlEx.Number);
            }

            return ex.Message.Contains("timeout", StringComparison.OrdinalIgnoreCase) ||
                   ex.Message.Contains("network", StringComparison.OrdinalIgnoreCase) ||
                   ex.Message.Contains("connection", StringComparison.OrdinalIgnoreCase) ||
                   ex.Message.Contains("deadlock", StringComparison.OrdinalIgnoreCase);
        }
    }
}
