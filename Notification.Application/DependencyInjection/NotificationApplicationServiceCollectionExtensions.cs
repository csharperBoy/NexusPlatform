using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Notification.Application.EventHandlers;

namespace Notification.Application.DependencyInjection
{
    public static class NotificationApplicationServiceCollectionExtensions
    {
        public static IServiceCollection AddNotificationApplication(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(WelcomeEmailEventHandler).Assembly));
            
            return services;
        }
    }
}