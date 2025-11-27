using Authorization.Domain.Entities;
using Core.Domain.Specifications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Authorization.Domain.Specifications
{
    public class EffectiveDataScopesSpec : BaseSpecification<DataScope>
    {
        public EffectiveDataScopesSpec()
            : base(d => d.IsActive &&
                       (!d.EffectiveFrom.HasValue || d.EffectiveFrom <= DateTime.UtcNow) &&
                       (!d.ExpiresAt.HasValue || d.ExpiresAt > DateTime.UtcNow))
        {
            AddInclude(d => d.Resource);
            ApplyOrderBy(d => d.AssigneeType);
            ApplyThenOrderBy(d => d.AssigneeId);
        }
    }
}
