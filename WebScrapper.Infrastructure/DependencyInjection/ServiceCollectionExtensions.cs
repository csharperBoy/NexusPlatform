using Core.Application.Abstractions;
using Core.Application.Abstractions.Events;
using Core.Infrastructure.DependencyInjection;
using Core.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Playwright;
using WebScrapper.Application.Interfaces;
using WebScrapper.Infrastructure.Services;

namespace WebScrapper.Infrastructure.DependencyInjection
{

    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection WebScrapper_AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            // 📌 گرفتن Connection String از تنظیمات
            var conn = configuration.GetConnectionString("DefaultConnection");
            //var migrationsAssembly = typeof(SampleDbContext).Assembly.GetName().Name;

            // 📌 رجیستر DbContext برای ماژول Sample
            //services.AddDbContext<SampleDbContext>((serviceProvider, options) =>
            //{
            //    options.UseSqlServer(conn, b =>
            //    {
            //        // تعیین Assembly محل Migrationها
            //        b.MigrationsAssembly(migrationsAssembly);

            //        // تعیین جدول تاریخچه Migrationها در اسکیمای "sample"
            //        b.MigrationsHistoryTable("__SampleMigrationsHistory", "sample");
            //    });
            //});

            //services.AddScoped<IUnitOfWork<SampleDbContext>, EfUnitOfWork<SampleDbContext>>();
            //// 📌 رجیستر Repository مبتنی بر Specification
            //services.AddScoped<ISpecificationRepository<SampleEntity, Guid>, EfSpecificationRepository<SampleDbContext, SampleEntity, Guid>>();
            services.AddScoped<IWebScrapperServicee<IElementHandle>, PlaywrightScrapperService>();

            // 📌 رجیستر HostedService برای مقداردهی اولیه ماژول
            services.AddHostedService<ModuleInitializer>();

            // 📌 رجیستر OutboxProcessor برای پردازش رویدادهای دامنه
            //var registration = services.BuildServiceProvider()
            //.GetRequiredService<IOutboxProcessorRegistration>();
            //registration.AddOutboxProcessor<SampleDbContext>(services);


            // ======== 2. پیکربندی Playwright ========
            services.AddSingleton<IPlaywright>(serviceProvider =>
            {
                try
                {
                    return Playwright.CreateAsync().GetAwaiter().GetResult();
                }
                catch (Exception ex)
                {
                    throw;
                }
            });

            services.AddScoped<IBrowser>(serviceProvider =>
            {
                var playwright = serviceProvider.GetRequiredService<IPlaywright>();
                try
                {
                    return playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
                    {
                        Headless = bool.Parse(configuration["Playwright:Headless"] ?? "true"),
                        Timeout = 30000,
                        Args = new[] { "--disable-blink-features=AutomationControlled" }
                    }).GetAwaiter().GetResult();
                }
                catch (Exception ex)
                {
                    throw;
                }
            });
            services.AddScopedWithAsync<IBrowserContext>(async provider =>
            {
                try
                {
                    var browser = provider.GetRequiredService<IBrowser>();
                    var context = await browser.NewContextAsync(new BrowserNewContextOptions
                    {
                        ViewportSize = ViewportSize.NoViewport 
                    });
                    return context;
                }
                catch (Exception ex)
                {
                    throw;
                }
            });
            services.AddScopedWithAsync<IPage>(async provider =>
            {
                var browser = provider.GetRequiredService<IBrowser>();
                try
                {
                    var page = await browser.NewPageAsync();
                    await page.AddInitScriptAsync("delete navigator.webdriver;");
                    return page;
                }
                catch (Exception ex)
                {
                    throw;
                }
            });
            return services;
        }
    }
}

