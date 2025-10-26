using Authentication.Application.DTOs;
using Authentication.Application;
using Core.Shared.Results;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Authentication.Infrastructure.Configuration;
using Microsoft.Extensions.Options;
using Core.Application.Abstractions.Events;
using System.Net;
using Authentication.Application.Interfaces;
using Authentication.Domain.Events;
using Microsoft.Extensions.Logging;
using Authentication.Infrastructure.Data;
using Authentication.Domain.Entities;
using Core.Application.Abstractions.Security;
using Core.Domain.Events;

namespace Authentication.Infrastructure.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IJwtTokenService _tokenService;
        private readonly JwtOptions _jwtOptions;
        private readonly IOutboxService<AuthDbContext> _outboxService;
        private readonly ILogger<AuthService> _logger;
        private readonly IRoleResolver _roleResolver; // abstraction برای گرفتن نقش‌ها

        public AuthService(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IJwtTokenService tokenService,
            IOptions<JwtOptions> jwtOptions,
            IOutboxService<AuthDbContext> outboxService,
            IRoleResolver roleResolver,
            ILogger<AuthService> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _tokenService = tokenService;
            _jwtOptions = jwtOptions.Value;
            _outboxService = outboxService;
            _roleResolver = roleResolver;
            _logger = logger;
        }

        public async Task<Result<AuthResponse>> RegisterAsync(RegisterRequest request)
        {
            var user = new ApplicationUser(Guid.NewGuid() , request.Username , request.Email)
            {
                FullName = request.DisplayName
            };

            // 1. ایجاد کاربر
            var createRes = await _userManager.CreateAsync(user, request.Password);
            if (!createRes.Succeeded)
            {
                var err = string.Join("; ", createRes.Errors.Select(e => e.Description));
                return Result<AuthResponse>.Fail(err);
            }

            // 2. انتشار Event
            var userRegisteredEvent = new UserRegisteredEvent(
                user.Id,
                user.UserName ?? "",
                user.Email ?? "");

            await _outboxService.AddEventsAsync(new[] { userRegisteredEvent });

            // 3. نقش‌ها (ممکن است هنوز خالی باشد)
            var roles = await _roleResolver.GetUserRolesAsync(user.Id);
            var token = await _tokenService.GenerateTokensAsync(user, roles);

            var response = new AuthResponse(
                token.AccessToken,
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
            var token = await _tokenService.GenerateTokensAsync(user, roles);

            var response = new AuthResponse(
                token.AccessToken,
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
            var token = await _tokenService.GenerateTokensAsync(user, roles);

            var response = new AuthResponse(
                token.AccessToken,
                DateTime.UtcNow.AddMinutes(_jwtOptions.AccessTokenExpiryMinutes),
                user.UserName ?? "");

            return Result<AuthResponse>.Ok(response);
        }
    }
}
