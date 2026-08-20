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
    
    public class GetContactSpec : BaseSpecification<ContactResource>
    {
        public GetContactSpec(ContactTypeEnum contactType, Guid profileId)
            : base(p => p.ContactType == contactType &&
                        p.Assignments.Any(a => a.ContactProfileId == profileId && a.IsCurrent))
        {
            AddInclude(p => p.ParentContactResource); // دریافت اطلاعات شماره اصلی/پایه
            AddInclude(p => p.Assignments.Where(a => a.ContactProfileId == profileId && a.IsCurrent));
        }
    }
}
