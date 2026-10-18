using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Trader.Application.Abstractions.Strategies;

namespace Trader.Infrastructure.DependencyInjection;

public static class StrategyRegistrationExtensions
{
    /// <summary>
    /// همه‌ی IStrategyRegistrationهای موجود در Trader.Application را Scan و Register می‌کند.
    /// برای افزودن Strategy جدید، فقط کافی است کلاس Registration بسازی.
    /// </summary>
    public static IServiceCollection AddTraderStrategies(this IServiceCollection services)
    {
        var assembly = Assembly.Load("Trader.Application");

        var registrations = assembly.GetTypes()
            .Where(t => t is { IsClass: true, IsAbstract: false })
            .Where(t => typeof(IStrategyRegistration).IsAssignableFrom(t))
            .ToList();

        foreach (var type in registrations)
        {
            services.AddSingleton(typeof(IStrategyRegistration), type);
        }

        return services;
    }
}