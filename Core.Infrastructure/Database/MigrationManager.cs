// Core/Infrastructure/Database/MigrationManager.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.DependencyInjection;
using Core.Infrastructure.Data;

namespace Core.Infrastructure.Database
{
    public class MigrationManager : IMigrationManager
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<MigrationManager> _logger;

        public MigrationManager(IServiceProvider serviceProvider, ILogger<MigrationManager> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        public async Task MigrateAsync<TContext>(CancellationToken cancellationToken = default) where TContext : DbContext, IBase_DbContext
        {
            const int maxRetries = 3;
            int retryCount = 0;

            while (true) // حلقه تا زمانی که موفق شود یا Exception پرتاب کند ادامه دارد
            {
                try
                {
                    await AttemptMigration<TContext>(cancellationToken);
                    return; // مهاجرت موفقیت‌آمیز بود، خروج از متد
                }
                catch (Exception ex)
                {
                    var errorType = ClassifyError(ex);

                    // بررسی اینکه آیا اصلاً مجاز به Retry هستیم و آیا تلاش‌هایمان تمام شده است؟
                    if (retryCount >= maxRetries || !ShouldRetry(errorType, retryCount, maxRetries))
                    {
                        _logger.LogCritical(ex, "❌ {ErrorType} error - Migration definitively failed for {DbContext} after {RetryCount} retries. App startup will abort.",
                            errorType, typeof(TContext).Name, retryCount);

                        // ⚠️ پرتاب مجدد خطا بسیار مهم است تا برنامه با دیتابیس ناقص بالا نیاید!
                        throw;
                    }

                    retryCount++;
                    var delay = CalculateDelay(errorType, retryCount);

                    _logger.LogWarning(ex, "⚠️ {ErrorType} error during migration. Retrying in {Delay}s (Attempt {RetryCount}/{MaxRetries})",
                        errorType, delay.TotalSeconds, retryCount, maxRetries);

                    await Task.Delay(delay, cancellationToken);
                }
            }
        }

        private async Task AttemptMigration<TContext>(CancellationToken cancellationToken) where TContext : DbContext, IBase_DbContext
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<TContext>();

            _logger.LogInformation("🔄 Starting database check and migration for {DbContext}...", typeof(TContext).Name);

            // لاگ‌گذاری هوشمندانه وضعیت دیتابیس قبل از اعمال تغییرات
            bool dbExists = await context.Database.CanConnectAsync(cancellationToken);
            if (dbExists)
            {
                var pendingMigrations = await context.Database.GetPendingMigrationsAsync(cancellationToken);
                var pendingList = pendingMigrations.ToList();
                if (pendingList.Any())
                {
                    _logger.LogInformation("📦 Found {Count} pending migrations for {DbContext}.", pendingList.Count, typeof(TContext).Name);
                }
                else
                {
                    _logger.LogInformation("✅ Database for {DbContext} is already up to date.", typeof(TContext).Name);
                }
            }
            else
            {
                _logger.LogInformation("🚧 Database for {DbContext} not found. It will be created now...", typeof(TContext).Name);
            }

            // متد MigrateAsync خودش کار Create دیتابیس و اجرای Pending ها را با هم انجام می‌دهد.
            await context.Database.MigrateAsync(cancellationToken);

            // اجرای ویوها و تریگرها (فقط یک‌بار نوشته می‌شود)
            context.EnsureTriggers(cancellationToken);
            context.EnsureViews(cancellationToken);

            _logger.LogInformation("🎉 Database setup completed successfully for {DbContext}.", typeof(TContext).Name);
        }

        public async Task<bool> HasPendingMigrationsAsync<TContext>(CancellationToken cancellationToken = default) where TContext : DbContext, IBase_DbContext
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

        private ErrorType ClassifyError(Exception ex)
        {
            if (IsTransientError(ex)) return ErrorType.Transient;

            var msg = ex.Message;
            if (msg.Contains("There is already an object", StringComparison.OrdinalIgnoreCase)) return ErrorType.AlreadyExists;
            if (msg.Contains("permission", StringComparison.OrdinalIgnoreCase)) return ErrorType.Permission;

            return ErrorType.Unknown;
        }

        private bool ShouldRetry(ErrorType errorType, int retryCount, int maxRetries)
        {
            return errorType switch
            {
                ErrorType.Transient => true,
                // در محیط‌های ماژولار و میکروسرویس، خطای AlreadyExists معمولاً به خاطر اجرای همزمان است.
                // اگر یک بار Retry کنیم، ممکن است سرویس دیگر کار را تمام کرده باشد و متد ما با موفقیت رد شود.
                ErrorType.AlreadyExists => retryCount < 1,
                ErrorType.Permission => false, // خطای پرمیشن با زمان دادن حل نمی‌شود
                ErrorType.Unknown => true,
                _ => false
            };
        }

        private TimeSpan CalculateDelay(ErrorType errorType, int retryCount)
        {
            return errorType switch
            {
                ErrorType.Transient => TimeSpan.FromSeconds(Math.Pow(2, retryCount)),
                ErrorType.AlreadyExists => TimeSpan.FromSeconds(2),
                ErrorType.Unknown => TimeSpan.FromSeconds(Math.Pow(3, retryCount)),
                _ => TimeSpan.FromSeconds(2)
            };
        }

        private static bool IsTransientError(Exception ex)
        {
            if (ex is SqlException sqlEx)
            {
                int[] transientErrors = { -2, 20, 64, 233, 1205, 11001, 4060, 18456, 40197, 40501 };
                return transientErrors.Contains(sqlEx.Number);
            }

            var msg = ex.Message;
            return msg.Contains("timeout", StringComparison.OrdinalIgnoreCase) ||
                   msg.Contains("network", StringComparison.OrdinalIgnoreCase) ||
                   msg.Contains("connection", StringComparison.OrdinalIgnoreCase) ||
                   msg.Contains("deadlock", StringComparison.OrdinalIgnoreCase);
        }

        private enum ErrorType
        {
            Transient,
            AlreadyExists,
            Permission,
            Unknown
        }
    }
}
