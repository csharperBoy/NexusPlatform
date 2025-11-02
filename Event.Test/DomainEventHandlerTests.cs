using System;
using System.Threading;
using System.Threading.Tasks;
using Core.Application.Common.Events;
using Core.Domain.Common;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Event.Test
{
    public class DomainEventHandlerTests
    {
        private sealed class TestDomainEvent : IDomainEvent
        {
            public DateTime OccurredOn { get; } = DateTime.UtcNow;
        }

        private sealed class TestHandler : DomainEventHandler<TestDomainEvent>
        {
            public bool Called { get; private set; }
            public TestHandler(ILogger<DomainEventHandler<TestDomainEvent>> logger) : base(logger) { }

            protected override Task HandleEventAsync(TestDomainEvent @event, CancellationToken cancellationToken)
            {
                Called = true;
                return Task.CompletedTask;
            }
        }

        [Fact]
        public async Task HandleAsync_Should_Invoke_HandleEventAsync_And_Log()
        {
            var logger = new Mock<ILogger<DomainEventHandler<TestDomainEvent>>>();
            var handler = new TestHandler(logger.Object);

            await handler.HandleAsync(new TestDomainEvent(), CancellationToken.None);

            handler.Called.Should().BeTrue();
            logger.VerifyLog(LogLevel.Information, times: 2); // قبل و بعد
        }

        private sealed class ThrowingHandler : DomainEventHandler<TestDomainEvent>
        {
            public ThrowingHandler(ILogger<DomainEventHandler<TestDomainEvent>> logger) : base(logger) { }
            protected override Task HandleEventAsync(TestDomainEvent @event, CancellationToken cancellationToken)
                => throw new ApplicationException("handler failed");
        }

        [Fact]
        public async Task HandleAsync_Should_Log_Error_And_Rethrow_When_Inner_Fails()
        {
            var logger = new Mock<ILogger<DomainEventHandler<TestDomainEvent>>>();
            var handler = new ThrowingHandler(logger.Object);

            var act = async () => await handler.HandleAsync(new TestDomainEvent(), CancellationToken.None);

            await act.Should().ThrowAsync<ApplicationException>()
                .WithMessage("handler failed");

            logger.VerifyLog(LogLevel.Error, times: 1);
        }
    }

    // Simple logger verification helpers
    internal static class LoggerVerifyExtensions
    {
        public static void VerifyLog<T>(this Mock<ILogger<T>> logger, LogLevel level, int times = 1)
        {
            logger.Verify(
                x => x.Log(
                    level,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception?, string>)It.IsAny<object>()),
                Times.Exactly(times));
        }
    }
}
