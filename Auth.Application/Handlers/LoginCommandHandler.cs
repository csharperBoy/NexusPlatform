﻿using Auth.Application.Commands;
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
    public class LoginUsernameBaseCommandHandler : IRequestHandler<LoginUsernameBaseCommand, Result<AuthResponse>>
    {
        private readonly IAuthService _authService;

        public LoginUsernameBaseCommandHandler(IAuthService authService)
        {
            _authService = authService;
        }

        public async Task<Result<AuthResponse>> Handle(LoginUsernameBaseCommand request, CancellationToken cancellationToken)
        {
            return await _authService.LoginWithUserNameAsync(new LoginRequest(request.Usename, request.Password));

        }
    }
    public class LoginEmailBaseCommandHandler : IRequestHandler<LoginEmailBaseCommand, Result<AuthResponse>>
    {
        private readonly IAuthService _authService;

        public LoginEmailBaseCommandHandler(IAuthService authService)
        {
            _authService = authService;
        }

        public async Task<Result<AuthResponse>> Handle(LoginEmailBaseCommand request, CancellationToken cancellationToken)
        {
            return await _authService.LoginWithEmailAsync(new LoginRequest(request.Email, request.Password));

        }
    }

}
