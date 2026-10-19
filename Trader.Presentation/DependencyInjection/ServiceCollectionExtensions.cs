using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using Trader.Presentation.Controller;

namespace Trader.Presentation.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection Trader_AddPresentation(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // ─── Controllers ───
        // همه‌ی Controllers در یک Assembly هستند، یک بار کافی است.
        services.AddControllers()
            .AddApplicationPart(typeof(AccountController).Assembly)
            .AddControllersAsServices();

        // ─── MediatR ───
        // Command/Query Handlerها در Trader.Application هستند.
        // ولی اگر در Presentation یا Infrastructure هم Handler داشته باشیم،
        // باید آن Assembly هم Register شود.
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(
                Assembly.Load("Trader.Application"));

            // اگر Handler در Presentation هم داری (که نباید داشته باشی):
            // cfg.RegisterServicesFromAssembly(typeof(AccountController).Assembly);
        });

        // ─── Pipeline Behaviors (اگر هنوز Register نشده) ───
        // اگر SetUserDataBehavior و سایر Behaviorها را در Core Register کرده‌ای،
        // اینجا لازم نیست. اگر نکرده‌ای، اینجا Register کن:
        //
        // services.AddTransient(typeof(IPipelineBehavior<,>), typeof(SetUserDataBehavior<,>));
        //
        // ⚠️ فقط یک بار در کل برنامه باید Register شود.
        // اگر در Core یا جای دیگری Register شده، این را حذف کن.

        return services;
    }
}