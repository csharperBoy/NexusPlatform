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

namespace Auth.Infrastructure.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ITokenService _tokenService;

        public AuthService(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ITokenService tokenService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _tokenService = tokenService;
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
            var token = _tokenService.CreateToken(user, roles);

            var response = new AuthResponse(token, DateTime.UtcNow.AddMinutes(int.Parse(_tokenService is JwtTokenService ? 60.ToString() : "60")), user.Email ?? "");
            return Result<AuthResponse>.Ok(response);
        }

        public async Task<Result<AuthResponse>> LoginWithEmailAsync(LoginRequest request)
        {
            var user = await _userManager.FindByEmailAsync(request.Username);
            if (user == null) return Result<AuthResponse>.Fail("کاربری با این ایمیل پیدا نشد.");

            var signRes = await _signInManager.CheckPasswordSignInAsync(user, request.Password, false);
            if (!signRes.Succeeded) return Result<AuthResponse>.Fail("اطلاعات ورود نامعتبر است.");

            var roles = await _userManager.GetRolesAsync(user);
            var token = _tokenService.CreateToken(user, roles);

            return Result<AuthResponse>.Ok(new AuthResponse(token, DateTime.UtcNow.AddMinutes(int.Parse(_tokenService is JwtTokenService ? 60.ToString() : "60")), user.Email ?? ""));
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
            var token = _tokenService.CreateToken(user, roles);

            return Result<AuthResponse>.Ok(new AuthResponse(
                token,
                DateTime.UtcNow.AddMinutes(int.Parse(_tokenService is JwtTokenService ? 60.ToString() : "60")),
                user.UserName ?? ""  // بازگشت نام کاربری به جای ایمیل
            ));
        }
    }
}
