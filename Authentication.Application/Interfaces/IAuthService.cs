using Authentication.Application.DTOs;
using Core.Shared.Results;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Authentication.Application.Interfaces
{
    public interface IAuthService
    {
        Task<Result<AuthResponse>> RegisterAsync(RegisterRequest request);
        Task<Result<AuthResponse>> LoginWithUserNameAsync(LoginRequest request);
        Task<Result<AuthResponse>> LoginWithEmailAsync(LoginRequest request);
    }
}
