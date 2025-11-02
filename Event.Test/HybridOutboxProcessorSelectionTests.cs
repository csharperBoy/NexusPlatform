using System.Threading;
using System.Threading.Tasks;
using Event.Infrastructure.Processor;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Event.Test
{
    public class HybridOutboxProcessorSelectionTests
    {
        private sealed class TestDbContext : DbContext
        {
            public TestDbContext(DbContextOptions<TestDbContext> options) : base(options) { }
        }

        [Theory]
        [InlineData("sqlserver")]
        [InlineData("postgresql")]
        [InlineData("unknown")]
        public async Task Should_Select_Proper_Processor_Based_On_Config(string providerName)
        {
            var services = new ServiceCollection();
            services.AddLogging();
            services.AddDbContext<TestDbContext>(o => o.UseInMemoryDatabase(providerName));
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new[]
                {
                    new KeyValuePair<string,string>("Database:Provider", providerName)
                })
                .Build();

            var sp = services.BuildServiceProvider();
            var logger = sp.GetRequiredService<ILogger<HybridOutboxProcessor<TestDbContext>>>();

            var hybrid = new HybridOutboxProcessor<TestDbContext>(sp, logger, config);

            using var cts = new CancellationTokenSource(500);
            var act = async () => await hybrid.StartAsync(cts.Token);

            await act.Should().NotThrowAsync(); // smoke: should start
            await hybrid.StopAsync(CancellationToken.None);
        }
    }
}
