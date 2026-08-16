using Core.Domain.Specifications;
using HR.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HR.Domain.Specifications
{
    public class GetPostLocationsSpec : BaseSpecification<PostLocation?>
    {
        public GetPostLocationsSpec(Guid postId)
            : base(p =>
                          p.FkPostId == postId && p.IsCurrent == true
            // && ( value == null || p.Value == value )
            )
        {
        }
    }
}
