using Core.Application.Abstractions.Authorization.Processor;
using Core.Application.Abstractions.Authorization.PublicService;
using Core.Application.Abstractions.Identity;
using Core.Application.Context;
using Core.Shared.DTOs.Authorization;
using Core.Shared.Enums.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace Core.Application.Abstractions.Authorization
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection Authorization_NullInject(this IServiceCollection services, IConfiguration configuration)
        {

            services.AddTransient(typeof(IRowLevelSecurityProcessor<>), typeof(NullRowLevelSecurityProcessor<>));
            services.AddScoped<IResourcePublicService, NullResourceService>();
            services.AddScoped<IPermissionPublicService, NullPermissionService>();
            services.AddScoped<IAuthorizationProcessor, NullAuthorizationProcessor>();
            services.AddScoped<IPermissionProcessor, NullPermissionProcessor>();

            return services;
        }
    }
    public class NullResourceService : IResourcePublicService
    {
        public Task SyncModuleResourcesAsync(List<ResourceDto> resources, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask; 
        }
    }
    public class NullPermissionService : IPermissionPublicService
    {
      

        public Task<Guid> CreatePermissionAssigneeAsync(AssigneeType type, CancellationToken cancellationToken = default)
        {
            return null;
        }

        public Task<IReadOnlyList<PermissionDto>> GetUserAllPermissionsAsync(Guid userId, Guid? personId, List<Guid>? positionsId, List<Guid> roleIds)
        {
            IReadOnlyList<PermissionDto> ret = new List<PermissionDto>();
            return Task.FromResult(ret);
        }

        public Task SaveAsync()
        {
            return Task.CompletedTask;
        }

        public Task SeedRolePermissionsAsync(List<PermissionDto> permissions, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }
    public class NullAuthorizationProcessor : IAuthorizationProcessor
    {

        public Task<bool> CheckAccessAsync(string resourceKey, string action)
        {
            return Task.FromResult(true); 
        }
    }
    public class NullPermissionProcessor : IPermissionProcessor
    {
    }
    public class NullRowLevelSecurityProcessor<TEntity> : IRowLevelSecurityProcessor<TEntity>
    where TEntity : class
    {
        public Task<IQueryable<TEntity>> ApplyFilter(IQueryable<TEntity> query)
        {
            return Task.FromResult(query);
        }



        public Task CheckPermissionAsync(TEntity entity, PermissionAction action)
        {
            return Task.CompletedTask;
        }

        public Task SetOwnerDefaults(TEntity entity)
        {
            return Task.CompletedTask;
        }
    }
}
