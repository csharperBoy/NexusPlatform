using Core.Domain.Specifications;
using HR.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HR.Domain.Specifications
{
    public class ActiveAssignmentsByPostSpec : BaseSpecification<Assignment>
    {
        public ActiveAssignmentsByPostSpec(Guid postId)
            : base(a => a.PostId == postId &&
                       a.IsCurrent &&
                       (!a.EffectiveTo.HasValue || a.EffectiveTo > DateOnly.FromDateTime(DateTime.UtcNow)))
        {
            AddInclude(a => a.Employee);
            AddInclude(a => a.Post.OrganizationUnit);
            ApplyOrderByDescending(a => a.EffectiveFrom);
        }

    }
}
