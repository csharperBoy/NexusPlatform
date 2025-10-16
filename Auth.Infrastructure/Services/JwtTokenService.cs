using Auth.Infrastructure.Configuration;
using Auth.Infrastructure.Data;
using Auth.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Auth.Infrastructure.Services
{
    public class JwtTokenService : IJwtTokenService
    {
        private readonly JwtOptions _jwtOptions;
        private readonly AuthDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;

        public JwtTokenService(
            IOptions<JwtOptions> options,
            AuthDbContext db,
            UserManager<ApplicationUser> userManager)
        {
            _jwtOptions = options.Value;
            _db = db;
            _userManager = userManager;
        }

        public async Task<(string AccessToken, string RefreshToken)> GenerateTokensAsync(ApplicationUser user, IEnumerable<string> roles)
        {
            // Claims
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id),
                new Claim(JwtRegisteredClaimNames.UniqueName, user.UserName ?? user.Email ?? ""),
                new Claim(JwtRegisteredClaimNames.Email, user.Email ?? "")
            };
            claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));

            // AccessToken
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.Key));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _jwtOptions.Issuer,
                audience: _jwtOptions.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_jwtOptions.AccessTokenExpiryMinutes),
                signingCredentials: creds
            );
            var accessToken = new JwtSecurityTokenHandler().WriteToken(token);

            // RefreshToken
            var refreshToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));

            // ذخیره RefreshToken در UserSession
            var session = new UserSession
            {
                UserId = user.Id,
                RefreshToken = refreshToken,
                ExpiresAt = DateTime.UtcNow.AddDays(_jwtOptions.RefreshTokenExpiryDays),
                CreatedAt = DateTime.UtcNow,
                DeviceInfo = "DefaultDevice" // بعدا می‌توان از request header گرفت
            };
            _db.UserSessions.Add(session);
            await _db.SaveChangesAsync();

            return (accessToken, refreshToken);
        }

        public async Task<bool> ValidateRefreshTokenAsync(string refreshToken, string userId)
        {
            var session = await _db.UserSessions
                .Where(s => s.UserId == userId && s.RefreshToken == refreshToken && s.ExpiresAt > DateTime.UtcNow)
                .FirstOrDefaultAsync();

            return session != null;
        }

        public async Task RevokeRefreshTokenAsync(string refreshToken)
        {
            var session = await _db.UserSessions
                .FirstOrDefaultAsync(s => s.RefreshToken == refreshToken);

            if (session != null)
            {
                _db.UserSessions.Remove(session);
                await _db.SaveChangesAsync();
            }
        }

        public async Task RevokeAllUserTokensAsync(string userId)
        {
            var sessions = await _db.UserSessions
                .Where(s => s.UserId == userId)
                .ToListAsync();

            if (sessions.Any())
            {
                _db.UserSessions.RemoveRange(sessions);
                await _db.SaveChangesAsync();
            }
        }
    }
}
