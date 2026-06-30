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
        public GetPermissionsSpec(
                                 Guid? AssigneeId = null,
                                 Guid? ResourceId = null,
                                 string? description = null)
            : base(p => 
                        //(AssigneeType == null || p.AssigneeType == AssigneeType ) && 
                        (AssigneeId == null || p.FkPermissionAssigneeId == AssigneeId) && 
                        (ResourceId == null || p.FkResourceId == ResourceId) && 
                        (description == null || p.Description.Contains(description)))
        {
            AddInclude(p => p.Resource);            
            ApplyOrderByDescending(p => p.CreatedAt);
            //ApplyThenOrderBy(p => p.ResourceId);
            //ApplyThenOrderBy(p => p.Action);
        }
    }
}
