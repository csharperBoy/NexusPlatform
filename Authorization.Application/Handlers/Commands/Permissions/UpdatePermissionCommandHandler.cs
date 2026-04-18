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
    public class UpdatePermissionCommandHandler : IRequestHandler<UpdatePermissionCommand, Result<bool>>
    {
        private readonly IPermissionInternalService _permissionService;
        private readonly ILogger<UpdatePermissionCommandHandler> _logger;

        public UpdatePermissionCommandHandler(
            IPermissionInternalService permissionService,
            ILogger<UpdatePermissionCommandHandler> logger)
        {
            _permissionService = permissionService;
            _logger = logger;
        }

        public async Task<Result<bool>> Handle(UpdatePermissionCommand request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Toggling permission ");

                await _permissionService.UpdatePermissionAsync(request);

                _logger.LogInformation(
                    "Permission toggled successfully");

                return Result<bool>.Ok(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to toggle permission ");

                return Result<bool>.Fail(ex.Message);
            }
        }
    }
}
