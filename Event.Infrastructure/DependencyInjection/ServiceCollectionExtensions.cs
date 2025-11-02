using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Cors; // اضافه کردن این using
using Core.Application.Models;
using Core.Application.Abstractions;
using Core.Application.Abstractions.Caching;
using Core.Application.Abstractions.Events;
using Core.Infrastructure.HealthChecks;
using Core.Infrastructure.Repositories;
using MediatR;
using Core.Application.Behaviors;
using Core.Infrastructure.Logging;
using Microsoft.Extensions.Logging;
using Serilog;
using Core.Infrastructure.Database;
using Microsoft.AspNetCore.Builder;
using Core.Infrastructure.Resilience;
using Event.Infrastructure.Services;


namespace Event.Infrastructure.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection Event_AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<IEventBus, MediatorEventBus>();
            services.AddOutboxServices();
            return services;
        }
      
        private static IServiceCollection AddOutboxServices(this IServiceCollection services)
        {
            services.AddTransient(typeof(IOutboxService<>), typeof(OutboxService<>));
            return services;
        }
       
    }
}
