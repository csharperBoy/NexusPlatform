using Core.Domain.Specifications;
using HR.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HR.Domain.Specifications
{
    public class GetAllPostInfoViewSpec : BaseSpecification<PostInfoView>
    {
        public GetAllPostInfoViewSpec(Guid? rootId = null)
            : base(u => rootId == null ||  u.FkParentId == rootId)
        {
            ApplyOrderBy(u => u.PostCode);
        }
    }
}
