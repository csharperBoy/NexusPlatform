using Identity.Application.Interfaces;
using Identity.Shared.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Identity.Application.EventHandlers
{
    public class AssignDefaultRoleEventHandler : INotificationHandler<UserRegisteredEvent>
    {
        private readonly IAuthorizationService _authorizationService;
        private readonly ILogger<AssignDefaultRoleEventHandler> _logger;

        public AssignDefaultRoleEventHandler(
            IAuthorizationService authorizationService,
            ILogger<AssignDefaultRoleEventHandler> logger)
        {
            _authorizationService = authorizationService;
            _logger = logger;
        }

        public async Task Handle(UserRegisteredEvent notification, CancellationToken cancellationToken)
        {
            var result = await _authorizationService.AssignDefaultRoleAsync(notification.UserId);

            if (result.Succeeded)
                _logger.LogInformation("Default role 'User' assigned to {UserId}", notification.UserId);
            else
                _logger.LogError("Failed to assign default role to {UserId}: {Error}", notification.UserId, result.Error);
        }
    }
}

