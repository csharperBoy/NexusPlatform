using Core.Application.Abstractions.Security;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Core.Infrastructure.Security
{
    public class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CurrentUserService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }
        public Guid? UserId
        {
            get
            {
                var userId = _httpContextAccessor.HttpContext?.User?.FindFirstValue(JwtRegisteredClaimNames.Sub);
                return string.IsNullOrEmpty(userId) ? null : Guid.Parse(userId);
            }
        }
        //public string? UserId => _httpContextAccessor.HttpContext?.User?.FindFirst("sub")?.Value;
        public string? UserName => _httpContextAccessor.HttpContext?.User?.Identity?.Name;
        public bool IsAuthenticated => _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;
        public IEnumerable<string> Roles => _httpContextAccessor.HttpContext?.User?.FindAll("role").Select(r => r.Value) ?? Enumerable.Empty<string>();
    }
}
