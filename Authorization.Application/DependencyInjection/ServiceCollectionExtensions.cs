using Authorization.Application.Attributes;
using Authorization.Application.Interfaces;
using Core.Application.Behaviors;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using FluentValidation;

namespace Authorization.Application.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
       
        public static IServiceCollection Authorization_AddApplication(this IServiceCollection services, IConfiguration configuration)
        {
            
            // رجیستر MediatR و همه Handlerهای موجود در اسمبلی Application
            services.AddMediatR(cfg =>
               cfg.RegisterServicesFromAssembly(typeof(ServiceCollectionExtensions).Assembly));


            services.AddValidatorsFromAssembly(typeof(ServiceCollectionExtensions).Assembly);

            return services;
        }
    }
}