using Authentication.Infrastructure.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Authentication.Infrastructure.Services
{
    public interface IJwtTokenService
    {
        Task<(string AccessToken, string RefreshToken)> GenerateTokensAsync(ApplicationUser user, IEnumerable<string> roles);
        Task<bool> ValidateRefreshTokenAsync(string refreshToken, string userId);
        Task RevokeRefreshTokenAsync(string refreshToken);
        Task RevokeAllUserTokensAsync(string userId);
    }
}
