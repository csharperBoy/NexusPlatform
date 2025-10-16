using Auth.Infrastructure.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Auth.Infrastructure.Services
{
    public interface IJwtTokenService
    {
        string GenerateAccessToken(ApplicationUser user, IEnumerable<string> roles, IEnumerable<System.Security.Claims.Claim>? additionalClaims = null);
        Task<RefreshToken> GenerateRefreshTokenAsync(ApplicationUser user, string createdByIp, string? deviceInfo = null);
        Task RevokeRefreshTokenAsync(RefreshToken token, string revokedByIp, string? reason = null);
        Task<bool> ValidateRefreshTokenAsync(string token, ApplicationUser user);
    }
}
