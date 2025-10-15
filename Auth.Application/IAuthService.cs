using Auth.Application.DTOs;
using Core.Shared.Results;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Auth.Application
{
    public interface IAuthService
    {
        Task<Result<AuthResponse>> RegisterAsync(RegisterRequest request);
        Task<Result<AuthResponse>> LoginWithUserNameAsync(LoginRequest request);
        Task<Result<AuthResponse>> LoginWithEmailAsync(LoginRequest request);
    }
}
