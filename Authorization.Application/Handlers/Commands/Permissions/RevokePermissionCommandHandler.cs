using Authorization.Application.Commands.Permissions;
using Authorization.Application.Interfaces;
using Core.Shared.Results;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Authorization.Application.Handlers.Commands.Permissions
{
    public class RevokePermissionCommandHandler : IRequestHandler<RevokePermissionCommand, Result<bool>>
    {
        private readonly IPermissionService _permissionService;
        private readonly ILogger<RevokePermissionCommandHandler> _logger;

        public RevokePermissionCommandHandler(
            IPermissionService permissionService,
            ILogger<RevokePermissionCommandHandler> logger)
        {
            _permissionService = permissionService;
            _logger = logger;
        }

        public async Task<Result<bool>> Handle(RevokePermissionCommand request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Revoking permission: {PermissionId}", request.PermissionId);

                await _permissionService.RevokePermissionAsync(request.PermissionId);

                _logger.LogInformation("Permission revoked successfully: {PermissionId}", request.PermissionId);

                return Result<bool>.Ok(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to revoke permission: {PermissionId}", request.PermissionId);
                return Result<bool>.Fail(ex.Message);
            }
        }
    }
}
