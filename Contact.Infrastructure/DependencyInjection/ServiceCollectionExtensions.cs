using Contact.Application.Interfaces;
using Contact.Domain.Entities;
using Contact.Infrastructure.Data;
using Contact.Infrastructure.Services;
using Core.Application.Abstractions;
using Core.Application.Abstractions.Contact;
using Core.Application.Abstractions.Events;
using Core.Application.Abstractions.HR;
using Core.Infrastructure.Repositories;
using HR.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Contact.Infrastructure.DependencyInjection
{
  
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection Contact_AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
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
                    b.MigrationsHistoryTable("__contactMigrationsHistory", "contact");
                });
            });
            services.AddScoped<PhoneBookService>();
            services.AddScoped<IPhoneBookPublicService>(sp => sp.GetRequiredService<PhoneBookService>());
            services.AddScoped<IPhoneBookInternalService>(sp => sp.GetRequiredService<PhoneBookService>());
            services.AddScoped<IPhoneBookInternalService, PhoneBookService>();
            
            services.AddScoped<ContactService>();
            services.AddScoped<IHrContactPublicService>(sp => sp.GetRequiredService<ContactService>());
            services.AddScoped<IPeopleContactPublicService>(sp => sp.GetRequiredService<ContactService>());
            services.AddScoped<IContactInternalService>(sp => sp.GetRequiredService<ContactService>());
            services.AddScoped<IContactInternalService, ContactService>();
            
            services.AddScoped<IUnitOfWork<ContactDbContext>, EfUnitOfWork<ContactDbContext>>();
            // 📌 رجیستر Repository مبتنی بر Specification
            services.AddScoped<ISpecificationRepository<PhoneBookInfoView, Guid>, EfSpecificationRepository<ContactDbContext, PhoneBookInfoView, Guid>>();

            services.AddScoped<IRepository<ContactDbContext, PartyContact, Guid>, EfRepository<ContactDbContext, PartyContact, Guid>>();
            services.AddScoped<ISpecificationRepository<PartyContact, Guid>, EfSpecificationRepository<ContactDbContext, PartyContact, Guid>>();


            services.AddScoped<IRepository<ContactDbContext, PostContact, Guid>, EfRepository<ContactDbContext, PostContact, Guid>>();
            services.AddScoped<ISpecificationRepository<PostContact, Guid>, EfSpecificationRepository<ContactDbContext, PostContact, Guid>>();

            services.AddScoped<IRepository<ContactDbContext, EmploymentContact, Guid>, EfRepository<ContactDbContext, EmploymentContact, Guid>>();
            services.AddScoped<ISpecificationRepository<EmploymentContact, Guid>, EfSpecificationRepository<ContactDbContext, EmploymentContact, Guid>>();

            services.AddScoped<IRepository<ContactDbContext, LocationContact, Guid>, EfRepository<ContactDbContext, LocationContact, Guid>>();
            services.AddScoped<ISpecificationRepository<LocationContact, Guid>, EfSpecificationRepository<ContactDbContext, LocationContact, Guid>>();

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

