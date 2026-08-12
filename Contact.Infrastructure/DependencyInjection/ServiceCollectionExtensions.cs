using Contact.Infrastructure.Data;
using Core.Application.Abstractions;
using Core.Application.Abstractions.Events;
using Core.Application.Abstractions.HR;
using Core.Application.Abstractions.PhoneBook;
using Core.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Contact.Application.Interfaces;
using Contact.Domain.Entities;
using Contact.Infrastructure.Services;

namespace Contact.Infrastructure.DependencyInjection
{
  
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection PhoneBook_AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            // 📌 گرفتن Connection String از تنظیمات
            var conn = configuration.GetConnectionString("DefaultConnection");
            var migrationsAssembly = typeof(ContactDbContext).Assembly.GetName().Name;

            // 📌 رجیستر DbContext برای ماژول PhoneBook
            services.AddDbContext<ContactDbContext>((serviceProvider, options) =>
            {
                options.UseSqlServer(conn, b =>
                {
                    // تعیین Assembly محل Migrationها
                    b.MigrationsAssembly(migrationsAssembly);

                    // تعیین جدول تاریخچه Migrationها در اسکیمای "sample"
                    b.MigrationsHistoryTable("__phonebookMigrationsHistory", "phonebook");
                });
            });
            services.AddScoped<PhoneBookService>();


            services.AddScoped<IPhoneBookPublicService>(sp => sp.GetRequiredService<PhoneBookService>());
            services.AddScoped<IPhoneBookInternalService>(sp => sp.GetRequiredService<PhoneBookService>());
            services.AddScoped<IPhoneBookInternalService, PhoneBookService>();
            
            services.AddScoped<IUnitOfWork<ContactDbContext>, EfUnitOfWork<ContactDbContext>>();
            // 📌 رجیستر Repository مبتنی بر Specification
            services.AddScoped<ISpecificationRepository<PhoneBookInfoView, Guid>, EfSpecificationRepository<ContactDbContext, PhoneBookInfoView, Guid>>();

            // 📌 رجیستر HostedService برای مقداردهی اولیه ماژول
            services.AddHostedService<ModuleInitializer>();

            // 📌 رجیستر OutboxProcessor برای پردازش رویدادهای دامنه
            var registration = services.BuildServiceProvider()
                                       .GetRequiredService<IOutboxProcessorRegistration>();
            registration.AddOutboxProcessor<ContactDbContext>(services);

            return services;
        }
    }
}

