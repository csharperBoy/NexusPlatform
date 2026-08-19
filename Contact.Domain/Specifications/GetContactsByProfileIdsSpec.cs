using Contact.Domain.Entities;
using Core.Domain.Specifications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contact.Domain.Specifications
{
  
    public class GetContactsByProfileIdsSpec : BaseSpecification<ContactItem>
    {
        public GetContactsByProfileIdsSpec(List<Guid> profileIds)
            : base(p =>
                       profileIds.Any(s => s == p.ContactProfileId)
            // && ( value == null || p.Value == value )
            )
        {
        }
    }
}
