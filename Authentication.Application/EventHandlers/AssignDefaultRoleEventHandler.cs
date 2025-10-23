using Authentication.Application.Interfaces;
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
    public class AssignDefaultRoleEventHandler : INotificationHandler<UserRegisteredEvent>
    {
        private readonly IUserRoleService _userRoleService;
        private readonly ILogger<AssignDefaultRoleEventHandler> _logger;

        public AssignDefaultRoleEventHandler(
            IUserRoleService userRoleService,
            ILogger<AssignDefaultRoleEventHandler> logger)
        {
            _userRoleService = userRoleService;
            _logger = logger;
        }

        public async Task Handle(UserRegisteredEvent notification, CancellationToken cancellationToken)
        {
            var result = await _userRoleService.AssignDefaultRoleAsync(notification.UserId);

            if (result.Succeeded)
            {
                _logger.LogInformation(
                    "✅ [EVENT] Successfully assigned default role to {Username} ({Email})",
                    notification.Username, notification.Email);
            }
            else
            {
                _logger.LogError(
                    "❌ [EVENT] Failed to assign default role to {Username}: {Error}",
                    notification.Username, result.Error);

                // می‌توانید Event دیگری برای خطا ایجاد کنید
                // یا به سیستم monitoring گزارش دهید
            }
        }
    }
}
