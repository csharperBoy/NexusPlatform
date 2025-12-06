using Authorization.Application.DTOs.Permissions;
using Authorization.Application.Interfaces;
using Authorization.Application.Queries.Permissions;
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
    public class GetPermissionsByUserQueryHandler
         : IRequestHandler<GetPermissionsByUserQuery, Result<IReadOnlyList<PermissionDto>>>
    {
        private readonly IPermissionService _permissionService;
        private readonly ILogger<GetPermissionsByUserQueryHandler> _logger;

        public GetPermissionsByUserQueryHandler(
            IPermissionService permissionService,
            ILogger<GetPermissionsByUserQueryHandler> logger)
        {
            _permissionService = permissionService;
            _logger = logger;
        }

        public async Task<Result<IReadOnlyList<PermissionDto>>> Handle(
            GetPermissionsByUserQuery request,
            CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogDebug("Getting permissions for user {UserId}", request.UserId);

                var permissions = await _permissionService.GetUserPermissionsAsync(request.UserId);
                return Result<IReadOnlyList<PermissionDto>>.Ok(permissions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get permissions for user {UserId}", request.UserId);
                return Result<IReadOnlyList<PermissionDto>>.Fail(ex.Message);
            }
        }
    }
}
