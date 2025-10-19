using Core.Application.Abstractions.Caching;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Infrastructure.HealthChecks
{
    public class HealthCheckService : IHealthCheckService
    {
        //private readonly DbContext _dbContext;
        private readonly ICacheService _cacheService;

        public HealthCheckService(//DbContext dbContext,
            ICacheService cacheService)
        {
         //   _dbContext = dbContext;
            _cacheService = cacheService;
        }

        public async Task<SystemStatus> GetSystemStatusAsync()
        {
            //var dbStatus = await GetDatabaseStatusAsync();
            var cacheStatus = await GetCacheStatusAsync();

            var isHealthy = //dbStatus.IsConnected && 
                cacheStatus.IsConnected;
            var message = isHealthy ?
                "All systems operational" :
               // $"Issues detected: DB: " +
           //     $"{dbStatus.IsConnected}," +
                $" Cache: {cacheStatus.IsConnected}";

            return new SystemStatus(isHealthy, message, DateTime.UtcNow);
        }

      /*  public async Task<DatabaseStatus> GetDatabaseStatusAsync()
        {
            try
            {
                var stopwatch = Stopwatch.StartNew();
                var canConnect = await _dbContext.Database.CanConnectAsync();
                stopwatch.Stop();

                return new DatabaseStatus(
                    canConnect,
                    canConnect ? "Database connection successful" : "Database connection failed",
                    stopwatch.ElapsedMilliseconds
                );
            }
            catch (Exception ex)
            {
                return new DatabaseStatus(false, $"Database error: {ex.Message}", 0);
            }
        }*/

        public async Task<CacheStatus> GetCacheStatusAsync()
        {
            try
            {
                var testKey = "health_check_" + Guid.NewGuid();
                var testValue = "test_value";
                var stopwatch = Stopwatch.StartNew();

                // تست نوشتن
                await _cacheService.SetAsync(testKey, testValue, TimeSpan.FromSeconds(5));

                // تست خواندن
                var result = await _cacheService.GetAsync<string>(testKey);

                stopwatch.Stop();

                var isHealthy = result == testValue;
                return new CacheStatus(
                    isHealthy,
                    isHealthy ? "Cache service operational" : "Cache test failed",
                    stopwatch.ElapsedMilliseconds
                );
            }
            catch (Exception ex)
            {
                return new CacheStatus(false, $"Cache error: {ex.Message}", 0);
            }
        }

        public Task<DatabaseStatus> GetDatabaseStatusAsync()
        {
            throw new NotImplementedException();
        }
    }
}
