using Authorization.Application.Attributes;
using Authorization.Application.Context;
using Authorization.Application.Interfaces;
using Core.Application.Behaviors;
using Core.Application.Provider;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Authorization.Application.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
       
        public static IServiceCollection Authorization_AddApplication(this IServiceCollection services, IConfiguration configuration)
        {
            
            // رجیستر MediatR و همه Handlerهای موجود در اسمبلی Application
            services.AddMediatR(cfg =>
               cfg.RegisterServicesFromAssembly(typeof(ServiceCollectionExtensions).Assembly));

            services.AddScoped<IDataScopeContextProvider , DataScopeContextProvider>(); 
            services.AddValidatorsFromAssembly(typeof(ServiceCollectionExtensions).Assembly);

            return services;
        }
    }
}