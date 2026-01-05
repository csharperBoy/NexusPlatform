using Authorization.Domain.Enums;
using Authorization.Domain.Events;
using Core.Domain.Attributes;
using Core.Domain.Common;
using Core.Domain.Enums;
using Core.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Authorization.Domain.Entities
{
    [SecuredResource("Authorization.Permission")]
    public class Permission : DataScopedEntity, IAggregateRoot
    {
        // 1. چه کسی؟
        public AssigneeType AssigneeType { get; private set; }
        public Guid AssigneeId { get; private set; } // RoleId, PositionId, or PersonId

        // 2. روی چه چیزی؟
        public Guid ResourceId { get; private set; }

        // 3. چه کاری؟
        public PermissionAction Action { get; private set; }

        // 4. با چه دامنه‌ای؟ (اینجا ادغام شد)
        public ScopeType Scope { get; private set; }
        public Guid? SpecificScopeId { get; private set; } // اگر Scope == SpecificProperty باشد پر می‌شود

        // 5. وضعیت
        public PermissionType Type { get; private set; } = PermissionType.allow; // برای Deny یا allow کردن
        public DateTime? EffectiveFrom { get; private set; }
        public DateTime? ExpiresAt { get; private set; }

        public string? Description { get; private set; }
        public bool IsActive { get; private set; } = true;

        // Navigation
        public virtual Resource Resource { get; private set; } = null!;

        [NotMapped]
        public bool IsExpired => ExpiresAt.HasValue && ExpiresAt < DateTime.UtcNow;
        [NotMapped]
        public bool IsEffective => !EffectiveFrom.HasValue || EffectiveFrom <= DateTime.UtcNow;
        [NotMapped]
        public bool IsValid => IsActive && !IsExpired && IsEffective;

        protected Permission() { }

        public Permission(
            Guid resourceId,
            AssigneeType assigneeType,
            Guid assigneeId,
            PermissionAction action,
            ScopeType scope = ScopeType.None,
            Guid? specificScopeId = null,
            PermissionType type =  PermissionType.allow)
        {
            ResourceId = resourceId;
            AssigneeType = assigneeType;
            AssigneeId = assigneeId;
            Action = action;
            SetScope(scope, specificScopeId);
            Type = type;
        }
        public Permission(
            Guid resourceId,
            AssigneeType assigneeType,
            Guid assigneeId,
            PermissionAction action,
            ScopeType scope = ScopeType.None,
            Guid? specificScopeId = null,
            PermissionType type = PermissionType.allow,            
            DateTime? effectiveFrom = null,
            DateTime? expiresAt = null,
            string? description = null,            
            string createdBy = "system")
        {
            if (resourceId == Guid.Empty) throw new ArgumentException("Resource ID cannot be empty.");
            if (assigneeId == Guid.Empty) throw new ArgumentException("Assignee ID cannot be empty.");

            ResourceId = resourceId;
            AssigneeType = assigneeType;
            AssigneeId = assigneeId;
            Action = action;
            SetScope(scope, specificScopeId);
            Type = type;
            EffectiveFrom = effectiveFrom;
            ExpiresAt = expiresAt;
            Description = description;
            CreatedBy = createdBy;
            CreatedAt = DateTime.UtcNow;
        }
        public void UpdateScope(ScopeType newScope, Guid? specificScopeId)
        {
            SetScope(newScope, specificScopeId);
            // اینجا Domain Event برای پاک کردن کش دسترسی‌ها واجب است
        }

        private void SetScope(ScopeType scope, Guid? specificId)
        {
            if (scope == ScopeType.SpecificProperty && !specificId.HasValue)
                throw new ArgumentException("SpecificScopeId is required when scope is SpecificProperty.");

            Scope = scope;
            SpecificScopeId = (scope == ScopeType.SpecificProperty) ? specificId : null;
        }
        public void Update(
           PermissionAction action,
          
           string description)
        {
            
            Action = action;
            Description = description;
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
