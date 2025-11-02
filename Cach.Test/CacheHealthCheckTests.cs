using Cach.Infrastructure.HealthChecks;
using Core.Application.Abstractions.Caching;
using FluentAssertions;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cach.Test
{
    public class CacheHealthCheckTests
    {
        [Fact]
        public async Task Should_Return_Healthy_When_Cache_Works()
        {
            var cache = new Mock<ICacheService>();
            cache.Setup(c => c.SetAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TimeSpan?>()))
                 .Returns(Task.CompletedTask);
            cache.Setup(c => c.GetAsync<string>(It.IsAny<string>()))
                 .ReturnsAsync("test_value");

            var hc = new CacheHealthCheck(cache.Object);

            var result = await hc.CheckHealthAsync(new HealthCheckContext());

            result.Status.Should().Be(HealthStatus.Healthy);
        }

        [Fact]
        public async Task Should_Return_Unhealthy_When_Cache_Fails()
        {
            var cache = new Mock<ICacheService>();
            cache.Setup(c => c.SetAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TimeSpan?>()))
                 .ThrowsAsync(new Exception("fail"));

            var hc = new CacheHealthCheck(cache.Object);

            var result = await hc.CheckHealthAsync(new HealthCheckContext());

            result.Status.Should().Be(HealthStatus.Unhealthy);
        }
    }
}
