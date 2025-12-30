using Authorization.Domain.Entities;
using Core.Domain.Specifications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Authorization.Domain.Specifications
{
    public class ResourceByIdSpec : BaseSpecification<Resource>
    {
        public ResourceByIdSpec(Guid resourceId)
            : base(r => r.Id == resourceId )
        {
            AddInclude(r => r.Parent);
            AddInclude(r => r.Children);
            AddInclude(r => r.Permissions);
        }
    }
}
