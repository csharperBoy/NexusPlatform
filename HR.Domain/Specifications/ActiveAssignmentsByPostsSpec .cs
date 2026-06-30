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
    /// Specification برای گرفتن Assignmentهای فعال برای مجموعه‌ای از Positionها
    /// </summary>
    public class ActiveAssignmentsByPostsSpec : BaseSpecification<Assignment>
    {
        public ActiveAssignmentsByPostsSpec(IEnumerable<Guid> postIds)
            : base(a => postIds.Contains(a.FkPostId) &&
                       a.IsCurrent &&
                       (!a.EffectiveTo.HasValue || a.EffectiveTo > DateOnly.FromDateTime( DateTime.UtcNow)))
        {
            AddInclude(a => a.Post);
            ApplyOrderBy(a => a.EffectiveFrom);
        }
    }
}
