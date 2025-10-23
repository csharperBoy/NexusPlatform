using Auth.Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Auth.Application.EventHandlers
{
    public class TestUserRegisteredEventHandler : INotificationHandler<UserRegisteredEvent>
    {
        private readonly ILogger<TestUserRegisteredEventHandler> _logger;

        public TestUserRegisteredEventHandler(ILogger<TestUserRegisteredEventHandler> logger)
        {
            _logger = logger;
        }

        public Task Handle(UserRegisteredEvent notification, CancellationToken cancellationToken)
        {
            _logger.LogInformation(
                "✅ [TEST EVENT] User {Username} registered with email {Email} at {RegistrationTime}",
                notification.Username, notification.Email, notification.RegistrationTime);

            return Task.CompletedTask;
        }
    }
}
