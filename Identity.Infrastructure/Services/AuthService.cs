using Core.Application.Abstractions;
using Core.Application.Abstractions.Events;
using Core.Application.Abstractions.Security;
using Core.Domain.ValueObjects;
using Core.Shared.Results;
using Identity.Application.Commands;
using Identity.Application.DTOs;
using Identity.Application.Interfaces;
using Identity.Domain.Entities;
using Identity.Domain.Specifications;
using Identity.Infrastructure.Configuration;
using Identity.Infrastructure.Data;
using Identity.Shared.Events;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Identity.Infrastructure.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IJwtTokenService _tokenService;
        private readonly JwtOptions _jwtOptions;
        private readonly IRepository<IdentityDbContext, RefreshToken, Guid> _refreshTokenRepository;
        private readonly ISpecificationRepository<RefreshToken, Guid> _refreshTokenSpecRepository;
        private readonly IUnitOfWork<IdentityDbContext> _unitOfWork;
        private readonly IRoleInternalService _roleService;

        public AuthService(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IJwtTokenService tokenService,
            IOptions<JwtOptions> jwtOptions,
            IRepository<IdentityDbContext, RefreshToken, Guid> refreshTokenRepository,
            ISpecificationRepository<RefreshToken, Guid> refreshTokenSpecRepository,
            IUnitOfWork<IdentityDbContext> unitOfWork,
            IRoleInternalService roleService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _tokenService = tokenService;
            _jwtOptions = jwtOptions.Value;
            _refreshTokenRepository = refreshTokenRepository;
            _refreshTokenSpecRepository = refreshTokenSpecRepository;
            _unitOfWork = unitOfWork;
            _roleService = roleService;
        }

        private async Task<Result<AuthResponse>> GenerateAuthResponse(ApplicationUser user)
        {
            var roles = await _roleService.GetUserRolesAsync(user.Id);
            var tokens = await _tokenService.GenerateTokensAsync(user, roles);

            var refreshEntity = new RefreshToken(
                user.Id,
                tokens.RefreshToken,
                DateTime.UtcNow.AddDays(_jwtOptions.RefreshTokenExpiryDays),
                "system",
                "unknown"
            );

            await _refreshTokenRepository.AddAsync(refreshEntity);
            await _unitOfWork.SaveChangesAsync();

            return Result<AuthResponse>.Ok(new AuthResponse(
                tokens.AccessToken,
                tokens.RefreshToken,
                user.Id,
                DateTime.UtcNow.AddMinutes(_jwtOptions.AccessTokenExpiryMinutes),
                user.UserName ?? ""
            ));
        }

        public async Task<Result<AuthResponse>> RegisterAsync(RegisterRequest request)
        {
            var user = new ApplicationUser(Guid.NewGuid(), request.Username, request.Email);
            user.SetFullName(request.DisplayName, request.DisplayName);

            var createRes = await _userManager.CreateAsync(user, request.Password);
            if (!createRes.Succeeded)
                return Result<AuthResponse>.Fail(string.Join("; ", createRes.Errors.Select(e => e.Description)));

            return await GenerateAuthResponse(user);
        }
        public async Task<Result<AuthResponse>> LoginAsync(string identifier, string password)
        {
            ApplicationUser? user;

            if (identifier.Contains("@"))
                user = await _userManager.FindByEmailAsync(identifier);
            else
                user = await _userManager.FindByNameAsync(identifier);

            if (user == null)
                return Result<AuthResponse>.Fail("کاربر یافت نشد.");

            var signRes = await _signInManager.CheckPasswordSignInAsync(user, password, false);
            if (!signRes.Succeeded)
                return Result<AuthResponse>.Fail("اطلاعات ورود نامعتبر است.");

            return await GenerateAuthResponse(user);
        }
        
        public async Task<Result<AuthTokens>> RefreshTokenAsync(RefreshTokenCommand request)
        {
            var spec = new RefreshTokenByValueSpec(request.RefreshToken);
            var refresh = await _refreshTokenSpecRepository.GetBySpecAsync(spec);

            if (refresh == null || refresh.IsRevoked || refresh.ExpiryDate < DateTime.UtcNow)
                return Result<AuthTokens>.Fail("Refresh token نامعتبر یا منقضی است.");

            var user = await _userManager.FindByIdAsync(refresh.UserId.ToString());
            if (user == null)
                return Result<AuthTokens>.Fail("کاربر یافت نشد.");

            var roles = await _roleService.GetUserRolesAsync(user.Id);
            var tokens = await _tokenService.GenerateTokensAsync(user, roles);

            refresh.Revoke(tokens.RefreshToken);

            var newRefresh = new RefreshToken(
                user.Id,
                tokens.RefreshToken,
                DateTime.UtcNow.AddDays(_jwtOptions.RefreshTokenExpiryDays),
                "system",
                "unknown"
            );

            await _refreshTokenRepository.UpdateAsync(refresh);
            await _refreshTokenRepository.AddAsync(newRefresh);
            await _unitOfWork.SaveChangesAsync();

            return Result<AuthTokens>.Ok(new AuthTokens(tokens.AccessToken, tokens.RefreshToken));
        }

        public async Task<Result> LogoutAsync(LogoutCommand request)
        {
            var spec = new RefreshTokenByValueSpec(request.RefreshToken);
            var refresh = await _refreshTokenSpecRepository.GetBySpecAsync(spec);

            if (refresh != null)
            {
                refresh.Revoke(null);
                await _refreshTokenRepository.UpdateAsync(refresh);
                await _unitOfWork.SaveChangesAsync();
            }

            return Result.Ok();
        }
    }

}
