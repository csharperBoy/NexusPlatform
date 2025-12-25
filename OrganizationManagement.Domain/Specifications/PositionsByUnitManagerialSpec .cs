using Core.Domain.Specifications;
using OrganizationManagement.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrganizationManagement.Domain.Specifications
{
    public class PositionsByUnitManagerialSpec : BaseSpecification<Position>
    {
        public PositionsByUnitManagerialSpec(Guid unitId)
            : base(p => p.FkOrganizationUnitId == unitId && p.IsManagerial)
        {
            AddInclude(p => p.OrganizationUnit);
            ApplyOrderBy(p => p.Title);
        }
    }
}
