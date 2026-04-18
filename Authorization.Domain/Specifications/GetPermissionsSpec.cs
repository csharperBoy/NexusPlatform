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

    public class GetPermissionsSpec : BaseSpecification<Permission>
    {
        public GetPermissionsSpec(AssigneeType? AssigneeType = null,
                                 Guid? AssigneeId = null,
                                 Guid? ResourceId = null,
                                 string? description = null)
            : base(p => 
                        (AssigneeType == null || p.AssigneeType == AssigneeType ) && 
                        (AssigneeId == null || p.AssigneeId == AssigneeId) && 
                        (ResourceId == null || p.ResourceId == ResourceId) && 
                        (description == null || p.Description.Contains(description)))
        {
            AddInclude(p => p.Resource);
            ApplyOrderBy(p => p.ResourceId);
            ApplyThenOrderBy(p => p.Action);
        }
    }
}
