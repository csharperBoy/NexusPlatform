using Authentication.Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Authentication.Application.EventHandlers
{
    public class WelcomeEmailEventHandler : INotificationHandler<UserRegisteredEvent>
    {
        private readonly ILogger<WelcomeEmailEventHandler> _logger;

        public WelcomeEmailEventHandler(ILogger<WelcomeEmailEventHandler> logger)
        {
            _logger = logger;
        }

        public Task Handle(UserRegisteredEvent notification, CancellationToken cancellationToken)
        {
            // اینجا می‌توانید سرویس واقعی ارسال ایمیل را فراخوانی کنید
            _logger.LogInformation(
                "📧 [EVENT] Sending welcome email to {Email} for user {Username}",
                notification.Email, notification.Username);

            // شبیه‌سازی ارسال ایمیل
            // await _emailService.SendWelcomeEmailAsync(notification.Email, notification.DisplayName);

            return Task.CompletedTask;
        }
    }
}
