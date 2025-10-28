using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Notification.Application.Interfaces;
using Notification.Presentation.Hubs;
using Notification.Presentation.RealTime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Notification.Presentation.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection Notification_AddPresentation(this IServiceCollection services)
        {
            services.AddSignalR();

            // SignalR notifier
            services.AddScoped<IRealtimeNotifier, SignalRRealtimeNotifier>();
            return services;
        }

        public static IApplicationBuilder UseNotificationPresentation(this IApplicationBuilder app)
        {
           return app;
        }
    }
}
