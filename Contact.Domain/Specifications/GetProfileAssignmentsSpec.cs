using Contact.Domain.Entities;
using Core.Domain.Specifications;
using Core.Shared.Enums.Contact;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contact.Domain.Specifications
{
    public class GetProfileAssignmentsSpec : BaseSpecification<ContactProfileAssignment>
    {
        public GetProfileAssignmentsSpec(ContactTypeEnum contactType, Guid profileId)
            : base(a => a.ContactProfileId == profileId &&
                        a.IsCurrent &&
                        a.ContactResource.ContactType == contactType)
        {
            AddInclude(a => a.ContactResource);
            AddInclude(a => a.ContactResource.ParentContactResource); // در صورت نیاز به اطلاعات شماره پایه
        }
    }
}
