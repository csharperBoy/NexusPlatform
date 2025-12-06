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
    public class GetUserEffectiveAccessQueryHandler
        : IRequestHandler<GetUserEffectiveAccessQuery, Result<UserAccessDto>>
    {
        private readonly IAuthorizationService _authorizationService;
        private readonly ILogger<GetUserEffectiveAccessQueryHandler> _logger;

        public GetUserEffectiveAccessQueryHandler(
            IAuthorizationService authorizationService,
            ILogger<GetUserEffectiveAccessQueryHandler> logger)
        {
            _authorizationService = authorizationService;
            _logger = logger;
        }

        public async Task<Result<UserAccessDto>> Handle(
            GetUserEffectiveAccessQuery request,
            CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogDebug("Getting effective access for user {UserId}", request.UserId);

                var userAccess = await _authorizationService.GetUserEffectiveAccessAsync(request.UserId);
                return Result<UserAccessDto>.Ok(userAccess);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get effective access for user {UserId}", request.UserId);
                return Result<UserAccessDto>.Fail(ex.Message);
            }
        }
    }
}
