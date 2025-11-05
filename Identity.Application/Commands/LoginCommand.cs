using Core.Shared.Results;
using Identity.Application.DTOs;
using MediatR;

namespace Identity.Application.Commands
{
    public record LoginUsernameBaseCommand(string Usename, string Password)
      : IRequest<Result<AuthResponse>>;

    public record LoginEmailBaseCommand(string Email, string Password)
      : IRequest<Result<AuthResponse>>;

}
