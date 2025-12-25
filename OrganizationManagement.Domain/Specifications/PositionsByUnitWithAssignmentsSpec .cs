using Core.Domain.Specifications;
using OrganizationManagement.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrganizationManagement.Domain.Specifications
{
    public class PositionsByUnitWithAssignmentsSpec : BaseSpecification<Position>
    {
        public PositionsByUnitWithAssignmentsSpec(Guid unitId, bool activeAssignmentsOnly = true)
            : base(p => p.FkOrganizationUnitId == unitId)
        {
            AddInclude(p => p.OrganizationUnit);

            if (activeAssignmentsOnly)
            {
                AddInclude(p => p.Assignments.Where(a => a.IsActive &&
                    (!a.EndDate.HasValue || a.EndDate > DateTime.UtcNow)));
            }
            else
            {
                AddInclude(p => p.Assignments);
            }

            ApplyOrderBy(p => p.Title);
        }
    }
}
