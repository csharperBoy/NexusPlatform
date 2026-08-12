using Contact.Domain.Entities;
using Core.Domain.Specifications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contact.Domain.Specifications
{
    public class GetPhoneBookSpec : BaseSpecification<PhoneBookInfoView>
    {
        public GetPhoneBookSpec(/*Guid? orgUnitId = null*/)
            : base()
        {
            ApplyOrderBy(u => u.EmploymentCode);
        }
    }
}
