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
    /// Specification برای گرفتن Assignmentهای فعال یک شخص
    /// </summary>
    public class ActiveAssignmentsByEmploymentSpec : BaseSpecification<Assignment>
    {
        public ActiveAssignmentsByEmploymentSpec(Guid employmentId)
            : base(a => a.FkEmploymentId == employmentId &&
                       a.IsCurrent &&
                       (!a.EffectiveTo.HasValue || a.EffectiveTo >  DateTime.UtcNow))
        {
            AddInclude(a => a.Post);
            AddInclude(a => a.Post.OrganizationUnit);
            ApplyOrderByDescending(a => a.EffectiveFrom);
        }
    }
}
