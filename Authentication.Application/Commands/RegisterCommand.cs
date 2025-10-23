using Authentication.Application.DTOs;
using Core.Shared.Results;
using MediatR;

namespace Authentication.Application.Commands
{
    public record RegisterCommand(string Email, string Password, string? DisplayName) : IRequest<Result<AuthResponse>>;

}
