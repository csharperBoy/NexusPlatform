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
    public class GetLocationContactSpec : BaseSpecification<LocationContact?>
    {
        public GetLocationContactSpec(HrContactType contactType, Guid LocationId, List<string>? values = null)
            : base(p =>
                          p.ContactType == contactType && p.FkLocationId == LocationId && p.IsCurrent && values.Contains(p.Value)
            // && ( value == null || p.Value == value )
            )
        {
        }
    }
}
