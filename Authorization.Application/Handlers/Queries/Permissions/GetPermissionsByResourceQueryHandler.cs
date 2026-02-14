using Authorization.Application.DTOs.Permissions;
using Authorization.Application.Interfaces;
using Authorization.Application.Queries.Permissions;
using Core.Shared.DTOs.Identity;
using Core.Shared.Results;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Authorization.Application.Handlers.Queries.Permissions
{
    public class GetPermissionsByResourceQueryHandler
        : IRequestHandler<GetPermissionsByResourceQuery, Result<IReadOnlyList<PermissionDto>>>
    {
        private readonly IPermissionInternalService _permissionService;
        private readonly ILogger<GetPermissionsByResourceQueryHandler> _logger;

        public GetPermissionsByResourceQueryHandler(
            IPermissionInternalService permissionService,
            ILogger<GetPermissionsByResourceQueryHandler> logger)
        {
            _permissionService = permissionService;
            _logger = logger;
        }

        public async Task<Result<IReadOnlyList<PermissionDto>>> Handle(
            GetPermissionsByResourceQuery request,
            CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogDebug("Getting permissions for resource {ResourceId}", request.ResourceId);

                // نیاز به متد جدید در IPermissionService داریم: GetPermissionsByResourceAsync
                var permissions = await _permissionService.GetUserPermissionsAsync(request.ResourceId);
                return Result<IReadOnlyList<PermissionDto>>.Ok(permissions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get permissions for resource {ResourceId}", request.ResourceId);
                return Result<IReadOnlyList<PermissionDto>>.Fail(ex.Message);
            }
        }
    }
}
