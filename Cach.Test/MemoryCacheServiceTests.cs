using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cach.Test
{
    public class MemoryCacheServiceTests
    {
        private readonly ICacheService _cache;

        public MemoryCacheServiceTests()
        {
            var memoryCache = new MemoryCache(new MemoryCacheOptions());
            var logger = new Mock<ILogger<MemoryCacheService>>();
            _cache = new MemoryCacheService(memoryCache, logger.Object);
        }

        [Fact]
        public async Task Set_And_Get_Should_Work()
        {
            await _cache.SetAsync("key1", "value1", TimeSpan.FromMinutes(1));
            var result = await _cache.GetAsync<string>("key1");
            result.Should().Be("value1");
        }

        [Fact]
        public async Task Exists_Should_Return_True_When_Key_Exists()
        {
            await _cache.SetAsync("key2", 123);
            var exists = await _cache.ExistsAsync("key2");
            exists.Should().BeTrue();
        }

        [Fact]
        public async Task Remove_Should_Delete_Key()
        {
            await _cache.SetAsync("key3", "toRemove");
            await _cache.RemoveAsync("key3");
            var exists = await _cache.ExistsAsync("key3");
            exists.Should().BeFalse();
        }

        [Fact]
        public async Task RemoveByPattern_Should_Remove_Matching_Keys()
        {
            await _cache.SetAsync("user:1", "u1");
            await _cache.SetAsync("user:2", "u2");
            await _cache.RemoveByPatternAsync("^user:");

            var exists1 = await _cache.ExistsAsync("user:1");
            var exists2 = await _cache.ExistsAsync("user:2");

            exists1.Should().BeFalse();
            exists2.Should().BeFalse();
        }
    }
}
