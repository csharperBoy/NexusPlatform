using Core.Application.Abstractions.Caching;
using Microsoft.Extensions.Caching.Distributed;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Core.Infrastructure.Caching
{
    public class RedisCacheService : ICacheService
    {
        private readonly IDistributedCache _cache;

        public RedisCacheService(IDistributedCache cache)
        {
            _cache = cache;
        }

        public async Task<T?> GetAsync<T>(string key)
        {
            var data = await _cache.GetStringAsync(key);
            return data == null ? default : JsonSerializer.Deserialize<T>(data);
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null)
        {
            var options = new DistributedCacheEntryOptions();

            if (expiration.HasValue)
                options.SetAbsoluteExpiration(expiration.Value);

            var data = JsonSerializer.Serialize(value);
            await _cache.SetStringAsync(key, data, options);
        }

        public Task RemoveAsync(string key) => _cache.RemoveAsync(key);

        public async Task<bool> ExistsAsync(string key)
        {
            var data = await _cache.GetAsync(key);
            return data != null;
        }

        public Task RemoveByPatternAsync(string pattern)
        {
            // در ری‌دیس می‌شه با pattern کلیدها رو پاک کرد
            // در صورت استفاده از کش دیگر، این متد رو پیاده‌سازی کن
            return Task.CompletedTask;
        }
    }
}
