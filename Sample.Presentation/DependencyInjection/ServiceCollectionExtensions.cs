using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Sample.Presentation.Controllers;

namespace Sample.Presentation.DependencyInjection
{
    /*
     📌 ServiceCollectionExtensions (Presentation Layer)
     ---------------------------------------------------
     این کلاس یک Extension برای IServiceCollection است که وظیفه‌اش رجیستر کردن
     سرویس‌های مربوط به لایه Presentation (کنترلرها و MediatR) در DI Container می‌باشد.

     ✅ نکات کلیدی:
     - متد اصلی: Sample_AddPresentation
       → در زمان راه‌اندازی برنامه (Startup/Program.cs) فراخوانی می‌شود.
       → سرویس‌های زیر رجیستر می‌شوند:

       1. AddControllers:
          - کنترلرهای API را به برنامه اضافه می‌کند.
          - AddApplicationPart(typeof(SampleController).Assembly) → اسمبلی کنترلرهای Sample را به سیستم اضافه می‌کند.
          - AddControllersAsServices → کنترلرها به عنوان سرویس در DI رجیستر می‌شوند (امکان تزریق وابستگی‌ها).

       2. AddMediatR:
          - MediatR را رجیستر می‌کند تا Commandها و Queryها از طریق Mediator مدیریت شوند.
          - RegisterServicesFromAssembly(typeof(SampleController).Assembly) → همه‌ی Handlerها در اسمبلی Presentation رجیستر می‌شوند.

     🛠 جریان کار:
     1. در Program.cs متد Sample_AddPresentation فراخوانی می‌شود.
     2. کنترلرهای Sample به برنامه اضافه می‌شوند.
     3. MediatR رجیستر می‌شود و آماده‌ی مدیریت Command/Queryهاست.
     4. حالا درخواست‌های API از طریق Controller → Mediator → Handler → Service → Repository پردازش می‌شوند.

     📌 نتیجه:
     این کلاس نقطه‌ی ورودی ماژول Sample به لایه Presentation است و تضمین می‌کند
     که کنترلرها و MediatR به صورت استاندارد رجیستر شوند.
    */

    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection Sample_AddPresentation(this IServiceCollection services, IConfiguration configuration)
        {
            // 📌 رجیستر کنترلرهای API
            services.AddControllers()
                .AddApplicationPart(typeof(SampleController).Assembly) // اسمبلی کنترلرهای Sample
                .AddControllersAsServices(); // کنترلرها به عنوان سرویس در DI

            // 📌 رجیستر MediatR برای مدیریت Command/Queryها
            services.AddMediatR(cfg =>
                cfg.RegisterServicesFromAssembly(typeof(SampleController).Assembly));

            return services;
        }
    }
}
