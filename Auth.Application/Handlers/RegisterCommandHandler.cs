using Auth.Application.Commands;
using Auth.Application.DTOs;
using Core.Shared.Results;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Auth.Application.Handlers
{
    public class RegisterCommandHandler : IRequestHandler<RegisterCommand, Result<AuthResponse>>
    {
        private readonly IAuthService _authService;
        public RegisterCommandHandler(IAuthService authService) => _authService = authService;

        public async Task<Result<AuthResponse>> Handle(RegisterCommand request, CancellationToken cancellationToken)
        {
            var dto = new DTOs.RegisterRequest(request.Email, request.Password, request.DisplayName);
            return await _authService.RegisterAsync(dto);
        }
    }
}
