using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Core.Domain.Common
{
    public class OutboxMessage : BaseEntity
    {
        public string TypeName { get; private set; } = string.Empty;        // Simple type name
        public string AssemblyQualifiedName { get; private set; } = string.Empty; // Full type resolution
        public string Content { get; private set; } = string.Empty;         // Serialized payload
        public DateTime OccurredOn { get; private set; } = DateTime.UtcNow;
        public DateTime? ProcessedOn { get; private set; }
        public string? Error { get; private set; }
        public OutboxMessageStatus Status { get; private set; } = OutboxMessageStatus.Pending;
        public int RetryCount { get; private set; }

        // Optimistic concurrency
        public byte[] RowVersion { get; private set; } = Array.Empty<byte>();

        // EF
        private OutboxMessage() { }

        public OutboxMessage(IDomainEvent domainEvent)
        {
            var type = domainEvent.GetType();
            TypeName = type.Name;
            AssemblyQualifiedName = type.AssemblyQualifiedName ?? type.FullName ?? type.Name;
            Content = System.Text.Json.JsonSerializer.Serialize(domainEvent, type);
            OccurredOn = domainEvent.OccurredOn;
            Status = OutboxMessageStatus.Pending;
            RetryCount = 0;
        }

        public void MarkAsProcessing()
        {
            Status = OutboxMessageStatus.Processing;
        }

        public void MarkAsCompleted()
        {
            Status = OutboxMessageStatus.Completed;
            ProcessedOn = DateTime.UtcNow;
        }

        public void MarkAsFailed(string error)
        {
            Status = OutboxMessageStatus.Failed;
            Error = error;
            RetryCount++;
        }
    }

    public enum OutboxMessageStatus
    {
        Pending = 0,
        Processing = 1,
        Completed = 2,
        Failed = 3
    }
}