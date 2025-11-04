using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Notification.Application.Interfaces;
using Notification.Infrastructure.Models;
using Notification.Infrastructure.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Notification.Infrastructure.DependencyInjection
{
  public static  class ServiceCollectionExtensions
    {
        public static IServiceCollection Notification_AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<IEmailSender, SmtpEmailSender>();
            services.Configure<SmtpOptions>(configuration.GetSection("Smtp"));


            return services;
        }

    }
}
