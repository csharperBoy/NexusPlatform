using Core.Application.Abstractions.Caching;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Diagnostics;

namespace Cach.Infrastructure.HealthChecks
{
    public class CacheHealthCheck : IHealthCheck
    {
        private readonly ICacheService _cacheService;

        public CacheHealthCheck(ICacheService cacheService)
        {
            _cacheService = cacheService;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context,
            CancellationToken cancellationToken = default)
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

                if (ok)
                {
                    return HealthCheckResult.Healthy(
                        $"Cache service operational (ResponseTime: {stopwatch.ElapsedMilliseconds} ms)");
                }
                else
                {
                    return HealthCheckResult.Unhealthy("Cache test failed");
                }
            }
            catch (Exception ex)
            {
                return HealthCheckResult.Unhealthy($"Cache error: {ex.Message}", ex);
            }
        }
    }

}
