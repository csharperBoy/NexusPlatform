using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace StreamBus.Presentation.DependencyInjection
{
  
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection StreamBus_AddPresentation(this IServiceCollection services, IConfiguration configuration)
        {
            //// 📌 رجیستر کنترلرهای API
            //services.AddControllers()
            //    .AddApplicationPart(typeof(StreamBusController).Assembly) // اسمبلی کنترلرهای StreamBus
            //    .AddControllersAsServices(); // کنترلرها به عنوان سرویس در DI

            //// 📌 رجیستر MediatR برای مدیریت Command/Queryها
            //services.AddMediatR(cfg =>
            //    cfg.RegisterServicesFromAssembly(typeof(StreamBusController).Assembly));

            return services;
        }
    }
}
