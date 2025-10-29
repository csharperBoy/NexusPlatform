using Core.Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;
using Notification.Application.Interfaces;
using Notification.Application.Models;
using Polly;
using Polly.Registry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace Notification.Application.EventHandlers
{
    public class WelcomeInAppNotificationHandler : INotificationHandler<UserRegisteredEvent>
    {
        private readonly IRealtimeNotifier _notifier;
        private readonly ILogger<WelcomeInAppNotificationHandler> _logger;

        private readonly IReadOnlyPolicyRegistry<string> _policies;
        public WelcomeInAppNotificationHandler(IRealtimeNotifier notifier
            , ILogger<WelcomeInAppNotificationHandler> logger
            , IReadOnlyPolicyRegistry<string> policies)
        {
            _notifier = notifier;
            _logger = logger;
            _policies = policies;
        }

        public async Task Handle(UserRegisteredEvent notification, CancellationToken cancellationToken)
        {
            var policy = _policies.Get<IAsyncPolicy>("DefaultRetry");
            await policy.ExecuteAsync(async ct =>
            {
                var message = new NotificationMessage
                {
                    UserId = notification.UserId.ToString(),
                    Title = "Welcome!",
                    Body = $"سلام {notification.Username}، خوش آمدی!",
                    Channel = NotificationChannel.InApp
                };

                await _notifier.SendToUserAsync(message.UserId, message, cancellationToken);
                _logger.LogInformation("Welcome in-app notification sent to {UserId}", notification.UserId);
            }, cancellationToken);
        }
    }
}