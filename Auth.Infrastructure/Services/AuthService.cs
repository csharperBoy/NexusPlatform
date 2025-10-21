using Auth.Application.DTOs;
using Auth.Application;
using Auth.Infrastructure.Identity;
using Core.Shared.Results;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Auth.Infrastructure.Configuration;
using Microsoft.Extensions.Options;
using Core.Application.Abstractions.Events;
using System.Net;

namespace Auth.Infrastructure.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IJwtTokenService _tokenService;
        private readonly JwtOptions _jwtOptions;
        private readonly IEventBus _eventBus;
        public AuthService(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IJwtTokenService tokenService, IOptions<JwtOptions> jwtOptions,
            IEventBus eventBus)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _tokenService = tokenService;
            _eventBus = eventBus;
            _jwtOptions = jwtOptions.Value;
        }

        public async Task<Result<AuthResponse>> RegisterAsync(RegisterRequest request)
        {
            var user = new ApplicationUser
            {
                UserName = request.Email,
                Email = request.Email,
                FullName = request.DisplayName
            };

            var createRes = await _userManager.CreateAsync(user, request.Password);
            if (!createRes.Succeeded)
            {
                var err = string.Join("; ", createRes.Errors.Select(e => e.Description));
                return Result<AuthResponse>.Fail(err);
            }

            // optionally add default role
            await _userManager.AddToRoleAsync(user, "User");

            var roles = await _userManager.GetRolesAsync(user);
            var token =await _tokenService.GenerateTokensAsync(user, roles);

            var response = new AuthResponse(
                token.AccessToken,
               DateTime.UtcNow.AddMinutes(_jwtOptions.AccessTokenExpiryMinutes), 
               user.UserName ?? "");
            return Result<AuthResponse>.Ok(response);
        }

        public async Task<Result<AuthResponse>> LoginWithEmailAsync(LoginRequest request)
        {
            var user = await _userManager.FindByEmailAsync(request.Username);
            if (user == null) return Result<AuthResponse>.Fail("کاربری با این ایمیل پیدا نشد.");

            var signRes = await _signInManager.CheckPasswordSignInAsync(user, request.Password, false);
            if (!signRes.Succeeded) return Result<AuthResponse>.Fail("اطلاعات ورود نامعتبر است.");

            var roles = await _userManager.GetRolesAsync(user);
            var token = await _tokenService.GenerateTokensAsync(user, roles);

            var response = new AuthResponse(
                token.AccessToken,
               DateTime.UtcNow.AddMinutes(_jwtOptions.AccessTokenExpiryMinutes),
               user.Email ?? "");
           

            return Result<AuthResponse>.Ok(response);
        }
        public async Task<Result<AuthResponse>> LoginWithUserNameAsync(LoginRequest request)
        {
            // پیدا کردن کاربر با نام کاربری
            var user = await _userManager.FindByNameAsync(request.Username);
            if (user == null)
                return Result<AuthResponse>.Fail("کاربری با این نام کاربری پیدا نشد.");

            var signRes = await _signInManager.CheckPasswordSignInAsync(user, request.Password, false);
            if (!signRes.Succeeded)
                return Result<AuthResponse>.Fail("اطلاعات ورود نامعتبر است.");

            var roles = await _userManager.GetRolesAsync(user);
            var token = await _tokenService.GenerateTokensAsync(user, roles);

            var response = new AuthResponse(
                token.AccessToken,
               DateTime.UtcNow.AddMinutes(_jwtOptions.AccessTokenExpiryMinutes),
               user.UserName ?? "");

            return Result<AuthResponse>.Ok(response);
        }
    }
}
