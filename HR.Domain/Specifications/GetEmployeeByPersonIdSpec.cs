using Core.Domain.Specifications;
using HR.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HR.Domain.Specifications
{
    
    public class GetEmployeeByPersonIdSpec : BaseSpecification<Employment>
    {
        public GetEmployeeByPersonIdSpec(Guid? personId = null)
            : base(p =>
                        personId == null || p.FkNaturalPersonId == personId)
        {
        }
    }
}
