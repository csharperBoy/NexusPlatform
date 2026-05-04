using Core.Application.Helper;
using Core.Application.Models;
using Core.Application.Provider;
using Core.Domain.Enums;
using Identity.Application.EventHandlers;
using Identity.Application.Provider;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Identity.Application.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection Identity_AddApplication(this IServiceCollection services, IConfiguration configuration)
        {
            // رجیستر MediatR و هندلرها
            services.AddMediatR(cfg =>
            {
                cfg.RegisterServicesFromAssembly(typeof(ServiceCollectionExtensions).Assembly);
            });
            var enabledModules = configuration
                      .GetSection("Modules:Enabled")
                      .Get<List<ModuleItem>>();   // List<ModuleInfo>
                                                  // آیا ماژول Authorization در لیست فعال است؟
            bool isAuthEnabled = enabledModules?
                .Any(m => string.Equals(m.Name, "Authorization", StringComparison.OrdinalIgnoreCase))
                ?? false;   // در صورت null به false برمی‌گردد

            // اگر فعال نیست، سرویس را اضافه می‌کنیم
            if (!isAuthEnabled)
            {
                services.AddScoped<IUserDataContextProvider, UserDataContextProvider>();
            }

            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(AssignDefaultRoleEventHandler).Assembly));

            return services;
        }
    }
}