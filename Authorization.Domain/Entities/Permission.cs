using Authorization.Domain.Enums;
using Authorization.Domain.Events;
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
            public int Priority { get; private set; } = 1;
            public string Condition { get; private set; }

            public DateTime? EffectiveFrom { get; private set; }
            public DateTime? ExpiresAt { get; private set; }

            public string Description { get; private set; }
            public bool IsActive { get; private set; } = true;
            public int Order { get; private set; }

            // Computed Properties
            public bool IsExpired => ExpiresAt.HasValue && ExpiresAt < DateTime.UtcNow;
            public bool IsEffective => !EffectiveFrom.HasValue || EffectiveFrom <= DateTime.UtcNow;
            public bool IsValid => IsActive && !IsExpired && IsEffective;

            // Navigation Properties
            public virtual Resource Resource { get; private set; } = null!;

            protected Permission() { }

            public Permission(
                Guid resourceId,
                AssigneeType assigneeType,
                Guid assigneeId,
                PermissionAction action,
                bool isAllow = true,
                int priority = 1,
                string condition = "",
                DateTime? effectiveFrom = null,
                DateTime? expiresAt = null,
                string description = "",
                int order = 0,
                string createdBy = "system")
            {
                if (resourceId == Guid.Empty) throw new ArgumentException("Resource ID cannot be empty.");
                if (assigneeId == Guid.Empty) throw new ArgumentException("Assignee ID cannot be empty.");
                if (priority < 1 || priority > 3) throw new ArgumentException("Priority must be between 1 and 3.");

                ResourceId = resourceId;
                AssigneeType = assigneeType;
                AssigneeId = assigneeId;
                Action = action;
                IsAllow = isAllow;
                Priority = priority;
                Condition = condition?.Trim();
                EffectiveFrom = effectiveFrom;
                ExpiresAt = expiresAt;
                Description = description;
                Order = order;
                CreatedBy = createdBy;
                CreatedAt = DateTime.UtcNow;
            }

            public void Update(
                PermissionAction action,
                bool isAllow,
                int priority,
                string condition,
                string description,
                int order)
            {
                if (priority < 1 || priority > 3) throw new ArgumentException("Priority must be between 1 and 3.");

                Action = action;
                IsAllow = isAllow;
                Priority = priority;
                Condition = condition?.Trim();
                Description = description;
                Order = order;
                ModifiedAt = DateTime.UtcNow;

                // ارسال ایونت وقتی دسترسی تغییر می‌کند
                AddDomainEvent(new PermissionChangedEvent(AssigneeId, ResourceId));
            }

            public void ToggleAllow(bool isAllow)
            {
                if (IsAllow == isAllow) return;

                IsAllow = isAllow;
                ModifiedAt = DateTime.UtcNow;

                // ارسال ایونت وقتی دسترسی تغییر می‌کند
                AddDomainEvent(new PermissionChangedEvent(AssigneeId, ResourceId));
            }

            public void SetTemporalRange(DateTime? effectiveFrom, DateTime? expiresAt)
            {
                if (effectiveFrom.HasValue && expiresAt.HasValue && effectiveFrom >= expiresAt)
                    throw new ArgumentException("Effective from date must be before expiration date.");

                EffectiveFrom = effectiveFrom;
                ExpiresAt = expiresAt;
                ModifiedAt = DateTime.UtcNow;
            }

            public void Activate()
            {
                if (IsActive) return;
                IsActive = true;
                ModifiedAt = DateTime.UtcNow;
            }

            public void Deactivate()
            {
                if (!IsActive) return;
                IsActive = false;
                ModifiedAt = DateTime.UtcNow;
            }

            public bool AppliesTo(AssigneeType assigneeType, Guid assigneeId)
            {
                return AssigneeType == assigneeType && AssigneeId == assigneeId;
            }
        }
    
}
