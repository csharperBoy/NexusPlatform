using Core.Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;
using Notification.Application.Interfaces;
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

        public WelcomeEmailEventHandler(IEmailSender emailSender, ILogger<WelcomeEmailEventHandler> logger)
        {
            _emailSender = emailSender;
            _logger = logger;
        }

        public async Task Handle(UserRegisteredEvent notification, CancellationToken cancellationToken)
        {
            await _emailSender.SendEmailAsync(notification.Email,
                "Welcome!",
                $"سلام {notification.Username}، خوش آمدی!");

            _logger.LogInformation("Welcome email sent to {Email}", notification.Email);
        }
    }
}
