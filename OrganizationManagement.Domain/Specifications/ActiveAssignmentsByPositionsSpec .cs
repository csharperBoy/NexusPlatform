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
    /// Specification برای گرفتن Assignmentهای فعال برای مجموعه‌ای از Positionها
    /// </summary>
    public class ActiveAssignmentsByPositionsSpec : BaseSpecification<Assignment>
    {
        public ActiveAssignmentsByPositionsSpec(IEnumerable<Guid> positionIds)
            : base(a => positionIds.Contains(a.FkPositionId) &&
                       a.IsActive &&
                       (!a.EndDate.HasValue || a.EndDate > DateTime.UtcNow))
        {
            AddInclude(a => a.Position);
            ApplyOrderBy(a => a.StartDate);
        }
    }
}
