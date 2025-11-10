using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
namespace Core.Domain.DependencyInjection
{
    /*
     📌 ServiceCollectionExtensions
     ------------------------------
     این کلاس یک **Extension Class** برای IServiceCollection است که وظیفه‌اش
     ثبت سرویس‌های مربوط به لایه Domain در DI Container می‌باشد.

     ✅ نکات کلیدی:
     - Core_AddDomain(IServiceCollection services, IConfiguration configuration)
       • متدی برای افزودن سرویس‌های دامنه به DI.
       • در حال حاضر فقط services را برمی‌گرداند (Placeholder).
       • در آینده می‌تواند شامل ثبت موارد زیر باشد:
         1. Domain Services (مثل سرویس‌های مربوط به قوانین دامنه).
         2. Event Dispatchers (برای انتشار Domain Events).
         3. Outbox Processors (برای مدیریت Outbox Pattern).
         4. Value Object Converters (برای EF Core یا Serialization).

     🛠 جریان کار:
     1. در زمان راه‌اندازی برنامه (Startup/Program.cs)، این متد فراخوانی می‌شود:
        services.Core_AddDomain(Configuration);
     2. همه‌ی سرویس‌های مربوط به Domain در DI ثبت می‌شوند.
     3. سایر لایه‌ها (Application, Infrastructure, API) می‌توانند این سرویس‌ها را استفاده کنند.

     📌 نتیجه:
     این کلاس پایه‌ی مکانیزم **Dependency Injection for Domain Layer** است
     و تضمین می‌کند که سرویس‌های دامنه به صورت ماژولار و قابل مدیریت در DI Container ثبت شوند.
    */

    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection Core_AddDomain(this IServiceCollection services, IConfiguration configuration)
        {
            // 📌 در حال حاضر فقط services را برمی‌گرداند
            // در آینده می‌توان سرویس‌های دامنه را اینجا ثبت کرد
            return services;
        }
    }
}
