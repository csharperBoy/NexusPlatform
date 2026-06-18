using Core.Domain.Specifications;
using HR.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HR.Domain.Specifications
{
    public class PostsByUnitManagerialSpec : BaseSpecification<Post>
    {
        public PostsByUnitManagerialSpec(Guid unitId)
            : base(p => p.OrganizationUnitId == unitId )
        {
            AddInclude(p => p.OrganizationUnit);
            ApplyOrderBy(p => p.Code);
        }
    }
}
