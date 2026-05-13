using Authorization.Application.Commands.PermissionRule;
using Authorization.Application.Interfaces.Service;
using Core.Shared.Results;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Authorization.Application.Handlers.Commands.PermissionRule
{/*
    public class UpdatePermissionRuleCommandHandler : IRequestHandler<UpdatePermissionRuleCommand, Result<bool>>
    {
        private readonly IPermissionRuleInternalService _permissionRuleService;
        private readonly ILogger<UpdatePermissionRuleCommandHandler> _logger;

        public UpdatePermissionRuleCommandHandler(
            IPermissionRuleInternalService permissionRuleService,
            ILogger<UpdatePermissionRuleCommandHandler> logger)
        {
            _permissionRuleService = permissionRuleService;
            _logger = logger;
        }

        public async Task<Result<bool>> Handle(UpdatePermissionRuleCommand request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("update permissionRule ");

                await _permissionRuleService.UpdatePermissionRuleAsync(request.Id,
                      request.PermissionId,
                      request.FieldName,
                      request.Operator,
                      request.Value,
                      request.LogicalOperator,
                      request.GroupOrder
                    );

                _logger.LogInformation(
                    "PermissionRule update successfully");

                return Result<bool>.Ok(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to update permissionRule ");

                return Result<bool>.Fail(ex.Message);
            }
        }
    }*/
}
