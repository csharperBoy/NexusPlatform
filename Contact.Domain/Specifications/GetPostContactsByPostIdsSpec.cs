using Contact.Domain.Entities;
using Core.Domain.Specifications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contact.Domain.Specifications
{
  
    public class GetPostContactsByPostIdsSpec : BaseSpecification<PostContact>
    {
        public GetPostContactsByPostIdsSpec(List<Guid> postIds)
            : base(p =>
                       postIds.Any(s => s == p.FkPostId)
            // && ( value == null || p.Value == value )
            )
        {
        }
    }
}
