using Cach.Application.Models;
using Cach.Infrastructure.Services;
using FluentAssertions;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Cach.Test
{
    public class RedisCacheServiceTests
    {
        private readonly Mock<IDistributedCache> _cacheMock;
        private readonly RedisCacheService _service;

        public RedisCacheServiceTests()
        {
            _cacheMock = new Mock<IDistributedCache>();
            var settings = Options.Create(new CacheSettings { DefaultExpirationMinutes = 5 });
            var logger = new Mock<ILogger<RedisCacheService>>();
            _service = new RedisCacheService(_cacheMock.Object, settings, logger.Object);
        }

        [Fact]
        public async Task SetAsync_Should_Serialize_And_Store()
        {
            await _service.SetAsync("key", new { Name = "Ali" });

            _cacheMock.Verify(c => c.SetStringAsync(
                "key",
                It.Is<string>(s => s.Contains("Ali")),
                It.IsAny<DistributedCacheEntryOptions>(),
                default));
        }

        [Fact]
        public async Task GetAsync_Should_Deserialize()
        {
            var json = JsonSerializer.Serialize(new { Name = "Sara" });
            _cacheMock.Setup(c => c.GetStringAsync("key", default)).ReturnsAsync(json);

            var result = await _service.GetAsync<dynamic>("key");

            ((string)result.Name).Should().Be("Sara");
        }
    }
}
