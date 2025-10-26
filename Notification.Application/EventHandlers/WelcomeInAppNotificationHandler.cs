using Core.Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;
using Notification.Application.Interfaces;
using Notification.Application.Models;
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

        public WelcomeInAppNotificationHandler(IRealtimeNotifier notifier, ILogger<WelcomeInAppNotificationHandler> logger)
        {
            _notifier = notifier;
            _logger = logger;
        }

        public async Task Handle(UserRegisteredEvent notification, CancellationToken cancellationToken)
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
        }
    }
}