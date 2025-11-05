using Core.Shared.Results;
using Identity.Application.DTOs;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Identity.Application.Commands
{
    // موجود: RegisterCommand, LoginUsernameBaseCommand, LoginEmailBaseCommand

    public record RefreshTokenCommand(string RefreshToken)
        : IRequest<Result<AuthTokens>>;

    public record LogoutCommand(string RefreshToken)
        : IRequest<Result>;
}
