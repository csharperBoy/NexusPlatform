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
    /// Specification برای گرفتن Positionهای یک واحد سازمانی
    /// </summary>
    public class PositionsByUnitSpec : BaseSpecification<Position>
    {
        public PositionsByUnitSpec(Guid unitId, bool includeOrganizationUnit = true)
            : base(p => p.FkOrganizationUnitId == unitId)
        {
            if (includeOrganizationUnit)
            {
                AddInclude(p => p.OrganizationUnit);
            }
            ApplyOrderBy(p => p.Title);
        }

       
    }
}
