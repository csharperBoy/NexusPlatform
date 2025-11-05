using Authorization.Application.Interfaces;
using Core.Application.Abstractions.Security;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Identity.Infrastructure.Services
{
    public class PermissionChecker : IPermissionChecker
    {
        private readonly IAuthorizationService _authorizationService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<PermissionChecker> _logger;

        public PermissionChecker(
            IAuthorizationService authorizationService,
            IHttpContextAccessor httpContextAccessor,
            ILogger<PermissionChecker> logger)
        {
            _authorizationService = authorizationService;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        public async Task<bool> HasPermissionAsync(string permission)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                _logger.LogWarning("⛔ Cannot check permission '{Permission}' — no authenticated user found.", permission);
                return false;
            }

            var result = await _authorizationService.UserHasPermissionAsync(userId.Value, permission);
            _logger.LogInformation("🔐 Permission check for {UserId} → {Permission} = {Result}", userId, permission, result);
            return result;
        }

        public async Task<bool> HasAnyPermissionAsync(params string[] permissions)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                _logger.LogWarning("⛔ Cannot check multiple permissions — no authenticated user found.");
                return false;
            }

            foreach (var permission in permissions)
            {
                if (await _authorizationService.UserHasPermissionAsync(userId.Value, permission))
                {
                    _logger.LogInformation("✅ User {UserId} has permission '{Permission}'", userId, permission);
                    return true;
                }
            }

            _logger.LogInformation("❌ User {UserId} lacks all tested permissions: {Permissions}", userId, string.Join(", ", permissions));
            return false;
        }

        private Guid? GetCurrentUserId()
        {
            var user = _httpContextAccessor.HttpContext?.User;
            var claim = user?.FindFirst("sub") ?? user?.FindFirst("userId");

            if (claim == null || string.IsNullOrWhiteSpace(claim.Value))
                return null;

            return Guid.TryParse(claim.Value, out var guid) ? guid : null;
        }

    }
}
