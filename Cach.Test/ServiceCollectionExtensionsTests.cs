using Core.Application.Abstractions.Caching;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cach.Test
{
    public class ServiceCollectionExtensionsTests
    {
        [Fact]
        public void Should_Register_ICacheService()
        {
            var inMemorySettings = new Dictionary<string, string>
            {
                {"CacheSettings:UseRedis", "false"}
            };

            IConfiguration config = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();

            var services = new ServiceCollection();
            services.Cach_AddInfrastructure(config);

            var provider = services.BuildServiceProvider();
            var cache = provider.GetService<ICacheService>();

            cache.Should().NotBeNull();
        }
    }
}
