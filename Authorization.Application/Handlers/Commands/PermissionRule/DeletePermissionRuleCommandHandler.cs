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
{
    /*
    public class DeletePermissionRuleCommandHandler : IRequestHandler<DeletePermissionRuleCommand, Result<bool>>
    {
        private readonly IPermissionRuleInternalService _permissionRuleService;
        private readonly ILogger<DeletePermissionRuleCommandHandler> _logger;

        public DeletePermissionRuleCommandHandler(
            IPermissionRuleInternalService permissionRuleService,
            ILogger<DeletePermissionRuleCommandHandler> logger)
        {
            _permissionRuleService = permissionRuleService;
            _logger = logger;
        }

        public async Task<Result<bool>> Handle(DeletePermissionRuleCommand request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Delete permissionRule: {PermissionRuleId}", request.Id);

                await _permissionRuleService.DeletePermissionRuleAsync(request.Id);

                _logger.LogInformation("PermissionRule Delete successfully: {PermissionRuleId}", request.Id);

                return Result<bool>.Ok(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to Delete permissionRule: {PermissionRuleId}", request.Id);
                return Result<bool>.Fail(ex.Message);
            }
        }
    }
    */
}
