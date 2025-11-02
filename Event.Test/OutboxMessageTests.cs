using Core.Domain.Common;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Event.Test
{
    public class OutboxMessageTests
    {
        private sealed class TestDomainEvent : IDomainEvent
        {
            public DateTime OccurredOn { get; } = DateTime.UtcNow;
            public string Payload { get; } = "data";
        }

        [Fact]
        public void Ctor_Should_Initialize_From_DomainEvent()
        {
            var evt = new TestDomainEvent();

            var msg = new OutboxMessage(evt, eventVersion: 2);

            msg.TypeName.Should().Be(nameof(TestDomainEvent));
            msg.AssemblyQualifiedName.Should().NotBeNullOrEmpty();
            msg.Content.Should().NotBeNullOrEmpty();
            msg.OccurredOnUtc.Should().Be(evt.OccurredOn);
            msg.Status.Should().Be(OutboxMessageStatus.Pending);
            msg.RetryCount.Should().Be(0);
            msg.EventVersion.Should().Be(2);
        }

        [Fact]
        public void MarkAsProcessing_Should_Set_Status_Processing()
        {
            var msg = new OutboxMessage(new TestDomainEvent());
            msg.MarkAsProcessing();
            msg.Status.Should().Be(OutboxMessageStatus.Processing);
            msg.ProcessedOnUtc.Should().BeNull();
        }

        [Fact]
        public void MarkAsCompleted_Should_Set_Status_Completed_And_Timestamp()
        {
            var msg = new OutboxMessage(new TestDomainEvent());
            msg.MarkAsCompleted();
            msg.Status.Should().Be(OutboxMessageStatus.Completed);
            msg.ProcessedOnUtc.Should().NotBeNull();
            msg.ProcessedOnUtc!.Value.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
        }

        [Fact]
        public void MarkAsFailed_Should_Set_Status_Failed_And_Error_Details_And_Increment_Retry()
        {
            var msg = new OutboxMessage(new TestDomainEvent());
            var ex = new InvalidOperationException("boom");

            msg.MarkAsFailed(ex);

            msg.Status.Should().Be(OutboxMessageStatus.Failed);
            msg.ErrorMessage.Should().Be("boom");
            msg.ErrorStackTrace.Should().NotBeNullOrEmpty();
            msg.RetryCount.Should().Be(1);
        }
    }
}
