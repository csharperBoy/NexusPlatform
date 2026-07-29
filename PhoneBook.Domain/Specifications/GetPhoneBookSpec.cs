using Core.Domain.Specifications;
using PhoneBook.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhoneBook.Domain.Specifications
{
    public class GetPhoneBookSpec : BaseSpecification<PhoneBookInfoView>
    {
        public GetPhoneBookSpec(/*Guid? orgUnitId = null*/)
            : base()
        {
            ApplyOrderBy(u => u.EmployeeCode);
        }
    }
}
