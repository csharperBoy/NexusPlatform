using Core.Presentation.Filters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
namespace Core.Presentation.DependencyInjection
{
    /*
     📌 ServiceCollectionExtensions (Presentation Layer)
     ---------------------------------------------------
     این کلاس یک **Extension Method** برای IServiceCollection فراهم می‌کند
     تا سرویس‌های مربوط به لایه‌ی Presentation (مثل API Controllers, Filters, Formatters)
     در DI Container ثبت شوند.

     ✅ نکات کلیدی:
     - Core_AddPresentation:
       • نقطه‌ی ورودی برای ثبت سرویس‌های Presentation.
       • فعلاً خالی است، اما در آینده می‌تواند شامل موارد زیر باشد:
         1. ثبت Controllerها و API Endpoints.
         2. ثبت Filters (مثل Exception Filters, Authorization Filters).
         3. ثبت Formatters (مثل JSON, XML).
         4. پیکربندی Swagger یا API Versioning در سطح Presentation.
         5. Middlewareهای خاص Presentation (مثل RequestLoggingMiddleware).

     - طراحی:
       • این کلاس مشابه ServiceCollectionExtensions در لایه‌های دیگر (Infrastructure, Application, Domain) است.
       • هدف: ایجاد یک نقطه‌ی استاندارد برای Bootstrapping سرویس‌های Presentation.
       • این کار باعث می‌شود معماری ماژولار و قابل توسعه باقی بماند.

     🛠 جریان کار:
     1. در زمان راه‌اندازی اپلیکیشن، این متد فراخوانی می‌شود:
        services.Core_AddPresentation(configuration);
     2. همه‌ی سرویس‌های مربوط به لایه‌ی Presentation در DI ثبت می‌شوند.
     3. سایر لایه‌ها (Application, Infrastructure, Domain) می‌توانند از این سرویس‌ها استفاده کنند.

     📌 نتیجه:
     این کلاس پایه‌ی مکانیزم **Presentation Bootstrapping** در معماری ماژولار است
     و تضمین می‌کند که سرویس‌های مربوط به UI/API به صورت استاندارد و یکپارچه مدیریت شوند.
    */

    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection Core_AddPresentation(this IServiceCollection services, IConfiguration configuration)
        {
           

            services.AddScoped<AuthorizeResourceFilter>();
            // 📌 در آینده می‌توان سرویس‌های Presentation را اینجا ثبت کرد
            return services;
        }
    }
}
