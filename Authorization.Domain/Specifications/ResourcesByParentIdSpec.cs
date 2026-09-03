using Authorization.Domain.Entities;
using Core.Domain.Specifications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Authorization.Domain.Specifications
{
    public class ResourcesByParentIdSpec : BaseSpecification<Resource>
    {
        public ResourcesByParentIdSpec(Guid? parentId)
            : base(r => r.ParentId == parentId)
        {
            AddInclude(r => r.Children);
            AddInclude(r => r.Permissions);
            ApplyThenOrderBy(r => r.Name);
        }
    }
}
