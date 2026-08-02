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
            services.AddScoped<IEmploymentPublicService, NullEmploymentService>();
            return services;
        }
    }
    
    public class NullOrgChartService : IOrgChartPublicService
    {

        public Task<List<Guid?>?> GetEmploymentOrganizeId(Guid? employmentId)
        {
            return Task.FromResult<List<Guid?>?>(null);
        }


        public Task<List<Guid>?> GetEmploymentPostsId(Guid? employmentId)
        {
            return Task.FromResult<List<Guid>?>(null);
        }

        public Task<List<Guid>?> GetEmploymentPostsPermissionAssigneeId(Guid? employmentId)
        {
            return null;
        }
    }
    public class NullEmploymentService : IEmploymentPublicService
    {
        public Task<Guid?> GetEmploymentId(Guid? personId)
        {
            return Task.FromResult<Guid?>(null);
        }

        public async Task SaveAsync()
        {
            await Task.CompletedTask;
        }
    }
}
