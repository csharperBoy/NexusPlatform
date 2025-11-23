using Authorization.Domain.Enums;
using Authorization.Domain.ValueObjects;
using Core.Domain.Common;
using Core.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Authorization.Domain.Entities
{
    public class Permission : AuditableEntity, IAggregateRoot
    {
        public Guid ResourceId { get; private set; }
        public AssigneeType AssigneeType { get; private set; }
        public Guid AssigneeId { get; private set; }
        public PermissionAction Action { get; private set; }
        public bool IsAllow { get; private set; } = true;
        public DateTime? ExpiresAt { get; private set; } // دسترسی موقت
        public DateTime? EffectiveFrom { get; private set; }
        public string Description { get; private set; }
        public int Order { get; private set; } // برای ترتیب نمایش
        public bool IsActive { get; private set; } = true;

        public bool IsExpired => ExpiresAt.HasValue && ExpiresAt < DateTime.UtcNow;
        public bool IsEffective => !EffectiveFrom.HasValue || EffectiveFrom <= DateTime.UtcNow;

        // Navigation
        public virtual Resource Resource { get; private set; } = null!;

        protected Permission() { }  // EF Core

        public Permission(Guid resourceId, AssigneeType assigneeType, Guid assigneeId,
                          PermissionAction action, bool isAllow = true, string createdBy = "system")
        {
            ResourceId = resourceId;
            AssigneeType = assigneeType;
            AssigneeId = assigneeId;
            Action = action;
            IsAllow = isAllow;
            CreatedBy = createdBy;
        }

        // Domain Behavior
        public void ToggleAllow(bool isAllow)
        {
            IsAllow = isAllow;
            ModifiedAt = DateTime.UtcNow;
        }
    }
}
