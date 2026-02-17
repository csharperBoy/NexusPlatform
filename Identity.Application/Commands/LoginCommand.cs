using Core.Shared.Results;
using Identity.Application.DTOs;
using MediatR;

namespace Identity.Application.Commands
{
    public record LoginCommand(string UserIdentifier, string Password)
    : IRequest<Result<AuthResponse>>;

}
