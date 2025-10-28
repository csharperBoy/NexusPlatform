using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Core.Infrastructure.HealthChecks
{
    public class DatabaseHealthCheck<TDbContext> : IHealthCheck
        where TDbContext : DbContext
    {
        private readonly TDbContext _dbContext;

        public DatabaseHealthCheck(TDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var sw = Stopwatch.StartNew();
                var canConnect = await _dbContext.Database.CanConnectAsync(cancellationToken);
                sw.Stop();

                if (canConnect)
                {
                    return HealthCheckResult.Healthy(
                        $"Database connection successful (ResponseTime: {sw.ElapsedMilliseconds} ms)");
                }
                else
                {
                    return HealthCheckResult.Unhealthy("Database connection failed");
                }
            }
            catch (Exception ex)
            {
                return HealthCheckResult.Unhealthy($"Database error: {ex.Message}", ex);
            }
        }
    }
}
