using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Notification.Application.EventHandlers;

namespace Notification.Application.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection Notification_AddApplication(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(WelcomeEmailEventHandler).Assembly));
            // قراردادها فقط رجیستر نمی‌شن؛ پیاده‌سازی‌ها در Infrastructure رجیستر می‌شن.
            return services;
        }
    }
}