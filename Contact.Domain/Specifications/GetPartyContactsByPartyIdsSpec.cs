using Contact.Domain.Entities;
using Core.Domain.Specifications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contact.Domain.Specifications
{
   
    public class GetPartyContactsByPartyIdsSpec : BaseSpecification<PartyContact>
    {
        public GetPartyContactsByPartyIdsSpec(List<Guid> partyIds)
            : base(p =>
                       partyIds.Any(s => s == p.FkPartyId)
            // && ( value == null || p.Value == value )
            )
        {
        }
    }
}
