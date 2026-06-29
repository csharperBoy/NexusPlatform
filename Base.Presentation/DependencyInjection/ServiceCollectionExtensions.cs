using Base.Presentation.Controllers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Base.Presentation.DependencyInjection
{

    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection Base_AddPresentation(this IServiceCollection services, IConfiguration configuration)
        {
           
            services.AddControllers()
               .AddApplicationPart(typeof(SettingController).Assembly) // اسمبلی کنترلرهای Base
               .AddControllersAsServices(); // کنترلرها به عنوان سرویس در DI

            // 📌 رجیستر MediatR برای مدیریت Command/Queryها
            services.AddMediatR(cfg =>
                cfg.RegisterServicesFromAssembly(typeof(SettingController).Assembly));

            return services;
        }
    }
}
