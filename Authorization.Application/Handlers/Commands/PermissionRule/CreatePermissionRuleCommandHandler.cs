using Authorization.Application.Commands.PermissionRule;
using Authorization.Application.Commands.Permissions;
using Authorization.Application.Interfaces.Service;
using Authorization.Domain.Entities;
using Core.Shared.Enums.Authorization;
using Core.Shared.Results;
using MediatR;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Authorization.Application.Handlers.Commands.PermissionRule
{/*
    public class CreatePermissionRuleCommandHandler : IRequestHandler<CreatePermissionRuleCommand, Result<Guid>>
    {
        private readonly IPermissionRuleInternalService _permissionRuleService;
        private readonly ILogger<CreatePermissionRuleCommandHandler> _logger;

        public CreatePermissionRuleCommandHandler(
            IPermissionRuleInternalService permissionRuleService,
            ILogger<CreatePermissionRuleCommandHandler> logger)
        {
            _permissionRuleService = permissionRuleService;
            _logger = logger;
        }

        public async Task<Result<Guid>> Handle(CreatePermissionRuleCommand request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation(
                    "create permission rule  for {PermissionId}",
                    request.PermissionId);

                Guid permissionRuleId = await _permissionRuleService.CreateRuleAsync(
                      request.PermissionId,
                      request.FieldName,
                      request.Operator,
                      request.Value,
                      request.LogicalOperator,
                      request.GroupOrder
                    );

                _logger.LogInformation(
                    "Permission Rule Create successfully: {permissionRuleId}", permissionRuleId);

                return Result<Guid>.Ok(permissionRuleId);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to create permission rule  for {permissionId}",
                    request.PermissionId);

                return Result<Guid>.Fail(ex.Message);
            }
        }
    }
    */
}
