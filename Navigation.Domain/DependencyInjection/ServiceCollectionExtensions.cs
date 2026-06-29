using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Navigation.Domain.DependencyInjection
{
    
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection Navigation_AddDomain(this IServiceCollection services, IConfiguration configuration)
        {
            // فعلاً هیچ سرویس خاصی رجیستر نمی‌شود.
            // در آینده می‌توانیم سرویس‌های مرتبط با Domain را اینجا اضافه کنیم.
            return services;
        }
    }
}
