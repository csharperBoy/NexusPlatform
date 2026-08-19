using Contact.Domain.Entities;
using Core.Domain.Specifications;
using Core.Shared.Enums.HR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contact.Domain.Specifications
{
    public class GetPostContactSpec : BaseSpecification<PostContact?>
    {
        public GetPostContactSpec( HrContactType contactType , Guid postId)
            : base(p =>
                          p.ContactType == contactType && p.FkPostId == postId && p.IsCurrent 
            // && ( value == null || p.Value == value )
            )
        {
        }
    }
}
