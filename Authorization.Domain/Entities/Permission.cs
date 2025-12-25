using Authorization.Domain.Enums;
using Authorization.Domain.Events;
using Core.Domain.Common;
using Core.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Authorization.Domain.Entities
{

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
        public Guid? SpecificScopeId { get; private set; } // اگر Scope == SpecificUnit باشد پر می‌شود

        // 5. وضعیت
        public bool IsAllow { get; private set; } = true; // برای Deny کردن خاص

        // Navigation
        public virtual Resource Resource { get; private set; } = null!;

        protected Permission() { }

        public Permission(
            Guid resourceId,
            AssigneeType assigneeType,
            Guid assigneeId,
            PermissionAction action,
            ScopeType scope = ScopeType.None,
            Guid? specificScopeId = null,
            bool isAllow = true)
        {
            ResourceId = resourceId;
            AssigneeType = assigneeType;
            AssigneeId = assigneeId;
            Action = action;
            SetScope(scope, specificScopeId);
            IsAllow = isAllow;
        }

        public void UpdateScope(ScopeType newScope, Guid? specificScopeId)
        {
            SetScope(newScope, specificScopeId);
            // اینجا Domain Event برای پاک کردن کش دسترسی‌ها واجب است
        }

        private void SetScope(ScopeType scope, Guid? specificId)
        {
            if (scope == ScopeType.SpecificUnit && !specificId.HasValue)
                throw new ArgumentException("SpecificScopeId is required when scope is SpecificUnit.");

            Scope = scope;
            SpecificScopeId = (scope == ScopeType.SpecificUnit) ? specificId : null;
        }
    }
}
