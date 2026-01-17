using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Trader.Server.Collector.Presentation.DependencyInjection
{
  
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection TraderServerCollector_AddPresentation(this IServiceCollection services, IConfiguration configuration)
        {
            // 📌 رجیستر کنترلرهای API
            services.AddControllers()
                .AddApplicationPart(typeof(CollectorController).Assembly) // اسمبلی کنترلرهای Sample
                .AddControllersAsServices(); // کنترلرها به عنوان سرویس در DI

            // 📌 رجیستر MediatR برای مدیریت Command/Queryها
            services.AddMediatR(cfg =>
                cfg.RegisterServicesFromAssembly(typeof(CollectorController).Assembly));

            return services;
        }
    }
}
