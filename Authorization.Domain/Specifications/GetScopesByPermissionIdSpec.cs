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
    public class GetScopesByPermissionIdSpec : BaseSpecification<Scope>
    {
        public GetScopesByPermissionIdSpec(Guid? permissionId = null)
            : base(p =>
                        (permissionId == null || p.PermissionId == permissionId))
        {
        }
    }
}
