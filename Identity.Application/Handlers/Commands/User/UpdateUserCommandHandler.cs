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
    public class UpdateUserCommandHandler : IRequestHandler<UpdateUserCommand, Result<bool>>
    {
        private readonly IUserInternalService _UserService;
        private readonly ILogger<UpdateUserCommandHandler> _logger;

        public UpdateUserCommandHandler(
            IUserInternalService UserService,
            ILogger<UpdateUserCommandHandler> logger)
        {
            _UserService = UserService;
            _logger = logger;
        }

        public async Task<Result<bool>> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Updating User: {UserId}", request.Id);

                await _UserService.UpdateUserAsync(request);

                _logger.LogInformation("User updated successfully: {UserId}", request.Id);

                return Result<bool>.Ok(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update User: {UserId}", request.Id);
                return Result<bool>.Fail(ex.Message);
            }
        }
    }

}
