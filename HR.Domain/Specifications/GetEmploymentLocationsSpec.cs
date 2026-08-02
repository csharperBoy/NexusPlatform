using Core.Domain.Specifications;
using Core.Shared.Enums.HR;
using HR.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HR.Domain.Specifications
{
    public class GetEmploymentLocationsSpec : BaseSpecification<EmploymentLocation?>
    {
        public GetEmploymentLocationsSpec( Guid employmentId)
            : base(p =>
                          p.FkEmploymentId == employmentId && p.IsCurrent == true
            // && ( value == null || p.Value == value )
            )
        {
        }
    }
}
