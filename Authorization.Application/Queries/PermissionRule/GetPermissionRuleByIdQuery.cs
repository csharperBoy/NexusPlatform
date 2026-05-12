using Core.Shared.DTOs.Authorization;
using Core.Shared.Results;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Authorization.Application.Queries.PermissionRule
{
    public record GetPermissionRuleByIdQuery(Guid Id)
       : IRequest<Result<PermissionRuleDto>>;

}
