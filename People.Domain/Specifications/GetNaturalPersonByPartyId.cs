using Core.Domain.Specifications;
using People.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace People.Domain.Specifications
{
    public class GetNaturalPersonByPartyId : BaseSpecification<NaturalPerson>
    {
        public GetNaturalPersonByPartyId(Guid? partyId = null)
            : base(p =>
                        partyId == null || p.FkPartyId == partyId)
        {
        }
    }
}
