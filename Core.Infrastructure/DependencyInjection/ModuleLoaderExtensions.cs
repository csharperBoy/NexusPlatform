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
            var enabledModules = configuration.GetSection("Modules:Enabled").Get<string[]>() ?? [];

            foreach (var module in enabledModules)
            {
                var methods = new[]
                {
                $"{module}_AddApplication",
                $"{module}_AddInfrastructure",
                $"{module}_AddPresentation",
                $"{module}_AddHealthChecks"
            };

                foreach (var methodName in methods)
                {
                    var method = FindExtensionMethod(methodName, typeof(IServiceCollection));
                    if (method != null)
                    {
                        method.Invoke(null, new object[] { services, configuration });
                    }
                }
            }

            return services;
        }

        public static async Task<IApplicationBuilder> UseEnableModulesApplicationBuilderExtensions(
            this IApplicationBuilder app,
            IConfiguration configuration)
        {
            var enabledModules = configuration.GetSection("Modules:Enabled").Get<string[]>() ?? [];

            foreach (var module in enabledModules)
            {
                var methodName = $"{module}_UseInfrastructure";
                var method = FindExtensionMethod(methodName, typeof(IApplicationBuilder));

                if (method != null)
                {
                    var result = method.Invoke(null, new object[] { app });
                    if (result is Task task)
                    {
                        await task;
                    }
                }
            }

            return app;
        }

        private static MethodInfo? FindExtensionMethod(string methodName, Type firstParamType)
        {
            return AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => a.GetTypes())
                .Where(t => t.IsSealed && t.IsAbstract) // static class
                .SelectMany(t => t.GetMethods(BindingFlags.Public | BindingFlags.Static))
                .FirstOrDefault(m =>
                    m.Name == methodName &&
                    m.GetParameters().Length >= 1 &&
                    m.GetParameters()[0].ParameterType == firstParamType);
        }
    }

}
