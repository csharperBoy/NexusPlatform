using Authorization.Application.Interfaces.Service;
using Authorization.Application.Queries.PermissionRule;
using Authorization.Application.Queries.Permissions;
using Core.Shared.DTOs.Authorization;
using Core.Shared.Results;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Authorization.Application.Handlers.Queries.PermissionRule
{/*
    public class GetPermissionRulesQueryHandler : IRequestHandler<GetPermissionRulesQuery, Result<IList<PermissionRuleDto>>>
    {
        private readonly IPermissionRuleInternalService _permissionRuleService;
        public GetPermissionRulesQueryHandler(IPermissionRuleInternalService permissionRuleService)
            => _permissionRuleService = permissionRuleService;

        public async Task<Result<IList<PermissionRuleDto>>> Handle(GetPermissionRulesQuery request, CancellationToken ct)
        {
            IReadOnlyList<PermissionRuleDto> permissionRules = await _permissionRuleService.GetPermissionRules(request.permissionId);
            return Result<IList<PermissionRuleDto>>.Ok(permissionRules.ToList());
        }
    }
    */
}
