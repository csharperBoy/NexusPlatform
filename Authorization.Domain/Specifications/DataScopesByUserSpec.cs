using Authorization.Domain.Entities;
using Authorization.Domain.Enums;
using Core.Domain.Specifications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Authorization.Domain.Specifications
{
    /*
     📌 DataScopesByUserSpec
     -----------------------
     DataScopeهای مستقیم یک کاربر را برمی‌گرداند.
     */

    public class DataScopesByUserSpec : BaseSpecification<DataScope>
    {
        public DataScopesByUserSpec(Guid userId)
            : base(d => d.AssigneeType == AssigneeType.Person &&
                        d.AssigneeId == userId)
        {
            AddInclude(d => d.Resource);

            ApplyOrderBy(d => d.ResourceId);
            ApplyThenOrderBy(d => d.Scope);
        }
    }
}
