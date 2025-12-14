using Core.Application.Abstractions;
using Core.Application.Abstractions.Events;
using Core.Application.Abstractions.Security;
using Core.Domain.ValueObjects;
using Core.Shared.Results;
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
        private readonly IOutboxService<IdentityDbContext> _outboxService;
        private readonly ILogger<AuthService> _logger;
        private readonly IRoleResolver _roleResolver;

        private readonly IRepository<IdentityDbContext, RefreshToken, Guid> _refreshTokenRepository;
        private readonly ISpecificationRepository<RefreshToken, Guid> _refreshTokenSpecRepository;

        private readonly IUnitOfWork<IdentityDbContext> _unitOfWork;
        public AuthService(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IJwtTokenService tokenService,
            IOptions<JwtOptions> jwtOptions,
            IOutboxService<IdentityDbContext> outboxService,
            IRoleResolver roleResolver,
            IRepository<IdentityDbContext, RefreshToken, Guid> refreshTokenRepository,
             ISpecificationRepository<RefreshToken, Guid> refreshTokenSpecRepository,
            IUnitOfWork<IdentityDbContext> unitOfWork,
            ILogger<AuthService> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _tokenService = tokenService;
            _jwtOptions = jwtOptions.Value;
            _outboxService = outboxService;
            _roleResolver = roleResolver;
            _refreshTokenRepository = refreshTokenRepository;
            _refreshTokenSpecRepository = refreshTokenSpecRepository;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<Result<AuthResponse>> RegisterAsync(RegisterRequest request)
        {
            var user = new ApplicationUser(Guid.NewGuid(), request.Username, request.Email);
            user.SetFullName(request.DisplayName, request.DisplayName);
            
            var createRes = await _userManager.CreateAsync(user, request.Password);
            if (!createRes.Succeeded)
            {
                var err = string.Join("; ", createRes.Errors.Select(e => e.Description));
                return Result<AuthResponse>.Fail(err);
            }

            var userRegisteredEvent = new UserRegisteredEvent(
                user.Id,
                user.UserName ?? "",
                user.Email ?? "");

            await _outboxService.AddEventsAsync(new[] { userRegisteredEvent });

            var roles = await _roleResolver.GetUserRolesAsync(user.Id);
            var tokens = await _tokenService.GenerateTokensAsync(user, roles);

            // ذخیره RefreshToken در دیتابیس
            var refreshEntity = new RefreshToken
            (
                 user.Id,
                 tokens.RefreshToken,
                 DateTime.UtcNow.AddDays(_jwtOptions.RefreshTokenExpiryDays),
                 "system", // TODO: از HttpContext بگیر
                 "unknown"
            );
            await _refreshTokenRepository.AddAsync(refreshEntity);
            await _unitOfWork.SaveChangesAsync();

            var response = new AuthResponse(
                tokens.AccessToken,tokens.RefreshToken,user.Id,
                DateTime.UtcNow.AddMinutes(_jwtOptions.AccessTokenExpiryMinutes),
                user.UserName ?? "");

            _logger.LogInformation("User {Email} registered successfully. Event published.", user.Email);

            return Result<AuthResponse>.Ok(response);
        }

        public async Task<Result<AuthResponse>> LoginWithEmailAsync(LoginRequest request)
        {
            var user = await _userManager.FindByEmailAsync(request.UserIdentifier);
            if (user == null) return Result<AuthResponse>.Fail("کاربری با این ایمیل پیدا نشد.");

            var signRes = await _signInManager.CheckPasswordSignInAsync(user, request.Password, false);
            if (!signRes.Succeeded) return Result<AuthResponse>.Fail("اطلاعات ورود نامعتبر است.");

            var roles = await _roleResolver.GetUserRolesAsync(user.Id);
            var tokens = await _tokenService.GenerateTokensAsync(user, roles);

            // ذخیره RefreshToken
            var refreshEntity = new RefreshToken(
                user.Id,
                 tokens.RefreshToken,
                 DateTime.UtcNow.AddDays(_jwtOptions.RefreshTokenExpiryDays),
                 "system",
                "unknown"
            );
            await _refreshTokenRepository.AddAsync(refreshEntity);
            await _unitOfWork.SaveChangesAsync();

            var response = new AuthResponse(
                tokens.AccessToken,tokens.RefreshToken,user.Id,
                DateTime.UtcNow.AddMinutes(_jwtOptions.AccessTokenExpiryMinutes),
                user.Email ?? "");

            return Result<AuthResponse>.Ok(response);
        }

        public async Task<Result<AuthResponse>> LoginWithUserNameAsync(LoginRequest request)
        {
            var user = await _userManager.FindByNameAsync(request.UserIdentifier);
            if (user == null)
                return Result<AuthResponse>.Fail("کاربری با این نام کاربری پیدا نشد.");

            var signRes = await _signInManager.CheckPasswordSignInAsync(user, request.Password, false);
            if (!signRes.Succeeded)
                return Result<AuthResponse>.Fail("اطلاعات ورود نامعتبر است.");

            var roles = await _roleResolver.GetUserRolesAsync(user.Id);
            var tokens = await _tokenService.GenerateTokensAsync(user, roles);

            // ذخیره RefreshToken
            var refreshEntity = new RefreshToken
            (
                user.Id,
                tokens.RefreshToken,
                DateTime.UtcNow.AddDays(_jwtOptions.RefreshTokenExpiryDays),
                "system",
                "unknown"
           );
            await _refreshTokenRepository.AddAsync(refreshEntity);
            await _unitOfWork.SaveChangesAsync();

            var response = new AuthResponse(
                tokens.AccessToken,tokens.RefreshToken,user.Id,
                DateTime.UtcNow.AddMinutes(_jwtOptions.AccessTokenExpiryMinutes),
                user.UserName ?? "");

            return Result<AuthResponse>.Ok(response);
        }

        public async Task<Result<AuthTokens>> RefreshTokenAsync(RefreshTokenRequest request)
        {
            var spec = new RefreshTokenByValueSpec(request.RefreshToken);
            var refresh = await _refreshTokenSpecRepository.GetBySpecAsync(spec);


            if (refresh == null || refresh.IsRevoked || refresh.ExpiryDate < DateTime.UtcNow)
                return Result<AuthTokens>.Fail("Refresh token نامعتبر یا منقضی است.");

            var user = await _userManager.FindByIdAsync(refresh.UserId.ToString());
            if (user == null)
                return Result<AuthTokens>.Fail("کاربر یافت نشد.");

            var roles = await _roleResolver.GetUserRolesAsync(user.Id);
            var tokens = await _tokenService.GenerateTokensAsync(user, roles);

            refresh.Revoke(replacedBy: tokens.RefreshToken);

            // ذخیره جدید
            var newRefresh = new RefreshToken
            (
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

        public async Task<Result> LogoutAsync(LogoutRequest request)
        {
            var spec = new RefreshTokenByValueSpec(request.RefreshToken);
            var refresh = await _refreshTokenSpecRepository.GetBySpecAsync(spec);

            if (refresh == null)
                return Result.Fail("Refresh token یافت نشد.");


            refresh.Revoke(replacedBy: null);

            await _refreshTokenRepository.UpdateAsync(refresh);
            await _unitOfWork.SaveChangesAsync();
            return Result.Ok();
        }
    }
}
