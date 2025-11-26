using Core.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Authorization.Domain.Events
{
    // 📌 وقتی ساختار سلسله مراتب منابع تغییر می‌کند (تأثیر روی ارث‌بری)
    public class ResourceHierarchyChangedEvent : IDomainEvent
    {
        public Guid ResourceId { get; }
        public DateTime OccurredOn { get; }

        public ResourceHierarchyChangedEvent(Guid resourceId)
        {
            ResourceId = resourceId;
            OccurredOn = DateTime.UtcNow;
        }
    }
}
