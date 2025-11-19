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
        public Resource Resource { get; private set; }
        // دسترسی می‌تواند به نقش یا کاربر مستقیم داده شود
        public Guid? RoleId { get; private set; }
        public Guid? UserId { get; private set; }

        public PermissionLevel Level { get; private set; }
        public bool IsEnabled { get; private set; }

        // محدودیت‌های سازمانی برای دسترسی سطح داده
        public OrganizationalScope DataScope { get; private set; }

        private Permission() { } // برای EF Core

        public Permission(Guid resourceId, Guid? roleId, Guid? userId, PermissionLevel level, OrganizationalScope dataScope = null)
        {
            if (roleId == null && userId == null)
                throw new ArgumentException("Permission must be assigned to either a role or user");

            ResourceId = resourceId;
            RoleId = roleId;
            UserId = userId;
            Level = level;
            DataScope = dataScope;
            IsEnabled = true;
        }

        public void UpdateLevel(PermissionLevel newLevel)
        {
            Level = newLevel;
        }

        public void Enable() => IsEnabled = true;
        public void Disable() => IsEnabled = false;

        public void UpdateDataScope(OrganizationalScope newScope)
        {
            DataScope = newScope;
        }
    }
}
}
