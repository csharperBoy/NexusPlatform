using Core.Shared.Results;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Authorization.Application.Queries
{

    public record GetEffectivePermissionsQuery(Guid UserId)
        : IRequest<Result<List<string>>>;
}
