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
     📌 PermissionsByUserSpec
     -------------------------
     Permissionهای مستقیم مخصوص یک User را برمی‌گرداند.
     */

    public class PermissionsByUserSpec : BaseSpecification<Permission>
    {
        public PermissionsByUserSpec(Guid userId)
            : base(p => p.FkPermissionAssigneeId == userId)
        {
            AddInclude(p => p.Resource);
            ApplyOrderBy(p => p.FkResourceId);
            ApplyThenOrderBy(p => p.Action);
        }
    }
}
