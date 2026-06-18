using Core.Domain.Specifications;
using HR.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HR.Domain.Specifications
{
    /// <summary>
    /// Specification برای گرفتن Positionهای یک واحد سازمانی
    /// </summary>
    public class PostsByUnitSpec : BaseSpecification<Post>
    {
        public PostsByUnitSpec(Guid unitId, bool includeOrganizationUnit = true)
            : base(p => p.OrganizationUnitId == unitId)
        {
            if (includeOrganizationUnit)
            {
                AddInclude(p => p.OrganizationUnit);
            }
            ApplyOrderBy(p => p.Code);
        }

       
    }
}
