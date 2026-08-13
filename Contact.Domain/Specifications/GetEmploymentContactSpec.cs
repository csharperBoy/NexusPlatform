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
        public GetEmploymentContactSpec(HrContactType contactType, Guid employmentId, string? value = null)
            : base(p =>
                          p.ContactType == contactType && p.FkEmploymentId == employmentId && p.IsCurrent
            // && ( value == null || p.Value == value )
            )
        {
        }
    }
}
