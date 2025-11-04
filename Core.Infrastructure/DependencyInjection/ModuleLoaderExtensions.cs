using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Core.Infrastructure.DependencyInjection
{
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
            // الگوی نام‌گذاری اسمبلی‌ها: ModuleName.Infrastructure / ModuleName.Application / ModuleName.Presentation
            var candidateAssemblies = new[]
            {
                $"{moduleName}.Infrastructure",
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
