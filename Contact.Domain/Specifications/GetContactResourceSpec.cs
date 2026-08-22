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
    
    //public class GetContactResourceSpec : BaseSpecification<ContactResource>
    //{
    //    public GetContactResourceSpec(ContactTypeEnum contactType, Guid profileId)
    //        : base(p => p.ContactType == contactType &&
    //                    p.Assignments.Any(a => a.ContactProfileId == profileId && a.IsCurrent))
    //    {
    //        AddInclude(p => p.ParentContactResource); // دریافت اطلاعات شماره اصلی/پایه
    //        AddInclude(p => p.Assignments.Where(a => a.ContactProfileId == profileId && a.IsCurrent));
    //    }
    //}

    public class GetContactResourceSpec : BaseSpecification<ContactResource>
    {
        public GetContactResourceSpec(ContactTypeEnum contactType, string value)
            : base(r => r.ContactType == contactType && r.Value == value)
        {
        }
    }
}
