using Contact.Domain.Entities;
using Core.Domain.Specifications;
using Core.Shared.Enums.HR;
using Core.Shared.Enums.People;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contact.Domain.Specifications
{
   
    public class GetPartyContactSpec : BaseSpecification<PartyContact?>
    {
        public GetPartyContactSpec(PartyContactType contactType, Guid PartyId, List<string>? values = null)
            : base(p =>
                          p.ContactType == contactType && p.FkPartyId == PartyId && p.IsCurrent && values.Contains(p.Value)
            // && ( value == null || p.Value == value )
            )
        {
        }
    }
}
