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
        public string Type { get; set; } = string.Empty;       // نوع ایونت
        public string Content { get; set; } = string.Empty;    // محتوای سریالایز شده
        public DateTime OccurredOn { get; set; } = DateTime.UtcNow;
        public DateTime? ProcessedOn { get; set; }            // زمان پردازش
        public string? Error { get; set; }                    // خطا در صورت وجود

        // وضعیت پردازش
        public OutboxMessageStatus Status { get; set; } = OutboxMessageStatus.Pending;

        // تعداد تلاش‌های ناموفق
        public int RetryCount { get; set; }

        // Constructor for EF Core
        private OutboxMessage() { }

        public OutboxMessage(IDomainEvent domainEvent)
        {
            Type = domainEvent.GetType().Name;
            Content = System.Text.Json.JsonSerializer.Serialize(domainEvent, domainEvent.GetType());
            OccurredOn = domainEvent.OccurredOn;
            Status = OutboxMessageStatus.Pending;
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
