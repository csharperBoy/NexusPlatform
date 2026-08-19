using Contact.Domain.Entities;
using Core.Domain.Specifications;
using Core.Shared.Enums.Contact;
using Core.Shared.Enums.HR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contact.Domain.Specifications
{
    
    public class GetContactSpec : BaseSpecification<ContactItem?>
    {
        public GetContactSpec(ContactTypeEnum contactType, Guid profileId)
            : base(p =>
                          p.ContactType == contactType && p.ContactProfileId == profileId && p.IsCurrent  
            // && ( value == null || p.Value == value )
            )
        {
        }
    }
}
