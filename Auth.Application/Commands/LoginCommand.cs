using Auth.Application.DTOs;
using Core.Shared.Results;
using MediatR;

namespace Auth.Application.Commands
{
    public record LoginUsernameBaseCommand(string Usename, string Password)
      : IRequest<Result<AuthResponse>>;

    public record LoginEmailBaseCommand(string Email, string Password)
      : IRequest<Result<AuthResponse>>;

}
