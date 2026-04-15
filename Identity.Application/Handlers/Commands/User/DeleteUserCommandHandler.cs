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

    public class DeleteUserCommandHandler : IRequestHandler<DeleteUserCommand, Result<bool>>
    {
        private readonly IUserInternalService _userService;
        private readonly ILogger<DeleteUserCommandHandler> _logger;

        public DeleteUserCommandHandler(
            IUserInternalService userService,
            ILogger<DeleteUserCommandHandler> logger)
        {
            _userService = userService;
            _logger = logger;
        }

        public async Task<Result<bool>> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Deleting user: {UserId}", request.Id);

                await _userService.DeleteUserAsync(request.Id);

                _logger.LogInformation("User deleted successfully: {UserId}", request.Id);

                return Result<bool>.Ok(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete user: {UserId}", request.Id);
                return Result<bool>.Fail(ex.Message);
            }
        }
    }

}
