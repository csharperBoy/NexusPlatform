using Identity.Shared.Events;
using MediatR;
using Microsoft.Extensions.Logging;
using Notification.Application.Interfaces;
using Polly;
using Polly.Registry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Notification.Application.EventHandlers
{
    public class WelcomeEmailEventHandler : INotificationHandler<UserRegisteredEvent>
    {
        private readonly IEmailSender _emailSender;
        private readonly ILogger<WelcomeEmailEventHandler> _logger;

        private readonly IReadOnlyPolicyRegistry<string> _policies;
        public WelcomeEmailEventHandler(IEmailSender emailSender,
            ILogger<WelcomeEmailEventHandler> logger
            , IReadOnlyPolicyRegistry<string> policies)
        {
            _emailSender = emailSender;
            _logger = logger;
            _policies = policies;
        }

        public async Task Handle(UserRegisteredEvent notification, CancellationToken cancellationToken)
        {
            var policy = _policies.Get<IAsyncPolicy>("DefaultRetry");
            await policy.ExecuteAsync(async ct =>
            {
                await _emailSender.SendEmailAsync(notification.Email,
                "Welcome!",
                $"سلام {notification.Username}، خوش آمدی!");

                _logger.LogInformation("Welcome email sent to {Email}", notification.Email);
            }, cancellationToken);
        }
    }
}
