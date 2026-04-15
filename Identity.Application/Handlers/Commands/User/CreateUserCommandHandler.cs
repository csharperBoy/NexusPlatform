using Core.Shared.Results;
using Identity.Application.Commands.User;
using Identity.Application.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Identity.Application.Handlers.Commands.User
{
    public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, Result<Guid>>
    {
        private readonly IUserInternalService _userService;
        private readonly ILogger<CreateUserCommandHandler> _logger;

        public CreateUserCommandHandler(
            IUserInternalService userService,
            ILogger<CreateUserCommandHandler> logger)
        {
            _userService = userService;
            _logger = logger;
        }

        public async Task<Result<Guid>> Handle(CreateUserCommand request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation(
                    "Creating user: ({UserName})",
                  request.UserName);

                Guid userId = await _userService.CreateUserAsync(request);

                _logger.LogInformation(
                    "User created successfully: {UserId} ({UserName})",
                    userId, request.UserName);

                return Result<Guid>.Ok(userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to create user:  ({UserName})",
                   request.UserName);

                return Result<Guid>.Fail(ex.Message);
            }
        }
    }

}
