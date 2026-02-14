using Authorization.Domain.Entities;
using Authorization.Domain.Enums;
using Core.Domain.Specifications;
using Core.Shared.Enums.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Authorization.Domain.Specifications
{
    /*
      📌 PermissionsByAssigneeSpec
      ----------------------------
      تمام Permissionهای وابسته به یک تخصیص‌گیرنده (Assignee = User یا Role)
      */

    public class PermissionsByAssigneeSpec : BaseSpecification<Permission>
    {
        public PermissionsByAssigneeSpec(AssigneeType assigneeType, Guid assigneeId)
            : base(p => p.AssigneeType == assigneeType && p.AssigneeId == assigneeId)
        {
            // شامل‌سازی Resource برای نمایش کامل
            AddInclude(p => p.Resource);

            // مرتب‌سازی پیش‌فرض: ResourceId و سپس Action
            ApplyOrderBy(p => p.ResourceId);
            ApplyThenOrderBy(p => p.Action);
        }
    }
}
