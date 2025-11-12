using Core.Application.Common.Events;
using Identity.Application.Interfaces;
using Identity.Shared.Events;
using MediatR;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Registry;

namespace Identity.Application.EventHandlers
{
    //public class AssignDefaultRoleEventHandler : INotificationHandler<UserRegisteredEvent>
    public class AssignDefaultRoleEventHandler : DomainEventHandler<UserRegisteredEvent>
    {
        private readonly IAuthorizationService _authorizationService;
        private readonly ILogger<AssignDefaultRoleEventHandler> _logger;

        private readonly IReadOnlyPolicyRegistry<string> _policies;
        public AssignDefaultRoleEventHandler(
            IAuthorizationService authorizationService,
            ILogger<AssignDefaultRoleEventHandler> logger,
            IReadOnlyPolicyRegistry<string> policies)
            : base(logger)
        {
            _authorizationService = authorizationService;
            _logger = logger;
        }


        protected override async Task HandleEventAsync(UserRegisteredEvent notification, CancellationToken cancellationToken)
        {
             var policy = _policies.Get<IAsyncPolicy>("DefaultRetry");
            await policy.ExecuteAsync(async ct =>
            {
                var result = await _authorizationService.AssignDefaultRoleAsync(notification.UserId);

                if (result.Succeeded)
                    _logger.LogInformation("Default role 'User' assigned to {UserId}", notification.UserId);
                else
                    _logger.LogError("Failed to assign default role to {UserId}: {Error}", notification.UserId, result.Error);

            }, cancellationToken);
        }
    }
}

