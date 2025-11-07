using Core.Application.Abstractions;
using Core.Application.Abstractions.Events;
using Core.Domain.Interfaces;
using Core.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Sample.Domain.Entities;
using Sample.Infrastructure.Data;
namespace Sample.Infrastructure.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection Sample_AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            var conn = configuration.GetConnectionString("DefaultConnection");
            var migrationsAssembly = typeof(SampleDbContext).Assembly.GetName().Name;

            // DbContext برای Userها
            services.AddDbContext<SampleDbContext>((serviceProvider, options) =>
            {
                options.UseSqlServer(conn, b =>
                {
                    b.MigrationsAssembly(migrationsAssembly);
                    b.MigrationsHistoryTable("__SampleMigrationsHistory", "sample");
                });
            });
            services.AddScoped<ISpecificationRepository<SampleEntity, Guid>, EfSpecificationRepository<SampleDbContext, SampleEntity, Guid>>();

            services.AddHostedService<ModuleInitializer>();
            // Resolve از DI
            var registration = services.BuildServiceProvider()
                                       .GetRequiredService<IOutboxProcessorRegistration>();
            registration.AddOutboxProcessor<SampleDbContext>(services);
            return services;
        }

    }
}
