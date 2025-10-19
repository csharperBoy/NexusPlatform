using Core.Application.Abstractions.Caching;
using Core.Application.Models;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
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
        private readonly CacheSettings _cacheSettings;
        private readonly JsonSerializerOptions _jsonOptions;

        public RedisCacheService(IDistributedCache cache, IOptions<CacheSettings> cacheSettings)
        {
            _cache = cache;
            _cacheSettings = cacheSettings.Value;
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false
            };
        }

        public async Task<T?> GetAsync<T>(string key)
        {
            try
            {
                var data = await _cache.GetStringAsync(key);
                return data == null ? default : JsonSerializer.Deserialize<T>(data, _jsonOptions);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Redis get error for key '{key}': {ex.Message}");
                return default;
            }
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null)
        {
            try
            {
                var options = new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = expiration ?? TimeSpan.FromMinutes(_cacheSettings.DefaultExpirationMinutes)
                };

                var data = JsonSerializer.Serialize(value, _jsonOptions);
                await _cache.SetStringAsync(key, data, options);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Redis set error for key '{key}': {ex.Message}");
            }
        }

        public async Task RemoveAsync(string key)
        {
            try
            {
                await _cache.RemoveAsync(key);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Redis remove error for key '{key}': {ex.Message}");
            }
        }

        public async Task<bool> ExistsAsync(string key)
        {
            try
            {
                var data = await _cache.GetAsync(key);
                return data != null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Redis exists error for key '{key}': {ex.Message}");
                return false;
            }
        }

        public Task RemoveByPatternAsync(string pattern)
        {
            // در Redis باید از SCAN و DEL استفاده کرد
            // برای سادگی فعلاً پیاده‌سازی نمی‌کنیم
            return Task.CompletedTask;
        }
    }
}
