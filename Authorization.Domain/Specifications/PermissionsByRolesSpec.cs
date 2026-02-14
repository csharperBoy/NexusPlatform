using Authorization.Domain.Entities;
using Core.Domain.Specifications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Shared.Enums.Authorization;
namespace Authorization.Domain.Specifications
{
    public class PermissionsByRolesSpec : BaseSpecification<Permission>
    {
        public PermissionsByRolesSpec(List<Guid> rolesId)
            : base(p => p.AssigneeType == AssigneeType.Role && // تغییر از User به Person
                      rolesId.Any(r => r == p.AssigneeId))
        {
            AddInclude(p => p.Resource);
            ApplyOrderBy(p => p.ResourceId);
            ApplyThenOrderBy(p => p.Action);
        }
    }
}
