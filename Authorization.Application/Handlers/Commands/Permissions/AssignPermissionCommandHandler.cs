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
    public class AssignPermissionCommandHandler : IRequestHandler<AssignPermissionCommand, Result<Guid>>
    {
        private readonly IPermissionInternalService _permissionService;
        private readonly ILogger<AssignPermissionCommandHandler> _logger;

        public AssignPermissionCommandHandler(
            IPermissionInternalService permissionService,
            ILogger<AssignPermissionCommandHandler> logger)
        {
            _permissionService = permissionService;
            _logger = logger;
        }

        public async Task<Result<Guid>> Handle(AssignPermissionCommand request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation(
                    "Assigning permission for {AssigneeType}:{AssigneeId} to resource {ResourceId}",
                    request.AssigneeType, request.AssigneeId, request.ResourceId);

                var permissionId = await _permissionService.AssignPermissionAsync(request);

                _logger.LogInformation(
                    "Permission assigned successfully: {PermissionId}", permissionId);

                return Result<Guid>.Ok(permissionId);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to assign permission for {AssigneeType}:{AssigneeId} to resource {ResourceId}",
                    request.AssigneeType, request.AssigneeId, request.ResourceId);

                return Result<Guid>.Fail(ex.Message);
            }
        }
    }
}
