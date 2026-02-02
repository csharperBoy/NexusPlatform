using Core.Application.Abstractions.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Application.Abstractions.HR
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection HR_NullServiceInject(this IServiceCollection services, IConfiguration configuration)
        {

            services.AddScoped<IPersonPublicService, NullPersonService>();
            services.AddScoped<IPositionPublicService, NullPositionService>();
            return services;
        }
    }
    public class NullPersonService : IPersonPublicService
    {
    }
    public class NullPositionService : IPositionPublicService
    {
        public Task<List<Guid>?> GetUserPositionsId(Guid userId)
        {
            return Task.FromResult<List<Guid>?>( null);
        }
    }

    }
