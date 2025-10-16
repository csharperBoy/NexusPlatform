using Auth.Infrastructure.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Auth.Infrastructure.Services
{
    public class JwtTokenService : IJwtTokenService
    {
        private readonly IConfiguration _config;
        public JwtTokenService(IConfiguration config) => _config = config;

        public string CreateToken(ApplicationUser user, IEnumerable<string> roles)
        {
            var key = _config["Jwt:Key"];
            var issuer = _config["Jwt:Issuer"];
            var audience = _config["Jwt:Audience"];
            var expiryMinutes = int.Parse(_config["Jwt:ExpiryMinutes"] ?? "60");

            var claims = new List<Claim>
            {
                //new Claim(JwtRegisteredClaimNames.Sub, user.Id),
                new Claim(JwtRegisteredClaimNames.UniqueName, user.UserName ?? user.Email ?? ""),
                new Claim(JwtRegisteredClaimNames.Email, user.Email ?? "")
            };

            claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
            var creds = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer,
                audience,
                claims,
                expires: DateTime.UtcNow.AddMinutes(expiryMinutes),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public string GenerateAccessToken(ApplicationUser user, IEnumerable<string> roles, IEnumerable<Claim>? additionalClaims = null)
        {
            throw new NotImplementedException();
        }

        public Task<RefreshToken> GenerateRefreshTokenAsync(ApplicationUser user, string createdByIp, string? deviceInfo = null)
        {
            throw new NotImplementedException();
        }

        public Task RevokeRefreshTokenAsync(RefreshToken token, string revokedByIp, string? reason = null)
        {
            throw new NotImplementedException();
        }

        public Task<bool> ValidateRefreshTokenAsync(string token, ApplicationUser user)
        {
            throw new NotImplementedException();
        }
    }
}
