using Authorization.Application.DTOs.Permissions;
using Authorization.Application.Interfaces;
using Authorization.Application.Queries.Users;
using Core.Shared.Results;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Authorization.Application.Handlers.Queries.Users
{
    public class GetUserPermissionsByResourceQueryHandler
      : IRequestHandler<GetUserPermissionsByResourceQuery, Result<EffectivePermissionDto>>
    {
        private readonly IPermissionEvaluator _permissionEvaluator;
        private readonly ILogger<GetUserPermissionsByResourceQueryHandler> _logger;

        public GetUserPermissionsByResourceQueryHandler(
            IPermissionEvaluator permissionEvaluator,
            ILogger<GetUserPermissionsByResourceQueryHandler> logger)
        {
            _permissionEvaluator = permissionEvaluator;
            _logger = logger;
        }

        public async Task<Result<EffectivePermissionDto>> Handle(
            GetUserPermissionsByResourceQuery request,
            CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogDebug(
                    "Getting user permissions for user {UserId} on resource {ResourceKey}",
                    request.UserId, request.ResourceKey);

                var permissions = await _permissionEvaluator.EvaluateUserPermissionsAsync(
                    request.UserId, request.ResourceKey);

                return Result<EffectivePermissionDto>.Ok(permissions);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to get user permissions for user {UserId} on resource {ResourceKey}",
                    request.UserId, request.ResourceKey);

                return Result<EffectivePermissionDto>.Fail(ex.Message);
            }
        }
    }
}
