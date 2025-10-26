using Core.Application.Abstractions.Caching;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Infrastructure.HealthChecks
{
    public class CacheHealthContributor : IHealthContributor
    {
        private readonly ICacheService _cacheService;
        public string Name => "Cache";

        public CacheHealthContributor(ICacheService cacheService)
        {
            _cacheService = cacheService;
        }

        public async Task<(bool IsHealthy, string Message, long ResponseTimeMs)> CheckAsync()
        {
            try
            {
                var testKey = "health_check_" + Guid.NewGuid();
                var testValue = "test_value";
                var stopwatch = Stopwatch.StartNew();

                await _cacheService.SetAsync(testKey, testValue, TimeSpan.FromSeconds(5));
                var result = await _cacheService.GetAsync<string>(testKey);

                stopwatch.Stop();
                var ok = result == testValue;
                return (ok, ok ? "Cache service operational" : "Cache test failed", stopwatch.ElapsedMilliseconds);
            }
            catch (Exception ex)
            {
                return (false, $"Cache error: {ex.Message}", 0);
            }
        }
    }

}
