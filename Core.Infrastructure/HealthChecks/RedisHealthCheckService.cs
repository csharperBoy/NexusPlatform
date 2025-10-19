using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Infrastructure.HealthChecks
{
    public class RedisHealthCheckService : IHealthCheckService
    {
        private readonly IDistributedCache _distributedCache;
        private readonly DbContext _dbContext;

        public RedisHealthCheckService(IDistributedCache distributedCache, DbContext dbContext)
        {
            _distributedCache = distributedCache;
            _dbContext = dbContext;
        }

        public async Task<SystemStatus> GetSystemStatusAsync()
        {
            var dbStatus = await GetDatabaseStatusAsync();
            var cacheStatus = await GetCacheStatusAsync();

            var isHealthy = dbStatus.IsConnected && cacheStatus.IsConnected;
            return new SystemStatus(isHealthy,
                isHealthy ? "Redis and Database operational" : "Service degradation",
                DateTime.UtcNow);
        }

        public async Task<DatabaseStatus> GetDatabaseStatusAsync()
        {
            // مشابه HealthCheckService
            try
            {
                var startTime = DateTime.UtcNow;
                var canConnect = await _dbContext.Database.CanConnectAsync();
                var responseTime = (DateTime.UtcNow - startTime).TotalMilliseconds;

                return new DatabaseStatus(canConnect,
                    canConnect ? "Database OK" : "Database unavailable",
                    (long)responseTime);
            }
            catch (Exception ex)
            {
                return new DatabaseStatus(false, ex.Message, 0);
            }
        }

        public async Task<CacheStatus> GetCacheStatusAsync()
        {
            try
            {
                var testKey = "redis_health_check";
                var testValue = "redis_test_value";
                var startTime = DateTime.UtcNow;

                // تست Redis-specific
                await _distributedCache.SetStringAsync(testKey, testValue,
                    new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(5) });

                var result = await _distributedCache.GetStringAsync(testKey);
                var responseTime = (DateTime.UtcNow - startTime).TotalMilliseconds;

                var isHealthy = result == testValue;
                return new CacheStatus(isHealthy,
                    isHealthy ? "Redis connected" : "Redis connection failed",
                    (long)responseTime);
            }
            catch (Exception ex)
            {
                return new CacheStatus(false, $"Redis error: {ex.Message}", 0);
            }
        }
    }
}
