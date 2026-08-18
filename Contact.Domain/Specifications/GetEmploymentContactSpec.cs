using Contact.Domain.Entities;
using Core.Domain.Specifications;
using Core.Shared.Enums.HR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contact.Domain.Specifications
{
    
    public class GetEmploymentContactSpec : BaseSpecification<EmploymentContact?>
    {
        public GetEmploymentContactSpec(HrContactType contactType, Guid employmentId, List<string>? values = null)
            : base(p =>
                          p.ContactType == contactType && p.FkEmploymentId == employmentId && p.IsCurrent && values.Contains(p.Value) 
            // && ( value == null || p.Value == value )
            )
        {
        }
    }
}
