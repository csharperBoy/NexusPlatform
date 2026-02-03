using Authorization.Application.Interfaces;
using Authorization.Domain.Entities;
using Authorization.Domain.Enums;
using Core.Application.Abstractions.Security;
using Core.Domain.Attributes;
using Core.Domain.Common;
using Core.Domain.Enums;
using Core.Infrastructure.Security;
using Microsoft.EntityFrameworkCore.DynamicLinq; // یا System.Linq.Dynamic.Core
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Reflection;

namespace Authorization.Infrastructure.Services
{
    public class DataScopeProcessor : IDataScopeProcessor
    {

        private readonly IAuthorizationChecker _permissionChecker;
        private readonly IPermissionInternalService _permissionService;
        private readonly ICurrentUserService _currentUserService;
        public DataScopeProcessor(ICurrentUserService currentUserService, IPermissionInternalService permissionService , IAuthorizationChecker permissionChecker)
        {
            _currentUserService = currentUserService;
            _permissionService = permissionService;
            _permissionChecker = permissionChecker;
        }



        public async Task<IQueryable<TEntity>> ApplyScope<TEntity>(IQueryable<TEntity> query) where TEntity : class
        {
            try
            {

            // 🚨 جلوگیری از لوپ منطقی
            if (typeof(TEntity) == typeof(Permission) || typeof(TEntity) == typeof(Resource))
                return query;

            // بخش IResourcedEntity
            if (typeof(IResourcedEntity).IsAssignableFrom(typeof(TEntity)))
            {
                
                    Guid userId = _currentUserService.UserId ?? Guid.Empty;
                    if (userId == Guid.Empty)
                        return query.Where("EquivalentResourceId == null");

                    var permissions = await _permissionService.GetUserPermissionsAsync(userId);
                    var allowedResourceIds = permissions.Select(p => p.ResourceId).ToList();

                    if (!allowedResourceIds.Any())
                        return query.Where("EquivalentResourceId == null");

                    // استفاده از متد Where با رشته - این قسمت باید کار کند
                    return query.Where("@0.Contains(EquivalentResourceId) || EquivalentResourceId == null", allowedResourceIds);
                
            }

            // بخش IDataScopedEntity
            if (!typeof(IDataScopedEntity).IsAssignableFrom(typeof(TEntity)))
                return query;

            var attribute = typeof(TEntity).GetCustomAttribute<SecuredResourceAttribute>();
            if (attribute == null)
                return query;

            var personId =(await _currentUserService.GetUserContext()).PersonId;
            if (personId == null)
                return query.Where("false");

            
                var scopes = await _permissionChecker.GetScopeForUser(personId.Value, attribute.ResourceKey);

                if (!scopes.Any())
                    return query.Where("false");

                if (scopes.Contains(ScopeType.All))
                    return query;

                if (scopes.Contains(ScopeType.None))
                    return query.Where("false");

                var maxScope = scopes.Max();

                if (maxScope == ScopeType.Unit)
                {
                    var userUnitId = _currentUserService.OrganizationUnitId;
                    return userUnitId.HasValue
                        ? query.Where("OwnerOrganizationUnitId == @0", userUnitId.Value)
                        : query.Where("false");
                }

                if (maxScope == ScopeType.Self)
                {
                    return query.Where("OwnerPersonId == @0", personId.Value);
                }
            

            return query.Where("false");

            }
            catch (Exception ex)
            {

                throw;
            }
        }
    }
}
