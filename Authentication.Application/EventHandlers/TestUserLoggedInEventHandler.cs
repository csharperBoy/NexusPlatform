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
    public class TestUserLoggedInEventHandler : INotificationHandler<UserLoggedInEvent>
    {
        private readonly ILogger<TestUserLoggedInEventHandler> _logger;

        public TestUserLoggedInEventHandler(ILogger<TestUserLoggedInEventHandler> logger)
        {
            _logger = logger;
        }

        public Task Handle(UserLoggedInEvent notification, CancellationToken cancellationToken)
        {
            _logger.LogInformation(
                "✅ [TEST EVENT] User {Username} logged in at {LoginTime} from IP {IpAddress}",
                notification.Username, notification.LoginTime, notification.IpAddress);

            // می‌توانید اینجا logic های واقعی اضافه کنید
            // مثلاً: ارسال ایمن خوش آمدگویی، آپدیت آخرین زمان لاگین، etc.

            return Task.CompletedTask;
        }
    }
}
