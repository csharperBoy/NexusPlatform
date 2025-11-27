using Authorization.Domain.Entities;
using Core.Domain.Specifications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Authorization.Domain.Specifications
{
    public class ActivePermissionsSpec : BaseSpecification<Permission>
    {
        public ActivePermissionsSpec()
            : base(p => p.IsActive &&
                       (!p.EffectiveFrom.HasValue || p.EffectiveFrom <= DateTime.UtcNow) &&
                       (!p.ExpiresAt.HasValue || p.ExpiresAt > DateTime.UtcNow))
        {
            AddInclude(p => p.Resource);
            ApplyOrderBy(p => p.AssigneeType);
            ApplyThenOrderBy(p => p.AssigneeId);
        }
    }
}
