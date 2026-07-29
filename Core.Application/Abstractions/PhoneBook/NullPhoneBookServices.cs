using Core.Application.Abstractions.People;
using Core.Domain.ValueObjects;
using Core.Shared.Enums.HR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Application.Abstractions.PhoneBook
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection PhoneBook_NullServiceInject(this IServiceCollection services, IConfiguration configuration)
        {

            services.AddScoped<IPhoneBookPublicService, NullPhoneBookServices>();
            return services;
        }
    }
    public class NullPhoneBookServices : IPhoneBookPublicService
    {
       
    }
}
