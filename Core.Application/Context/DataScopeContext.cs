using Core.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Application.Context
{
    public sealed class DataScopeContext
    {
        public Guid UserId { get; init; }
        public Guid? PersonId { get; init; }
          public IReadOnlySet<Guid>? OrganizationUnitId { get; init; }
        = new HashSet<Guid>();
        public IReadOnlySet<ScopeType> AllowedScopes { get; init; }
            = new HashSet<ScopeType>();

        public IReadOnlySet<Guid> AllowedResourceIds { get; init; }
            = new HashSet<Guid>();
    }

}
