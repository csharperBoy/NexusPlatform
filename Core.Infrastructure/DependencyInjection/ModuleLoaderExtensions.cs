using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
namespace Core.Infrastructure.DependencyInjection
{
    /*
     📌 ModuleLoaderExtensions
     -------------------------
     این کلاس مجموعه‌ای از **Extension Methods** برای IServiceCollection و IApplicationBuilder است
     که وظیفه‌ی بارگذاری و فعال‌سازی ماژول‌ها به صورت داینامیک (Dynamic Module Loading) را بر عهده دارد.

     ✅ نکات کلیدی:
     1. AddEnableModulesServiceCollectionExtensions:
        • ماژول‌های فعال را از بخش "Modules:Enabled" در IConfiguration می‌خواند.
        • برای هر ماژول، متدهای زیر را به ترتیب اجرا می‌کند (اگر وجود داشته باشند):
          - {ModuleName}_AddInfrastructure
          - {ModuleName}_AddApplication
          - {ModuleName}_AddDomain
          - {ModuleName}_AddPresentation
          - {ModuleName}_AddHealthChecks
        • این متدها باید به صورت Extension Method در اسمبلی‌های مربوطه تعریف شده باشند.
        • هدف: ثبت سرویس‌های هر ماژول در DI Container.

     2. UseEnableModulesApplicationBuilderExtensions:
        • ماژول‌های فعال را از IConfiguration می‌خواند.
        • برای هر ماژول، متد {ModuleName}_UseInfrastructure را اجرا می‌کند (اگر وجود داشته باشد).
        • هدف: ثبت Middlewareها و تنظیمات زیرساختی هر ماژول در Pipeline اپلیکیشن.

     3. FindExtensionMethod:
        • وظیفه‌ی پیدا کردن متد Extension در اسمبلی‌های کاندید (Infrastructure, Domain, Application, Presentation).
        • فقط کلاس‌های static (sealed + abstract) بررسی می‌شوند.
        • اگر متد پیدا شود، با Reflection اجرا می‌شود.
        • اگر پیدا نشود، پیام هشدار در Console چاپ می‌شود.

     4. ModuleConfig:
        • مدل پیکربندی ماژول‌ها.
        • Name → نام ماژول (مثلاً "Core", "Identity", "Orders").
        • Order → ترتیب بارگذاری ماژول‌ها.

     🛠 جریان کار:
     1. در فایل appsettings.json بخش Modules:Enabled تعریف می‌شود:
        "Modules": {
          "Enabled": [
            { "Name": "Core", "Order": 1 },
            { "Name": "Identity", "Order": 2 }
          ]
        }

     2. در زمان راه‌اندازی اپلیکیشن:
        services.AddEnableModulesServiceCollectionExtensions(Configuration);
        app.UseEnableModulesApplicationBuilderExtensions(Configuration);

     3. هر ماژول به صورت داینامیک بارگذاری می‌شود و سرویس‌ها و Middlewareهای مربوطه ثبت می‌شوند.

     📌 نتیجه:
     این کلاس پایه‌ی مکانیزم **Dynamic Module Loading** در معماری ماژولار است
     و تضمین می‌کند که ماژول‌ها به صورت مستقل، قابل مدیریت و قابل توسعه بارگذاری شوند.
    */

    public static class ModuleLoaderExtensions
    {
        public static IServiceCollection AddEnableModulesServiceCollectionExtensions(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            var enabledModules = configuration
                .GetSection("Modules:Enabled")
                .Get<List<ModuleConfig>>() ?? new();

            foreach (var module in enabledModules.OrderBy(m => m.Order))
            {
                var methods = new[]
                {
                    $"{module.Name}_AddInfrastructure",
                    $"{module.Name}_AddApplication",
                    $"{module.Name}_AddDomain",
                    $"{module.Name}_AddPresentation",
                    $"{module.Name}_AddHealthChecks"
                };

                foreach (var methodName in methods)
                {
                    var method = FindExtensionMethod(module.Name, methodName, typeof(IServiceCollection));
                    if (method != null)
                    {
                        Console.WriteLine($"✅ اجرای متد {methodName} از ماژول {module.Name}");
                        method.Invoke(null, new object[] { services, configuration });
                    }
                    else
                    {
                        Console.WriteLine($"⚠️ متد {methodName} در ماژول {module.Name} پیدا نشد.");
                    }
                }
            }

            return services;
        }

        public static async Task<IApplicationBuilder> UseEnableModulesApplicationBuilderExtensions(
            this IApplicationBuilder app,
            IConfiguration configuration)
        {
            var enabledModules = configuration
                .GetSection("Modules:Enabled")
                .Get<List<ModuleConfig>>() ?? new();

            foreach (var module in enabledModules.OrderBy(m => m.Order))
            {
                var methodName = $"{module.Name}_UseInfrastructure";
                var method = FindExtensionMethod(module.Name, methodName, typeof(IApplicationBuilder));

                if (method != null)
                {
                    Console.WriteLine($"✅ اجرای متد {methodName} از ماژول {module.Name}");
                    var result = method.Invoke(null, new object[] { app });
                    if (result is Task task)
                    {
                        await task;
                    }
                }
                else
                {
                    Console.WriteLine($"⚠️ متد {methodName} در ماژول {module.Name} پیدا نشد.");
                }
            }

            return app;
        }

        private static MethodInfo? FindExtensionMethod(string moduleName, string methodName, Type firstParamType)
        {
            var candidateAssemblies = new[]
            {
                $"{moduleName}.Infrastructure",
                $"{moduleName}.Domain",
                $"{moduleName}.Application",
                $"{moduleName}.Presentation"
            };

            foreach (var asmName in candidateAssemblies)
            {
                try
                {
                    var asm = Assembly.Load(asmName);
                    Console.WriteLine($"➡️ جستجو در اسمبلی: {asm.FullName}");

                    foreach (var type in asm.GetTypes().Where(t => t.IsSealed && t.IsAbstract))
                    {
                        foreach (var m in type.GetMethods(BindingFlags.Public | BindingFlags.Static))
                        {
                            if (m.Name == methodName &&
                                m.GetParameters().Length >= 1 &&
                                m.GetParameters()[0].ParameterType == firstParamType)
                            {
                                Console.WriteLine($"✅ متد {methodName} پیدا شد در {asm.GetName().Name} :: {type.FullName}");
                                return m;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ نتونستم اسمبلی {asmName} رو لود کنم: {ex.Message}");
                }
            }

            return null;
        }
    }

    public class ModuleConfig
    {
        public string Name { get; set; } = "";
        public int Order { get; set; }
    }
}
