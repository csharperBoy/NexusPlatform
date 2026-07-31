using Core.Domain.Specifications;
using Core.Shared.Enums.HR;
using HR.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HR.Domain.Specifications
{
    public class GetPostContactSpec : BaseSpecification<PostContact?>
    {
        public GetPostContactSpec( HrContactType contactType , Guid postId, string? value = null)
            : base(p =>
                          p.ContactType == contactType && p.FkPostId == postId 
           // && ( value == null || p.Value == value )
            )
        {
        }
    }
}
