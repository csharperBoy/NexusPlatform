using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Authorization.Domain.DependencyInjection
{
  

    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection Sample_AddDomain(this IServiceCollection services, IConfiguration configuration)
        {
            // فعلاً هیچ سرویس خاصی رجیستر نمی‌شود.
            // در آینده می‌توانیم سرویس‌های مرتبط با Domain را اینجا اضافه کنیم.
            return services;
        }
    }
}

