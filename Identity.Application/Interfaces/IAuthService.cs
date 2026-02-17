using Core.Shared.Results;
using Identity.Application.Commands;
using Identity.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Identity.Application.Interfaces
{
    public interface IAuthService
    {
        Task<Result<AuthResponse>> RegisterAsync(RegisterRequest request);
        Task<Result<AuthResponse>> LoginAsync(string identifier, string password);
        Task<Result<AuthTokens>> RefreshTokenAsync(RefreshTokenCommand request);
        Task<Result> LogoutAsync(LogoutCommand request);
    }
}