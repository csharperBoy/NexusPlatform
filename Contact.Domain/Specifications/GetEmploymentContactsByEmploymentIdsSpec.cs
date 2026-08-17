using Contact.Domain.Entities;
using Core.Domain.Specifications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contact.Domain.Specifications
{
    
    public class GetEmploymentContactsByEmploymentIdsSpec : BaseSpecification<EmploymentContact>
    {
        public GetEmploymentContactsByEmploymentIdsSpec(List<Guid> employmentIds)
            : base(p =>
                       employmentIds.Any(s => s == p.FkEmploymentId)
            // && ( value == null || p.Value == value )
            )
        {
        }
    }
}
