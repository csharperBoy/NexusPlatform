using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Notification.Application.DependencyInjection
{
    public static class NotificationApplicationServiceCollectionExtensions
    {
        public static IServiceCollection AddNotificationApplication(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(WelcomeEmailEventHandler).Assembly));
            services.AddScoped<IEmailSender, SmtpEmailSender>();

            return services;
        }
    }
}