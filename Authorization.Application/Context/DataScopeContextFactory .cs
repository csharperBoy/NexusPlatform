using Authorization.Application.Interfaces;
using Core.Application.Abstractions.Security;
using Core.Application.Context;
using Core.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Authorization.Application.Context
{
    public class DataScopeContextFactory : IDataScopeContextFactory
    {
        private readonly ICurrentUserService _currentUser;
        private readonly IPermissionInternalService _permissionService;
        private readonly IAuthorizationChecker _authorizationChecker;

        public async Task<DataScopeContext> CreateAsync(string resourceKey)
        {
            var userId = _currentUser.UserId ?? Guid.Empty;
            if (userId == Guid.Empty)
                return new DataScopeContext { AllowedScopes = new HashSet<ScopeType> { ScopeType.None } };

            var userContext = await _currentUser.GetUserContext();

            var scopes = await _authorizationChecker
                .GetScopeForUser(userContext.PersonId!.Value, resourceKey);

            var permissions = await _permissionService
                .GetUserPermissionsAsync(userId);

            return new DataScopeContext
            {
                UserId = userId,
                PersonId = userContext.PersonId,
                OrganizationUnitId = userContext.OrganizationUnitId?.ToHashSet(),
                AllowedScopes = scopes.ToHashSet(),
                AllowedResourceIds = permissions.Select(p => p.ResourceId).ToHashSet()
            };
        }
    }

}
