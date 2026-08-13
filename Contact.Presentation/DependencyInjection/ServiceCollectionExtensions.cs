using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Contact.Presentation.Controllers;
using Contact.Presentation.Controller;

namespace Contact.Presentation.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection Contact_AddPresentation(this IServiceCollection services, IConfiguration configuration)
        {
            // 📌 رجیستر کنترلرهای API
           services.AddControllers()
                .AddApplicationPart(typeof(PhoneBookController).Assembly) // اسمبلی کنترلرهای PhoneBook
                .AddControllersAsServices(); // کنترلرها به عنوان سرویس در DI
           services.AddControllers()
                .AddApplicationPart(typeof(PostContactController).Assembly) // اسمبلی کنترلرهای PhoneBook
                .AddControllersAsServices(); // کنترلرها به عنوان سرویس در DI
           services.AddControllers()
                .AddApplicationPart(typeof(EmploymentContactController).Assembly) // اسمبلی کنترلرهای PhoneBook
                .AddControllersAsServices(); // کنترلرها به عنوان سرویس در DI
           services.AddControllers()
                .AddApplicationPart(typeof(LocationContactController).Assembly) // اسمبلی کنترلرهای PhoneBook
                .AddControllersAsServices(); // کنترلرها به عنوان سرویس در DI

            // 📌 رجیستر MediatR برای مدیریت Command/Queryها
            services.AddMediatR(cfg =>
                cfg.RegisterServicesFromAssembly(typeof(PhoneBookController).Assembly));
            services.AddMediatR(cfg =>
                cfg.RegisterServicesFromAssembly(typeof(PostContactController).Assembly));
            services.AddMediatR(cfg =>
                cfg.RegisterServicesFromAssembly(typeof(EmploymentContactController).Assembly));
            services.AddMediatR(cfg =>
                cfg.RegisterServicesFromAssembly(typeof(LocationContactController).Assembly));
        
            return services;
        }
    }
}
