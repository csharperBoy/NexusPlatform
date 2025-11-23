using Authorization.Domain.Entities;
using Authorization.Domain.Enums;
using Core.Domain.Specifications;
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
            : base(p => p.AssigneeType == AssigneeType.User &&
                        p.AssigneeId == userId)
        {
            AddInclude(p => p.Resource);

            ApplyOrderBy(p => p.ResourceId);
            ApplyThenOrderBy(p => p.Action);
        }
    }
}
