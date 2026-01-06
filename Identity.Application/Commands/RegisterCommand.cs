using Core.Shared.Results;
using Identity.Application.DTOs;
using MediatR;

namespace Identity.Application.Commands
{
    public record RegisterCommand(string Username, string Email, string Password, string? DisplayName) : IRequest<Result<AuthResponse>>;
}