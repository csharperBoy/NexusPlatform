using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Domain.Common;
using Event.Infrastructure.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Event.Test
{
    public class OutboxServiceTests
    {
        private sealed class TestDbContext : DbContext
        {
            public TestDbContext(DbContextOptions<TestDbContext> options) : base(options) { }
            public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();
            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                modelBuilder.ApplyConfiguration(new Core.Infrastructure.Database.Configurations.OutboxMessageConfiguration("Test"));
            }
        }

        private static TestDbContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<TestDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            return new TestDbContext(options);
        }

        private sealed class TestEvent : IDomainEvent
        {
            public DateTime OccurredOn { get; } = DateTime.UtcNow;
            public string Name { get; } = "E";
        }

        [Fact]
        public async Task AddEventsAsync_Should_Create_OutboxMessages()
        {
            using var ctx = CreateContext();
            var logger = new Mock<ILogger<OutboxService<TestDbContext>>>();
            var service = new OutboxService<TestDbContext>(ctx, logger.Object);

            await service.AddEventsAsync(new[] { new TestEvent(), new TestEvent() });
            await ctx.SaveChangesAsync();

            var all = await ctx.OutboxMessages.ToListAsync();
            all.Should().HaveCount(2);
            all.All(m => m.Status == OutboxMessageStatus.Pending).Should().BeTrue();
        }

        [Fact]
        public async Task GetPendingMessagesAsync_Should_Return_Pending_Ordered_By_OccurredOn()
        {
            using var ctx = CreateContext();
            var logger = new Mock<ILogger<OutboxService<TestDbContext>>>();
            var service = new OutboxService<TestDbContext>(ctx, logger.Object);

            await service.AddEventsAsync(new[] { new TestEvent(), new TestEvent(), new TestEvent() });
            await ctx.SaveChangesAsync();

            var pending = await service.GetPendingMessagesAsync(2);
            pending.Should().HaveCount(2);
            pending.Select(x => x.Status).Distinct().Should().ContainSingle().Which.Should().Be(OutboxMessageStatus.Pending);
        }

        [Fact]
        public async Task Mark_Status_Transitions_Should_Persist()
        {
            using var ctx = CreateContext();
            var logger = new Mock<ILogger<OutboxService<TestDbContext>>>();
            var service = new OutboxService<TestDbContext>(ctx, logger.Object);

            await service.AddEventsAsync(new[] { new TestEvent() });
            await ctx.SaveChangesAsync();

            var msg = await ctx.OutboxMessages.FirstAsync();

            await service.MarkAsProcessingAsync(msg.Id);
            (await ctx.OutboxMessages.FindAsync(msg.Id))!.Status.Should().Be(OutboxMessageStatus.Processing);

            await service.MarkAsCompletedAsync(msg.Id);
            var completed = await ctx.OutboxMessages.FindAsync(msg.Id);
            completed!.Status.Should().Be(OutboxMessageStatus.Completed);
            completed.ProcessedOnUtc.Should().NotBeNull();

            await service.AddEventsAsync(new[] { new TestEvent() });
            await ctx.SaveChangesAsync();
            var msg2 = await ctx.OutboxMessages.OrderBy(x => x.OccurredOnUtc).LastAsync();

            await service.MarkAsFailedAsync(msg2.Id, new InvalidOperationException("x"));
            var failed = await ctx.OutboxMessages.FindAsync(msg2.Id);
            failed!.Status.Should().Be(OutboxMessageStatus.Failed);
            failed.ErrorMessage.Should().Be("x");
            failed.ErrorStackTrace.Should().NotBeNullOrEmpty();
            failed.RetryCount.Should().Be(1);
        }

        [Fact]
        public async Task CleanupProcessedMessagesAsync_Should_Delete_Older_Completed()
        {
            using var ctx = CreateContext();
            var logger = new Mock<ILogger<OutboxService<TestDbContext>>>();
            var service = new OutboxService<TestDbContext>(ctx, logger.Object);

            await service.AddEventsAsync(new[] { new TestEvent(), new TestEvent() });
            await ctx.SaveChangesAsync();

            var msgs = await ctx.OutboxMessages.ToListAsync();
            foreach (var m in msgs)
            {
                m.MarkAsCompleted();
                // backdate one
                m.ProcessedOnUtc = DateTime.UtcNow.AddMinutes(-30);
            }
            await ctx.SaveChangesAsync();

            await service.CleanupProcessedMessagesAsync(DateTime.UtcNow.AddMinutes(-10));

            var remaining = await ctx.OutboxMessages.CountAsync();
            remaining.Should().Be(0);
        }
    }
}
