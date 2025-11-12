using Core.Shared.Results;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Identity.Application.Queries
{
    public record GetUserRolesQuery(Guid UserId) : IRequest<Result<IList<string>>>;

}
