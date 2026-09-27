using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Scheduler.Presentation.Controllers;

namespace Scheduler.Presentation.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection Contact_AddPresentation(this IServiceCollection services, IConfiguration configuration)
        {
            // 📌 رجیستر کنترلرهای API
           services.AddControllers()
                .AddApplicationPart(typeof(SchedulerController).Assembly) // اسمبلی کنترلرهای PhoneBook
                .AddControllersAsServices(); // کنترلرها به عنوان سرویس در DI
        

            // 📌 رجیستر MediatR برای مدیریت Command/Queryها
            services.AddMediatR(cfg =>
                cfg.RegisterServicesFromAssembly(typeof(SchedulerController).Assembly));
            
            return services;
        }
    }
}
