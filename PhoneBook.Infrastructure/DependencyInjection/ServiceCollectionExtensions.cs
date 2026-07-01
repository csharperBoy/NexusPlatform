using Core.Application.Abstractions;
using Core.Application.Abstractions.Events;
using Core.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace PhoneBook.Infrastructure.DependencyInjection
{
  
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection PhoneBook_AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            // 📌 گرفتن Connection String از تنظیمات
            var conn = configuration.GetConnectionString("DefaultConnection");
          /*  var migrationsAssembly = typeof(PhoneBookDbContext).Assembly.GetName().Name;

            // 📌 رجیستر DbContext برای ماژول PhoneBook
            services.AddDbContext<PhoneBookDbContext>((serviceProvider, options) =>
            {
                options.UseSqlServer(conn, b =>
                {
                    // تعیین Assembly محل Migrationها
                    b.MigrationsAssembly(migrationsAssembly);

                    // تعیین جدول تاریخچه Migrationها در اسکیمای "sample"
                    b.MigrationsHistoryTable("__PhoneBookMigrationsHistory", "sample");
                });
            });

            services.AddScoped<IUnitOfWork<PhoneBookDbContext>, EfUnitOfWork<PhoneBookDbContext>>();
            // 📌 رجیستر Repository مبتنی بر Specification
            services.AddScoped<ISpecificationRepository<PhoneBookEntity, Guid>, EfSpecificationRepository<PhoneBookDbContext, PhoneBookEntity, Guid>>();

            // 📌 رجیستر HostedService برای مقداردهی اولیه ماژول
            services.AddHostedService<ModuleInitializer>();

            // 📌 رجیستر OutboxProcessor برای پردازش رویدادهای دامنه
            var registration = services.BuildServiceProvider()
                                       .GetRequiredService<IOutboxProcessorRegistration>();
            registration.AddOutboxProcessor<PhoneBookDbContext>(services);
*/
            return services;
        }
    }
}

