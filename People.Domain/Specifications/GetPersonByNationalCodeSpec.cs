using Core.Domain.Specifications;
using People.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace People.Domain.Specifications
{
    
    public class GetPersonByNationalCodeSpec : BaseSpecification<NaturalPerson>
    {
        public GetPersonByNationalCodeSpec(string NationalCode )
            : base(p =>
                        p.NationalCode.Value == NationalCode)
        {
        }
    }
}
