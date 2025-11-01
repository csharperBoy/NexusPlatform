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
        // نوع رویداد
        public string TypeName { get; private set; } = string.Empty;
        public string AssemblyQualifiedName { get; private set; } = string.Empty;

        // محتوای سریال‌شده
        public string Content { get; private set; } = string.Empty;

        // زمان وقوع رویداد
        public DateTime OccurredOnUtc { get; private set; } = DateTime.UtcNow;

        // زمان پردازش
        public DateTime? ProcessedOnUtc { get; private set; }

        // خطاها
        public string? ErrorMessage { get; private set; }
        public string? ErrorStackTrace { get; private set; }

        // وضعیت
        public OutboxMessageStatus Status { get; private set; } = OutboxMessageStatus.Pending;
        public int RetryCount { get; private set; }

        // نسخه‌ی رویداد
        public int EventVersion { get; private set; } = 1;

        // Optimistic concurrency
        public byte[] RowVersion { get; private set; } = Array.Empty<byte>();

        // EF
        private OutboxMessage() { }

        public OutboxMessage(IDomainEvent domainEvent, int eventVersion = 1)
        {
            var type = domainEvent.GetType();
            TypeName = type.Name;
            AssemblyQualifiedName = type.AssemblyQualifiedName ?? type.FullName ?? type.Name;

            Content = System.Text.Json.JsonSerializer.Serialize(domainEvent, type);

            OccurredOnUtc = domainEvent.OccurredOn;
            Status = OutboxMessageStatus.Pending;
            RetryCount = 0;
            EventVersion = eventVersion;
        }

        public void MarkAsProcessing()
        {
            Status = OutboxMessageStatus.Processing;
        }

        public void MarkAsCompleted()
        {
            Status = OutboxMessageStatus.Completed;
            ProcessedOnUtc = DateTime.UtcNow;
        }

        public void MarkAsFailed(Exception ex)
        {
            Status = OutboxMessageStatus.Failed;
            ErrorMessage = ex.Message;
            ErrorStackTrace = ex.StackTrace;
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
