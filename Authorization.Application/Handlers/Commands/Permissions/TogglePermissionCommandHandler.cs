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
    public class TogglePermissionCommandHandler : IRequestHandler<TogglePermissionCommand, Result<bool>>
    {
        private readonly IPermissionService _permissionService;
        private readonly ILogger<TogglePermissionCommandHandler> _logger;

        public TogglePermissionCommandHandler(
            IPermissionService permissionService,
            ILogger<TogglePermissionCommandHandler> logger)
        {
            _permissionService = permissionService;
            _logger = logger;
        }

        public async Task<Result<bool>> Handle(TogglePermissionCommand request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation(
                    "Toggling permission {PermissionId} to {IsAllow}",
                    request.PermissionId, request.IsAllow);

                await _permissionService.TogglePermissionAsync(request.PermissionId, request.IsAllow);

                _logger.LogInformation(
                    "Permission toggled successfully: {PermissionId} to {IsAllow}",
                    request.PermissionId, request.IsAllow);

                return Result<bool>.Ok(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to toggle permission {PermissionId} to {IsAllow}",
                    request.PermissionId, request.IsAllow);

                return Result<bool>.Fail(ex.Message);
            }
        }
    }
}
