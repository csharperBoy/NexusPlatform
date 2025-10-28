using Core.Application.Abstractions.Events;
using Core.Application.Abstractions;
using Core.Application.Behaviors;
using Core.Application.Models;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Core.Application.Abstractions.Caching;
using Microsoft.Extensions.Logging;

namespace Audit.Infrastructure.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection Audit_AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            return services;
        }

    }
}
