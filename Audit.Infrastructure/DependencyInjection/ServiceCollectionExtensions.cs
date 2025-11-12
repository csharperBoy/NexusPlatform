using Audit.Application.Interfaces;
using Audit.Domain.Entities;
using Audit.Infrastructure.Data;
using Audit.Infrastructure.Services;
using Core.Application.Abstractions;
using Core.Application.Abstractions.Auditing;
using Core.Application.Abstractions.Events;
using Core.Domain.Interfaces;
using Core.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
namespace Audit.Infrastructure.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection Audit_AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            var conn = configuration.GetConnectionString("DefaultConnection");
            var migrationsAssembly = typeof(AuditDbContext).Assembly.GetName().Name;

            // DbContext برای Userها
            services.AddDbContext<AuditDbContext>((serviceProvider, options) =>
            {
                options.UseSqlServer(conn, b =>
                {
                    b.MigrationsAssembly(migrationsAssembly);
                    b.MigrationsHistoryTable("__AuditMigrationsHistory", "audit");
                });
            });
            services.AddScoped<IUnitOfWork<AuditDbContext>, EfUnitOfWork<AuditDbContext>>();

            services.AddScoped<ISpecificationRepository<AuditLog, Guid>, EfSpecificationRepository<AuditDbContext, AuditLog, Guid>>();
            services.AddScoped<IAuditService, AuditService>();
            services.AddScoped<IAuditQueryService, AuditQueryService>();

            // Resolve از DI
            var registration = services.BuildServiceProvider()
                                       .GetRequiredService<IOutboxProcessorRegistration>();
            registration.AddOutboxProcessor<AuditDbContext>(services);
            return services;
        }

    }
}
