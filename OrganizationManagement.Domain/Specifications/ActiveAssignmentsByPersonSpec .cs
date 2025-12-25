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
    /// Specification برای گرفتن Assignmentهای فعال یک شخص
    /// </summary>
    public class ActiveAssignmentsByPersonSpec : BaseSpecification<Assignment>
    {
        public ActiveAssignmentsByPersonSpec(Guid personId)
            : base(a => a.FkPersonId == personId &&
                       a.IsActive &&
                       (!a.EndDate.HasValue || a.EndDate > DateTime.UtcNow))
        {
            AddInclude(a => a.Position);
            AddInclude(a => a.Position.OrganizationUnit);
            ApplyOrderByDescending(a => a.StartDate);
        }
    }
}
