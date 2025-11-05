using Core.Shared.Results;
using Identity.Application.Commands;
using Identity.Application.DTOs;
using Identity.Application.Interfaces;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Identity.Application.Handlers
{
    public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, Result<AuthTokens>>
    {
        private readonly IAuthService _authService;

        public RefreshTokenCommandHandler(IAuthService authService) => _authService = authService;

        public async Task<Result<AuthTokens>> Handle(RefreshTokenCommand request, CancellationToken ct)
        {
            return await _authService.RefreshTokenAsync(new RefreshTokenRequest(request.RefreshToken));
        }
    }

    public class LogoutCommandHandler : IRequestHandler<LogoutCommand, Result>
    {
        private readonly IAuthService _authService;

        public LogoutCommandHandler(IAuthService authService) => _authService = authService;

        public async Task<Result> Handle(LogoutCommand request, CancellationToken ct)
        {
            return await _authService.LogoutAsync(new LogoutRequest(request.RefreshToken));
        }
    }
}
