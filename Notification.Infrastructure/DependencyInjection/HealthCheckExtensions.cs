using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Notification.Infrastructure.HealthChecks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Notification.Infrastructure.DependencyInjection
{
    public static class HealthCheckExtensions
    {
        public static IServiceCollection Notification_AddHealthChecks(this IServiceCollection services, IConfiguration config)
        {
            var conn = config.GetConnectionString("DefaultConnection");

            services.AddHealthChecks()
                .AddCheck<SmtpHealthCheck>("SMTP");

            return services;
        }
    }
}
