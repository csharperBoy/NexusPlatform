using Core.Application.Abstractions.People;
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
            services.AddScoped<IOrgChartPublicService, NullOrgChartService>();
            services.AddScoped<IEmployeePublicService, NullEmployeeService>();
            return services;
        }
    }
    
    public class NullOrgChartService : IOrgChartPublicService
    {

        public Task<List<Guid>?> GetEmployeeOrganizeId(Guid? employeeId)
        {
            return Task.FromResult<List<Guid>?>(null);
        }


        public Task<List<Guid>?> GetEmployeePostsId(Guid? employeeId)
        {
            return Task.FromResult<List<Guid>?>(null);
        }

        public Task<List<Guid>?> GetEmployeePostsPermissionAssigneeId(Guid? employeeId)
        {
            return null;
        }
    }
    public class NullEmployeeService : IEmployeePublicService
    {
        public Task<Guid?> GetEmployeeId(Guid? personId)
        {
            return Task.FromResult<Guid?>(null);
        }

        public async Task SaveAsync()
        {
            await Task.CompletedTask;
        }
    }
}
