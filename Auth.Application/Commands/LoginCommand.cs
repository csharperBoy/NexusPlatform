using Auth.Application.DTOs;
using Core.Shared.Results;
using MediatR;

namespace Auth.Application.Commands
{
    public record LoginCommand(string Email, string Password)
      : IRequest<Result<AuthResponse>>;

    // خروجی: توکن JWT (یا می‌تونی DTO بسازی به جای string)
}
