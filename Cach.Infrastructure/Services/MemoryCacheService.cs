using Core.Application.Abstractions.Caching;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;


namespace Cach.Infrastructure.Services
{
    public class MemoryCacheService : ICacheService
    {
        private readonly IMemoryCache _memoryCache;
        private readonly ILogger<MemoryCacheService> _logger;
        private static readonly ConcurrentDictionary<string, byte> _keys = new();

        public MemoryCacheService(IMemoryCache memoryCache, ILogger<MemoryCacheService> logger)
        {
            _memoryCache = memoryCache;
            _logger = logger;
        }

        public Task<T?> GetAsync<T>(string key)
        {
            var value = _memoryCache.Get<T>(key);
            _logger.LogInformation("📦 MemoryCache get key '{Key}' → {Hit}", key, value != null);
            return Task.FromResult(value);
        }

        public Task SetAsync<T>(string key, T value, TimeSpan? expiration = null)
        {
            var options = new MemoryCacheEntryOptions();
            if (expiration.HasValue)
                options.SetAbsoluteExpiration(expiration.Value);
            else
                options.SetSlidingExpiration(TimeSpan.FromMinutes(30));

            _memoryCache.Set(key, value, options);
            _keys.TryAdd(key, 0);

            _logger.LogInformation("✅ MemoryCache set key '{Key}' with expiration {Expiration}", key, expiration);
            return Task.CompletedTask;
        }

        public Task RemoveAsync(string key)
        {
            _memoryCache.Remove(key);
            _keys.TryRemove(key, out _);
            _logger.LogInformation("🗑️ MemoryCache removed key '{Key}'", key);
            return Task.CompletedTask;
        }

        public Task<bool> ExistsAsync(string key)
        {
            var exists = _memoryCache.TryGetValue(key, out _);
            _logger.LogInformation("🔍 MemoryCache exists check for key '{Key}' → {Exists}", key, exists);
            return Task.FromResult(exists);
        }

        public Task RemoveByPatternAsync(string pattern)
        {
            var regex = new Regex(pattern, RegexOptions.Compiled);
            var toRemove = _keys.Keys.Where(k => regex.IsMatch(k)).ToList();

            foreach (var key in toRemove)
            {
                _memoryCache.Remove(key);
                _keys.TryRemove(key, out _);
                _logger.LogInformation("🧹 MemoryCache removed key by pattern '{Key}'", key);
            }

            return Task.CompletedTask;
        }
    }
}