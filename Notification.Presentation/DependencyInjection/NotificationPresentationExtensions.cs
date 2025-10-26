using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Notification.Presentation.DependencyInjection
{
    public static class NotificationPresentationExtensions
    {
        public static IServiceCollection AddNotificationPresentation(this IServiceCollection services)
        {
            services.AddSignalR();
            return services;
        }

        public static IApplicationBuilder UseNotificationPresentation(this IApplicationBuilder app)
        {
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHub<NotificationHub>("/hubs/notifications");
            });
            return app;
        }
    }
}
