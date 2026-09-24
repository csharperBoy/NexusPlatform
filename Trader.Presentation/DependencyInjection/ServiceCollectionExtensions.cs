using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Trader.Presentation.Controller;

namespace Trader.Presentation.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection Trader_AddPresentation(this IServiceCollection services, IConfiguration configuration)
        {
            // 📌 رجیستر کنترلرهای API
            services.AddControllers()
                .AddApplicationPart(typeof(AccountController).Assembly) // اسمبلی کنترلرهای PhoneBook
                .AddControllersAsServices(); // کنترلرها به عنوان سرویس در DI
        

            // 📌 رجیستر MediatR برای مدیریت Command/Queryها
            services.AddMediatR(cfg =>
                cfg.RegisterServicesFromAssembly(typeof(AccountController).Assembly));
            // 📌 رجیستر کنترلرهای API
            services.AddControllers()
                .AddApplicationPart(typeof(SchedulePlanController).Assembly) // اسمبلی کنترلرهای PhoneBook
                .AddControllersAsServices(); // کنترلرها به عنوان سرویس در DI
        

            // 📌 رجیستر MediatR برای مدیریت Command/Queryها
            services.AddMediatR(cfg =>
                cfg.RegisterServicesFromAssembly(typeof(SchedulePlanController).Assembly));
            // 📌 رجیستر کنترلرهای API
            services.AddControllers()
                .AddApplicationPart(typeof(ServerClockController).Assembly) // اسمبلی کنترلرهای PhoneBook
                .AddControllersAsServices(); // کنترلرها به عنوان سرویس در DI
        

            // 📌 رجیستر MediatR برای مدیریت Command/Queryها
            services.AddMediatR(cfg =>
                cfg.RegisterServicesFromAssembly(typeof(ServerClockController).Assembly));
            // 📌 رجیستر کنترلرهای API
            services.AddControllers()
                .AddApplicationPart(typeof(SymbolController).Assembly) // اسمبلی کنترلرهای PhoneBook
                .AddControllersAsServices(); // کنترلرها به عنوان سرویس در DI
        

            // 📌 رجیستر MediatR برای مدیریت Command/Queryها
            services.AddMediatR(cfg =>
                cfg.RegisterServicesFromAssembly(typeof(SymbolController).Assembly));
            
            return services;
        }
    }
}
