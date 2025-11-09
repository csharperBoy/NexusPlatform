using Core.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sample.Shared.Events
{
    /// <summary>
    /// ایونت بین‌ماژولی برای اطلاع‌رسانی ایجاد یک Sample جدید
    /// این ایونت از IDomainEvent ارث‌بری می‌کند تا توسط Outbox مکانیزم Core منتشر شود.
    /// </summary>
    public class SampleCreatedIntegrationEvent : IDomainEvent
    {
        public Guid SampleId { get; }
        public string Property1 { get; }
        public DateTime OccurredOn { get; }

        public SampleCreatedIntegrationEvent(Guid sampleId, string property1, DateTime occurredOn)
        {
            SampleId = sampleId;
            Property1 = property1;
            OccurredOn = occurredOn;
        }
    }
}
