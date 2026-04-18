using Authorization.Domain.Enums;
using Authorization.Domain.Events;
using Core.Domain.Attributes;
using Core.Domain.Common;
using Core.Domain.Common.EntityProperties;
using Core.Domain.Enums;
using Core.Domain.Interfaces;
using Core.Shared.Enums;
using Core.Shared.Enums.Authorization;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Authorization.Domain.Entities
{
    [SecuredResource("Authorization.Permission")]
    public class Permission : BaseEntity ,IAuditableEntity, IOwnerableEntity, IAggregateRoot
    {
        #region IAuditableEntity Impelement
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow; // 📌 زمان ایجاد
        public string? CreatedBy { get; set; }                      // 📌 کاربر ایجادکننده
        public DateTime? ModifiedAt { get; set; }                   // 📌 زمان آخرین تغییر
        public string? ModifiedBy { get; set; }                     // 📌 کاربر آخرین تغییر
        #endregion
        // 1. چه کسی؟
        public AssigneeType AssigneeType { get; private set; }
        public Guid AssigneeId { get; private set; } // RoleId, PositionId, or PersonId

        // 2. روی چه چیزی؟
        public Guid ResourceId { get; private set; }

        // 3. چه کاری؟
        public PermissionAction Action { get; private set; }

        // 5. وضعیت
        public PermissionEffect Effect { get; private set; } = PermissionEffect.allow; // برای Deny یا allow کردن
        public DateTime? EffectiveFrom { get; private set; }
        public DateTime? ExpiresAt { get; private set; }

        public string? Description { get; private set; }
        public bool IsActive { get; private set; } = true;

        // Navigation
        public virtual Resource Resource { get; private set; } = null!;
        public virtual ICollection<PermissionRule> Rules { get; private set; }

        public virtual ICollection<Scope> Scopes { get; private set; }
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
            PermissionEffect effect = PermissionEffect.allow)
        {
            ResourceId = resourceId;
            AssigneeType = assigneeType;
            AssigneeId = assigneeId;
            Action = action;
            Effect = effect;
        }
        public Permission(
            Guid resourceId,
            AssigneeType assigneeType,
            Guid assigneeId,
            PermissionAction action,
            PermissionEffect effect = PermissionEffect.allow,            
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
            Effect = effect;
            EffectiveFrom = effectiveFrom;
            ExpiresAt = expiresAt;
            Description = description;
            CreatedBy = createdBy;
            CreatedAt = DateTime.UtcNow;
        }


        private void Touch() => ModifiedAt = DateTime.UtcNow;

        

        public bool ApplyChange(
                Guid? _ResourceId = null,
                AssigneeType? _AssigneeType = null,
                Guid? _AssigneeId = null,
                PermissionAction? _Action = null,
                PermissionEffect? _effect = null,
                DateTime? _EffectiveFrom = null,
                DateTime? _ExpiresAt = null,
                bool? _IsActive = null,
                string? _Description = null

            )
        {
            bool hasChange = false;
            // آپدیت فیلدها
            if (_ResourceId != null && _ResourceId != this.ResourceId)
            {
                this.ResourceId =(Guid) _ResourceId;
                hasChange = true;
            }
            if (_AssigneeType != null && _AssigneeType != this.AssigneeType)
            {
                this.AssigneeType =(AssigneeType) _AssigneeType;
                hasChange = true;
            }


            if (_AssigneeId != null && _AssigneeId != this.AssigneeId)
            {
                this.AssigneeId =(Guid) _AssigneeId;
                hasChange = true;
            }
            if (_Action != null && _Action != this.Action)
            {
                this.Action = (PermissionAction)_Action;
                hasChange = true;
            }
            if (_effect != null && _effect != this.Effect)
            {
                this.Effect = (PermissionEffect)_effect;
                hasChange = true;
            }
            if (_EffectiveFrom != null && _EffectiveFrom != this.EffectiveFrom)
            {
                this.EffectiveFrom = _EffectiveFrom;
                hasChange = true;
            }
            if (_ExpiresAt != null && _ExpiresAt != this.ExpiresAt)
            {
                this.ExpiresAt = _ExpiresAt;
                hasChange = true;
            }
            if (_IsActive != null && _IsActive != this.IsActive)
            {
                this.IsActive =(bool) _IsActive;
                hasChange = true;
            }
            if (_Description != null && _Description != this.Description)
            {
                this.Description = _Description;
                hasChange = true;
            }

            if (hasChange)
            {
                Touch();
            }
            return hasChange;
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
        public bool AppliesTo(AssigneeType assigneeType, List<Guid> assigneeId)
        {
            return AssigneeType == assigneeType &&   assigneeId.Any(a=>a == AssigneeId);
        }

        #region IDataScopedEntity Impelement
        public Guid? OwnerOrganizationUnitId { get; protected set; }
        public Guid? OwnerPositionId { get; protected set; }
        public Guid? OwnerPersonId { get; protected set; }
        public Guid? OwnerUserId { get; protected set; }

        public void SetOwners(Guid? userId, Guid? personId, Guid? positiontId, Guid? orgUnitId)
        {
            OwnerUserId = userId;
            OwnerPersonId = personId;
            OwnerPositionId = positiontId;
            OwnerOrganizationUnitId = orgUnitId;
        }
        public void SetPersonOwner(Guid personId)
        {
            OwnerPersonId = personId;
        }
        public void SetUserOwner(Guid userId)
        {
            OwnerUserId = userId;
        }
        public void SetPositionOwner(Guid positiontId)
        {
            OwnerPositionId = positiontId;
        }
        public void SetOrganizationUnitOwner(Guid orgUnitId)
        {
            OwnerOrganizationUnitId = orgUnitId;
        }
        #endregion

    }
}
