using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Core.Application.Abstractions.Events;
using Core.Domain.Common;
using Core.Infrastructure.Database.Configurations;
using Event.Infrastructure.Processor;
using Event.Infrastructure.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Event.Test
{
    public class OutboxProcessorTests
    {
        private sealed class TestDbContext : DbContext
        {
            public TestDbContext(DbContextOptions<TestDbContext> options) : base(options) { }
            public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();
            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                modelBuilder.ApplyConfiguration(new OutboxMessageConfiguration("Test"));
            }
        }

        private sealed class TestEvent : IDomainEvent
        {
            public DateTime OccurredOn { get; } = DateTime.UtcNow;
            public string Payload { get; } = "E";
        }

        [Fact]
        public async Task Processor_Should_Publish_Events_And_Complete_Messages()
        {
            // Arrange DI container
            var services = new ServiceCollection();

            services.AddLogging();

            var eventBusMock = new Mock<IEventBus>();
            eventBusMock.Setup(b => b.PublishAsync(It.IsAny<IDomainEvent>()))
                        .Returns(Task.CompletedTask)
                        .Verifiable();

            services.AddSingleton<IEventBus>(eventBusMock.Object);

            services.AddDbContext<TestDbContext>(o => o.UseInMemoryDatabase(Guid.NewGuid().ToString()));
            services.AddTransient(typeof(IOutboxService<>), typeof(OutboxService<>));
            services.AddTransient(typeof(OutboxProcessor<TestDbContext>));

            var provider = services.BuildServiceProvider();

            // Seed outbox
            using (var scope = provider.CreateScope())
            {
                var ctx = scope.ServiceProvider.GetRequiredService<TestDbContext>();
                var svc = new OutboxService<TestDbContext>(ctx, scope.ServiceProvider.GetRequiredService<ILogger<OutboxService<TestDbContext>>>());

                await svc.AddEventsAsync(new[] { new TestEvent(), new TestEvent() });
                await ctx.SaveChangesAsync();
            }

            // Act: Run processor shortly
            var processor = provider.GetRequiredService<OutboxProcessor<TestDbContext>>();
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(2));
            await processor.StartAsync(cts.Token);
            await Task.Delay(500, cts.Token); // give it time to process
            await processor.StopAsync(CancellationToken.None);

            // Assert: messages completed and EventBus invoked
            using (var scope = provider.CreateScope())
            {
                var ctx = scope.ServiceProvider.GetRequiredService<TestDbContext>();
                var all = await ctx.OutboxMessages.ToListAsync();
                all.Should().NotBeEmpty();
                all.All(m => m.Status is OutboxMessageStatus.Completed or OutboxMessageStatus.Processing)
                   .Should().BeTrue();
            }

            eventBusMock.Verify(b => b.PublishAsync(It.IsAny<IDomainEvent>()), Times.AtLeast(2));
        }
    }
}
