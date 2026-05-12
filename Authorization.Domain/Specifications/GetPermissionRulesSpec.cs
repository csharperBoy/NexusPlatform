using Authorization.Domain.Entities;
using Core.Domain.Specifications;
using Core.Shared.Enums.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Authorization.Domain.Specifications
{
    public class GetPermissionRulesSpec : BaseSpecification<PermissionRule>
    {
        public GetPermissionRulesSpec(
                                 Guid? permissionId = null)
            : base(p =>
                        (permissionId == null || p.PermissionId == permissionId))
        {
            AddInclude(p => p.JoinDetail);
            ApplyOrderBy(p => p.GroupOrder);
            ApplyThenOrderBy(p => p.Id);
        }
    }
}
