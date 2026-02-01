
using Core.Application.Abstractions.Events;
using Core.Application.Abstractions.HR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using User.Application.Interfaces;
using User.Infrastructure.Data;
using User.Infrastructure.Services;
namespace User.Infrastructure.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection User_AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            var conn = configuration.GetConnectionString("DefaultConnection");
            var migrationsAssembly = typeof(UserDbContext).Assembly.GetName().Name;

            // DbContext برای Userها
            services.AddDbContext<UserDbContext>((serviceProvider, options) =>
            {
                options.UseSqlServer(conn, b =>
                {
                    b.MigrationsAssembly(migrationsAssembly);
                    b.MigrationsHistoryTable("__UserMigrationsHistory", "user");
                });
            });
            services.AddScoped<IPersonPublicService>(sp => sp.GetRequiredService<PersonService>());
            services.AddScoped<IPersonInternalService>(sp => sp.GetRequiredService<PersonService>());
            // Resolve از DI
            var registration = services.BuildServiceProvider()
                                       .GetRequiredService<IOutboxProcessorRegistration>();
            registration.AddOutboxProcessor<UserDbContext>(services);
            return services;
        }

    }
}
