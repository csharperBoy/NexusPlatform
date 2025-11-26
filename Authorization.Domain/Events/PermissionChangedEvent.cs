using Authorization.Domain.Entities;
using Authorization.Domain.Enums;
using Core.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Authorization.Domain.Events
{
    // 📌 وقتی دسترسی کاربر به یک منبع تغییر می‌کند
    public class PermissionChangedEvent : IDomainEvent
    {
        public Guid UserId { get; }
        public Guid ResourceId { get; }
        public DateTime OccurredOn { get; }

        public PermissionChangedEvent(Guid userId, Guid resourceId)
        {
            UserId = userId;
            ResourceId = resourceId;
            OccurredOn = DateTime.UtcNow;
        }
    }
}
