using Cach.Application.Models;
using Core.Application.Abstractions.Caching;
using Core.Application.Models;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Cach.Infrastructure.Services
{
    public class RedisCacheService : ICacheService
    {
        private readonly IDistributedCache _cache;
        private readonly CacheSettings _settings;
        private readonly ILogger<RedisCacheService> _logger;
        private readonly JsonSerializerOptions _jsonOptions;

        public RedisCacheService(
            IDistributedCache cache,
            IOptions<CacheSettings> settings,
            ILogger<RedisCacheService> logger)
        {
            _cache = cache;
            _settings = settings.Value;
            _logger = logger;
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
                _logger.LogError(ex, "❌ Redis get error for key '{Key}'", key);
                return default;
            }
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null)
        {
            try
            {
                var options = new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = expiration ?? TimeSpan.FromMinutes(_settings.DefaultExpirationMinutes)
                };

                var data = JsonSerializer.Serialize(value, _jsonOptions);
                await _cache.SetStringAsync(key, data, options);

                _logger.LogInformation("✅ Redis set key '{Key}' with expiration {Expiration}", key, options.AbsoluteExpirationRelativeToNow);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Redis set error for key '{Key}'", key);
            }
        }

        public async Task RemoveAsync(string key)
        {
            try
            {
                await _cache.RemoveAsync(key);
                _logger.LogInformation("🗑️ Redis removed key '{Key}'", key);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Redis remove error for key '{Key}'", key);
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
                _logger.LogError(ex, "❌ Redis exists check failed for key '{Key}'", key);
                return false;
            }
        }

        public Task RemoveByPatternAsync(string pattern)
        {
            _logger.LogWarning("⚠️ RemoveByPatternAsync for Redis is not implemented due to keyspace scan concerns.");
            return Task.CompletedTask;
        }
    }
}