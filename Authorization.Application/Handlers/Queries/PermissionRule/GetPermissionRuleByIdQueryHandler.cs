using Authorization.Application.Interfaces.Service;
using Authorization.Application.Queries.PermissionRule;
using Authorization.Application.Queries.Permissions;
using Core.Shared.DTOs.Authorization;
using Core.Shared.Results;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Authorization.Application.Handlers.Queries.PermissionRule
{
    public class GetPermissionRuleByIdQueryHandler
: IRequestHandler<GetPermissionRuleByIdQuery, Result<PermissionRuleDto>>
    {
        private readonly IPermissionRuleInternalService _PermissionRuleService;
        private readonly ILogger<GetPermissionRuleByIdQueryHandler> _logger;

        public GetPermissionRuleByIdQueryHandler(
            IPermissionRuleInternalService PermissionRuleService,
            ILogger<GetPermissionRuleByIdQueryHandler> logger)
        {
            _PermissionRuleService = PermissionRuleService;
            _logger = logger;
        }

        public async Task<Result<PermissionRuleDto>> Handle(
            GetPermissionRuleByIdQuery request,
            CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogDebug("Getting PermissionRule by ID: {PermissionRuleId}", request.Id);

                PermissionRuleDto? result = await _PermissionRuleService.GetById(request.Id);

                if (result == null)
                {
                    return Result<PermissionRuleDto>.Fail($"PermissionRule with ID {request.Id} not found");
                }


                return Result<PermissionRuleDto>.Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get PermissionRule by ID: {PermissionRuleId}", request.Id);
                return Result<PermissionRuleDto>.Fail(ex.Message);
            }
        }
    }
}
