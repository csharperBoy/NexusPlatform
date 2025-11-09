using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Sample.Domain.DependencyInjection
{
    /*
     📌 ServiceCollectionExtensions (Domain Layer)
     ---------------------------------------------
     این کلاس برای رجیستر کردن سرویس‌های مربوط به Domain Layer در DI (Dependency Injection) استفاده می‌شود.

     ✅ نکات کلیدی:
     - Domain Layer معمولاً شامل Entities, ValueObjects, Domain Events و Specifications است.
     - این لایه منطق تجاری خالص (Pure Business Logic) را نگه می‌دارد و نباید وابستگی مستقیم به زیرساخت داشته باشد.
     - در بسیاری از پروژه‌ها، Domain نیاز به رجیستر کردن سرویس خاصی ندارد (چون بیشتر کلاس‌ها POCO هستند).
     - با این حال، داشتن یک متد Extension مثل Sample_AddDomain باعث می‌شود ساختار پروژه یکپارچه باشد
       و اگر در آینده نیاز به رجیستر کردن سرویس‌های مرتبط با Domain داشتیم، اینجا انجام شود.

     🛠 جریان کار:
     1. در Startup یا Program.cs، متد Sample_AddDomain فراخوانی می‌شود.
     2. فعلاً هیچ سرویس خاصی رجیستر نمی‌شود (فقط یک placeholder است).
     3. در آینده می‌توانیم سرویس‌های مرتبط با Domain (مثل Domain Event Dispatchers یا Specifications مشترک) را اینجا اضافه کنیم.

     📌 نتیجه:
     این کلاس یک نقطه‌ی ورودی استاندارد برای رجیستر کردن سرویس‌های Domain است،
     حتی اگر فعلاً خالی باشد. این کار باعث می‌شود معماری پروژه یکپارچه و قابل توسعه باقی بماند.
    */

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

