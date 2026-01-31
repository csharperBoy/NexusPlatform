using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TraderServer.Presentation.Controllers;

namespace TraderServer.Presentation.DependencyInjection
{
  
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection TraderServer_AddPresentation(this IServiceCollection services, IConfiguration configuration)
        {
            // 📌 رجیستر کنترلرهای API
            services.AddControllers()
                .AddApplicationPart(typeof(CollectorController).Assembly) // اسمبلی کنترلرهای Sample
                .AddControllersAsServices(); // کنترلرها به عنوان سرویس در DI
            services.AddControllers()
               .AddApplicationPart(typeof(StockController).Assembly) // اسمبلی کنترلرهای Sample
               .AddControllersAsServices(); // کنترلرها به عنوان سرویس در DI


            // 📌 رجیستر MediatR برای مدیریت Command/Queryها
            services.AddMediatR(cfg =>
                cfg.RegisterServicesFromAssembly(typeof(CollectorController).Assembly));
            services.AddMediatR(cfg =>
                cfg.RegisterServicesFromAssembly(typeof(StockController).Assembly));

            return services;
        }
        
    }
}
