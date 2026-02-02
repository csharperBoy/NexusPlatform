using Core.Application.Abstractions.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Application.Abstractions.Identity
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection Identity_NullServiceInject(this IServiceCollection services, IConfiguration configuration)
        {

            services.AddScoped<IRolePublicService, NullRoleService>();
            services.AddScoped<IUserPublicService, NullUserService>();
            return services;
        }
    }
    public class NullRoleService : IRolePublicService
    {
        public Task<Guid> GetAdminRoleIdAsync(CancellationToken cancellationToken = default)
        {
            return null;
        }

        public Task<List<Guid>> GetAllUserRolesId(Guid userId)
        {
            return null;
        }

        public Task<IList<string>> GetUserRolesAsync(Guid userId)
        {
            return null;
        }
    }
    public class NullUserService : IUserPublicService
    {
        public Task<Guid?> GetPersonId(Guid userId)
        {
            return null;
        }

        public Task<Guid> GetUserId(string userName)
        {
            return null;
        }

        
    }

    }
