using Core.Domain.Specifications;
using HR.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HR.Domain.Specifications
{
    /*public class PostsByUnitWithAssignmentsSpec : BaseSpecification<Post>
    {
        public PostsByUnitWithAssignmentsSpec(Guid unitId, bool activeAssignmentsOnly = true)
            : base(p => p.OrganizationUnitId == unitId)
        {
            AddInclude(p => p.OrganizationUnit);

            if (activeAssignmentsOnly)
            {
                AddInclude(p => p.Assignments.Where(a => a. &&
                    (!a.EndDate.HasValue || a.EndDate > DateTime.UtcNow)));
            }
            else
            {
                AddInclude(p => p.Assignments);
            }

            ApplyOrderBy(p => p.Code);
        }
    }*/
}
