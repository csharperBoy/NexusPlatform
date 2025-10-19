using Core.Infrastructure.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Infrastructure.Database
{
    public class HealthAwareMigrationManager : IMigrationManager
    {
        private readonly IMigrationManager _inner;
        private readonly IHealthCheckService _healthCheck;
        private readonly ILogger<HealthAwareMigrationManager> _logger;

        public HealthAwareMigrationManager(IMigrationManager inner, IHealthCheckService healthCheck, ILogger<HealthAwareMigrationManager> logger)
        {
            _inner = inner;
            _healthCheck = healthCheck;
            _logger = logger;
        }

        public async Task MigrateAsync<TContext>(CancellationToken cancellationToken = default) where TContext : DbContext
        {
            // بررسی سلامت سیستم قبل از migration
            var healthStatus = await _healthCheck.GetDatabaseStatusAsync();

            if (!healthStatus.IsConnected)
            {
                _logger.LogWarning("Database health check failed before migration: {Message}", healthStatus.Message);

                // اگر در Production هستیم و دیتابیس مشکل داره، migration رو به تعویق بنداز
                if (IsProductionEnvironment())
                {
                    _logger.LogError("Cannot run migrations in production with unhealthy database. Aborting.");
                    throw new InvalidOperationException($"Database unhealthy: {healthStatus.Message}");
                }
            }

            await _inner.MigrateAsync<TContext>(cancellationToken);

            // بررسی سلامت بعد از migration
            var postHealthStatus = await _healthCheck.GetDatabaseStatusAsync();
            _logger.LogInformation("Post-migration health: {Status} ({ResponseTime}ms)",
                postHealthStatus.Message, postHealthStatus.ResponseTimeMs);
        }

        private static bool IsProductionEnvironment()
        {
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            return environment?.Equals("Production", StringComparison.OrdinalIgnoreCase) == true;
        }

        public Task<bool> HasPendingMigrationsAsync<TContext>(CancellationToken cancellationToken = default) where TContext : DbContext
        {
            throw new NotImplementedException();
        }

        public Task EnsureDatabaseCreatedAsync<TContext>(CancellationToken cancellationToken = default) where TContext : DbContext
        {
            throw new NotImplementedException();
        }
    }
}
