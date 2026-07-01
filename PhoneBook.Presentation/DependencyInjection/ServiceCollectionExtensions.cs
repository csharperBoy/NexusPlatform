using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace PhoneBook.Presentation.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection PhoneBook_AddPresentation(this IServiceCollection services, IConfiguration configuration)
        {
            // 📌 رجیستر کنترلرهای API
        /*    services.AddControllers()
                .AddApplicationPart(typeof(PhoneBookController).Assembly) // اسمبلی کنترلرهای PhoneBook
                .AddControllersAsServices(); // کنترلرها به عنوان سرویس در DI

            // 📌 رجیستر MediatR برای مدیریت Command/Queryها
            services.AddMediatR(cfg =>
                cfg.RegisterServicesFromAssembly(typeof(PhoneBookController).Assembly));
        */
            return services;
        }
    }
}
