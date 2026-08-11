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
    public class GetLocationContactSpec : BaseSpecification<LocationContact?>
    {
        public GetLocationContactSpec(HrContactType contactType, Guid LocationId, string? value = null)
            : base(p =>
                          p.ContactType == contactType && p.FkLocationId == LocationId && p.IsCurrent
            // && ( value == null || p.Value == value )
            )
        {
        }
    }
}
