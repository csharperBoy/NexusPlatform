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
    public class DeletePermissionCommandHandler : IRequestHandler<DeletePermissionCommand, Result<bool>>
    {
        private readonly IPermissionInternalService _permissionService;
        private readonly ILogger<DeletePermissionCommandHandler> _logger;

        public DeletePermissionCommandHandler(
            IPermissionInternalService permissionService,
            ILogger<DeletePermissionCommandHandler> logger)
        {
            _permissionService = permissionService;
            _logger = logger;
        }

        public async Task<Result<bool>> Handle(DeletePermissionCommand request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Delete permission: {PermissionId}", request.Id);

                await _permissionService.DeletePermissionAsync(request.Id);

                _logger.LogInformation("Permission Delete successfully: {PermissionId}", request.Id);

                return Result<bool>.Ok(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to Delete permission: {PermissionId}", request.Id);
                return Result<bool>.Fail(ex.Message);
            }
        }
    }
}
