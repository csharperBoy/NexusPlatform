using Contact.Domain.Entities;
using Core.Domain.Specifications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contact.Domain.Specifications
{
    public class GetLocationContactsByLocationIdsSpec : BaseSpecification<LocationContact>
    {
        public GetLocationContactsByLocationIdsSpec(List<Guid> locationIds)
            : base(p =>
                       locationIds.Any(s=>s == p.FkLocationId )
            // && ( value == null || p.Value == value )
            )
        {
        }
    }
}
