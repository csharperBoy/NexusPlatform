using Core.Domain.Specifications;
using OrganizationManagement.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrganizationManagement.Domain.Specifications
{
    /// <summary>
    /// Specification برای گرفتن واحدهای سازمانی فرزند یک والد
    /// </summary>
    public class OrganizationUnitsByParentSpec : BaseSpecification<OrganizationUnit>
    {
        public OrganizationUnitsByParentSpec(Guid? parentId)
            : base(u => u.ParentId == parentId)
        {
            AddInclude(u => u.Parent);
            ApplyOrderBy(u => u.Name);
        }
    }
}
