using Core.Application.Abstractions;
using Identity.Domain.Entities;
using Identity.Infrastructure.Configuration;
using Identity.Infrastructure.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
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

namespace Identity.Infrastructure.Services
{
    public class JwtTokenService : IJwtTokenService
    {
        private readonly JwtOptions _jwtOptions;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IRepository<IdentityDbContext, UserSession, Guid> _sessionRepository;
        private readonly IUnitOfWork<IdentityDbContext> _unitOfWork;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public JwtTokenService(
            IOptions<JwtOptions> options,
            UserManager<ApplicationUser> userManager,
            IRepository<IdentityDbContext, UserSession, Guid> sessionRepository,
            IUnitOfWork<IdentityDbContext> unitOfWork,
            IHttpContextAccessor httpContextAccessor)
        {
            _jwtOptions = options.Value;
            _userManager = userManager;
            _sessionRepository = sessionRepository;
            _unitOfWork = unitOfWork;
            _httpContextAccessor = httpContextAccessor;
        }
        private string GetDeviceInfo()
        {
            return _httpContextAccessor.HttpContext?.Request.Headers["User-Agent"].ToString()
                   ?? "Unknown-Device";
        }

        private string GetClientIp()
        {
            return _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString()
                   ?? "Unknown-IP";
        }
        public async Task<(string AccessToken, string RefreshToken)> GenerateTokensAsync(ApplicationUser user, IEnumerable<string> roles)
        {
            // Claims
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.UniqueName, user.UserName ?? user.Email ?? ""),
                new Claim(JwtRegisteredClaimNames.Email, user.Email ?? ""),

                new Claim(ClaimTypes.Role, string.Join(",", roles)) 
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
                DeviceInfo = GetDeviceInfo()
            };

            await _sessionRepository.AddAsync(session);
            await _unitOfWork.SaveChangesAsync();

            return (accessToken, refreshToken);
        }

        public async Task<bool> ValidateRefreshTokenAsync(string refreshToken, string userId)
        {
            var userGuid = Guid.Parse(userId);
            var session = await _sessionRepository.AsQueryable()
                                .FirstOrDefaultAsync(s => s.UserId == userGuid &&
                               s.RefreshToken == refreshToken &&
                               s.ExpiresAt > DateTime.UtcNow);


            return session != null;
        }

        public async Task RevokeRefreshTokenAsync(string refreshToken)
        {
            var session = await _sessionRepository.AsQueryable().FirstOrDefaultAsync(s => s.RefreshToken == refreshToken);

            if (session != null)
            {
                await _sessionRepository.DeleteAsync(session);
                await _unitOfWork.SaveChangesAsync();
            }
        }

        public async Task RevokeAllUserTokensAsync(string userId)
        {

            var userGuid = Guid.Parse(userId);
            var sessions = await _sessionRepository.AsQueryable()
                                                     .Where(s => s.UserId == userGuid)
                                                     .ToListAsync();

            if (sessions.Any())
            {
                await _sessionRepository.RemoveRangeAsync(sessions);
                await _unitOfWork.SaveChangesAsync();
            }

        }

    }
}