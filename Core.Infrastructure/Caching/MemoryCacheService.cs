using Core.Application.Abstractions.Caching;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;


namespace Core.Infrastructure.Caching
{
    public class MemoryCacheService : ICacheService
    {
        private readonly IMemoryCache _memoryCache;

        // Track keys to support pattern removal
        private static readonly ConcurrentDictionary<string, byte> _keys = new();

        public MemoryCacheService(IMemoryCache memoryCache)
        {
            _memoryCache = memoryCache;
        }

        public Task<T?> GetAsync<T>(string key)
        {
            return Task.FromResult(_memoryCache.Get<T>(key));
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
            return Task.CompletedTask;
        }

        public Task RemoveAsync(string key)
        {
            _memoryCache.Remove(key);
            _keys.TryRemove(key, out _);
            return Task.CompletedTask;
        }

        public Task<bool> ExistsAsync(string key)
        {
            return Task.FromResult(_memoryCache.TryGetValue(key, out _));
        }

        public Task RemoveByPatternAsync(string pattern)
        {
            var regex = new Regex(pattern, RegexOptions.Compiled);
            var toRemove = _keys.Keys.Where(k => regex.IsMatch(k)).ToList();
            foreach (var key in toRemove)
            {
                _memoryCache.Remove(key);
                _keys.TryRemove(key, out _);
            }
            return Task.CompletedTask;
        }
    }
}