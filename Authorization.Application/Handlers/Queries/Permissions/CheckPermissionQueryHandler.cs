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
    public class CheckPermissionQueryHandler
        : IRequestHandler<CheckPermissionQuery, Result<bool>>
    {
        private readonly IAuthorizationService _authorizationService;
        private readonly ILogger<CheckPermissionQueryHandler> _logger;

        public CheckPermissionQueryHandler(
            IAuthorizationService authorizationService,
            ILogger<CheckPermissionQueryHandler> logger)
        {
            _authorizationService = authorizationService;
            _logger = logger;
        }

        public async Task<Result<bool>> Handle(
            CheckPermissionQuery request,
            CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogDebug(
                    "Checking permission for user {UserId} on {ResourceKey}:{Action}",
                    request.UserId, request.ResourceKey, request.Action);

                var hasAccess = await _authorizationService.CheckAccessAsync(
                    request.UserId, request.ResourceKey, request.Action);

                return Result<bool>.Ok(hasAccess);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to check permission for user {UserId} on {ResourceKey}:{Action}",
                    request.UserId, request.ResourceKey, request.Action);

                return Result<bool>.Fail(ex.Message);
            }
        }
    }
}
