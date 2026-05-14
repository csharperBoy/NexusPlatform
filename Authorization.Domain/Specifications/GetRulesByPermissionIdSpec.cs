using Authorization.Domain.Entities;
using Core.Domain.Specifications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Authorization.Domain.Specifications
{
    public class GetRulesByPermissionIdSpec : BaseSpecification<PermissionRule>
    {
        public GetRulesByPermissionIdSpec(Guid? permissionId = null)
            : base(p =>
                        (permissionId == null || p.PermissionId == permissionId))
        {
        }
    }
}
