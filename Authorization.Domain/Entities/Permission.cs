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
            // Core Properties
            public Guid ResourceId { get; private set; }
            public AssigneeType AssigneeType { get; private set; }
            public Guid AssigneeId { get; private set; }
            public PermissionAction Action { get; private set; }

            // Permission Settings
            public bool IsAllow { get; private set; } = true;
            public int Priority { get; private set; } = 1; // 1: Low, 2: Medium, 3: High
            public string Condition { get; private set; } // شرط اضافی برای دسترسی

            // Temporal Settings
            public DateTime? EffectiveFrom { get; private set; }
            public DateTime? ExpiresAt { get; private set; }

            // Metadata
            public string Description { get; private set; }
            public bool IsActive { get; private set; } = true;
            public int Order { get; private set; } // برای ترتیب ارزیابی

            // Computed Properties
            public bool IsExpired => ExpiresAt.HasValue && ExpiresAt < DateTime.UtcNow;
            public bool IsEffective => !EffectiveFrom.HasValue || EffectiveFrom <= DateTime.UtcNow;
            public bool IsValid => IsActive && !IsExpired && IsEffective;

            // Navigation Properties
            public virtual Resource Resource { get; private set; } = null!;

            // Constructor for EF Core
            protected Permission() { }

            // Main Constructor
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
                ValidateInputs(resourceId, assigneeId, priority, effectiveFrom, expiresAt);

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

                AddDomainEvent(new PermissionCreatedEvent(Id, ResourceId, AssigneeType, AssigneeId, Action, IsAllow));
            }

            // Domain Methods
            public void Update(
                PermissionAction action,
                bool isAllow,
                int priority,
                string condition,
                string description,
                int order)
            {
                ValidatePriority(priority);

                Action = action;
                IsAllow = isAllow;
                Priority = priority;
                Condition = condition?.Trim();
                Description = description;
                Order = order;
                ModifiedAt = DateTime.UtcNow;

                AddDomainEvent(new PermissionUpdatedEvent(Id, Action, IsAllow, Priority));
            }

            public void ToggleAllow(bool isAllow)
            {
                if (IsAllow == isAllow) return;

                IsAllow = isAllow;
                ModifiedAt = DateTime.UtcNow;

                AddDomainEvent(new PermissionToggledEvent(Id, IsAllow));
            }

            public void SetPriority(int priority)
            {
                ValidatePriority(priority);

                Priority = priority;
                ModifiedAt = DateTime.UtcNow;
            }

            public void SetTemporalRange(DateTime? effectiveFrom, DateTime? expiresAt)
            {
                ValidateTemporalRange(effectiveFrom, expiresAt);

                EffectiveFrom = effectiveFrom;
                ExpiresAt = expiresAt;
                ModifiedAt = DateTime.UtcNow;

                AddDomainEvent(new PermissionTemporalRangeChangedEvent(Id, EffectiveFrom, ExpiresAt));
            }

            public void Activate()
            {
                if (IsActive) return;

                IsActive = true;
                ModifiedAt = DateTime.UtcNow;
                AddDomainEvent(new PermissionActivatedEvent(Id));
            }

            public void Deactivate()
            {
                if (!IsActive) return;

                IsActive = false;
                ModifiedAt = DateTime.UtcNow;
                AddDomainEvent(new PermissionDeactivatedEvent(Id));
            }

            public void ExtendExpiration(DateTime newExpiresAt)
            {
                if (newExpiresAt <= DateTime.UtcNow)
                    throw new AuthorizationDomainException("New expiration date must be in the future.");

                if (ExpiresAt.HasValue && newExpiresAt <= ExpiresAt)
                    throw new AuthorizationDomainException("New expiration date must extend current expiration.");

                ExpiresAt = newExpiresAt;
                ModifiedAt = DateTime.UtcNow;
            }

            // Validation Methods
            private static void ValidateInputs(
                Guid resourceId,
                Guid assigneeId,
                int priority,
                DateTime? effectiveFrom,
                DateTime? expiresAt)
            {
                if (resourceId == Guid.Empty)
                    throw new AuthorizationDomainException("Resource ID cannot be empty.");

                if (assigneeId == Guid.Empty)
                    throw new AuthorizationDomainException("Assignee ID cannot be empty.");

                ValidatePriority(priority);
                ValidateTemporalRange(effectiveFrom, expiresAt);
            }

            private static void ValidatePriority(int priority)
            {
                if (priority < 1 || priority > 3)
                    throw new AuthorizationDomainException("Priority must be between 1 and 3.");
            }

            private static void ValidateTemporalRange(DateTime? effectiveFrom, DateTime? expiresAt)
            {
                if (effectiveFrom.HasValue && expiresAt.HasValue && effectiveFrom >= expiresAt)
                    throw new AuthorizationDomainException("Effective from date must be before expiration date.");
            }

            // Business Logic
            public bool Matches(Permission other)
            {
                return ResourceId == other.ResourceId &&
                       AssigneeType == other.AssigneeType &&
                       AssigneeId == other.AssigneeId &&
                       Action == other.Action;
            }

            public bool Overrides(Permission other)
            {
                // بررسی آیا این دسترسی اولویت بالاتری دارد
                return Priority > other.Priority ||
                       (Priority == other.Priority && Order < other.Order) ||
                       (AssigneeType == AssigneeType.Person && other.AssigneeType != AssigneeType.Person);
            }

            public bool AppliesTo(AssigneeType assigneeType, Guid assigneeId)
            {
                return AssigneeType == assigneeType && AssigneeId == assigneeId;
            }
        }
    
}
