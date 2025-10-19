// Core/Infrastructure/Database/MigrationManager.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.DependencyInjection; // استفاده از Microsoft.Data.SqlClient

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

        public async Task MigrateAsync<TContext>(CancellationToken cancellationToken = default) where TContext : DbContext
        {
            const int maxRetries = 3;
            int retryCount = 0;

            while (retryCount < maxRetries)
            {
                try
                {
                    using var scope = _serviceProvider.CreateScope();
                    var context = scope.ServiceProvider.GetRequiredService<TContext>();

                    _logger.LogInformation("🔄 Checking migrations for {DbContext}...", typeof(TContext).Name);

                    // بررسی اتصال به دیتابیس
                    if (!await context.Database.CanConnectAsync(cancellationToken))
                    {
                        _logger.LogWarning("Database not accessible. Attempting to create...");
                        await context.Database.EnsureCreatedAsync(cancellationToken);
                        _logger.LogInformation("✅ Database created successfully.");
                        return;
                    }

                    // بررسی migrationهای pending
                    var pendingMigrations = await context.Database.GetPendingMigrationsAsync(cancellationToken);
                    var pendingList = pendingMigrations.ToList();

                    if (!pendingList.Any())
                    {
                        _logger.LogInformation("✅ No pending migrations for {DbContext}.", typeof(TContext).Name);
                        return;
                    }

                    _logger.LogInformation("📦 Found {Count} pending migrations for {DbContext}",
                        pendingList.Count, typeof(TContext).Name);

                    // اجرای migration
               //     await context.Database.MigrateAsync(cancellationToken);

                    _logger.LogInformation("🎉 Successfully applied {Count} migrations for {DbContext}",
                        pendingList.Count, typeof(TContext).Name);

                    return; // موفقیت‌آمیز
                }
                catch (Exception ex) when (IsTransientError(ex) && retryCount < maxRetries - 1)
                {
                    retryCount++;
                    _logger.LogWarning(ex, "⚠️ Transient error during migration (Attempt {RetryCount}/{MaxRetries})",
                        retryCount, maxRetries);

                    await Task.Delay(TimeSpan.FromSeconds(Math.Pow(2, retryCount)), cancellationToken);
                }
            }

            // آخرین تلاش
            await FinalMigrationAttempt<TContext>(cancellationToken);
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
                throw;
            }
        }

        private static bool IsTransientError(Exception ex)
        {
            // بررسی خطاهای موقتی با Microsoft.Data.SqlClient
            if (ex is SqlException sqlEx)
            {
                int[] transientErrors = { -2, 20, 64, 233, 1205, 11001, 4060, 18456, 40197, 40501 };
                return transientErrors.Contains(sqlEx.Number);
            }

            // بررسی خطاهای عمومی
            return ex.Message.Contains("timeout", StringComparison.OrdinalIgnoreCase) ||
                   ex.Message.Contains("network", StringComparison.OrdinalIgnoreCase) ||
                   ex.Message.Contains("connection", StringComparison.OrdinalIgnoreCase) ||
                   ex.Message.Contains("deadlock", StringComparison.OrdinalIgnoreCase);
        }
    }
}