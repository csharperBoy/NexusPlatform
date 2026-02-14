using Authorization.Application.DTOs.Permissions;
using Authorization.Application.Interfaces;
using Core.Application.Abstractions.Authorization;
using Core.Application.Abstractions.HR;
using Core.Application.Abstractions.Identity;
using Core.Application.Abstractions.Security;
using Core.Application.Context;
using Core.Application.Provider;
using Core.Domain.Enums;
using Core.Shared.DTOs.Identity;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Authorization.Application.Context
{
    public class DataScopeContextProvider : IDataScopeContextProvider
    {
        private readonly IPermissionInternalService _permissionService;
        private readonly IPermissionProcessor _permissionProcess;
        private readonly IAuthorizationChecker _authorizationChecker;
        private readonly IHttpContextAccessor _httpContext;
        private readonly IUserPublicService _userService;
        private readonly IPositionPublicService _positionService;
        private readonly IRolePublicService _roleService;

        public DataScopeContextProvider(
            IPermissionInternalService permissionService,
            IPermissionProcessor permissionProcess,
            IAuthorizationChecker authorizationChecker,
            IHttpContextAccessor httpContext,
            IUserPublicService userService,
            IPositionPublicService positionService,
            IRolePublicService roleService
            )
        {
            _permissionService = permissionService;
            _authorizationChecker = authorizationChecker;
            _httpContext = httpContext;
            _userService = userService;
            _positionService = positionService;
            _permissionProcess = permissionProcess;
            _roleService = roleService;
        }
        public async Task<DataScopeContext> GetAsync(CancellationToken ct)
        {



            var userIdstr = _httpContext.HttpContext?.User?
                       .FindFirst(ClaimTypes.NameIdentifier)?.Value;
            Guid userId = string.IsNullOrEmpty(userIdstr) ? Guid.Empty : Guid.Parse(userIdstr);
            if (userId == Guid.Empty)
                return new DataScopeContext { Permissions = new HashSet<PermissionDto> { } };

            Guid? PersonId = await _userService.GetPersonId(userId);
            List<Guid>? PositionId = await _positionService.GetUserPositionsId(userId);
            List<Guid> RoleIds = await _roleService.GetAllUserRolesId(userId);
            List<Guid>? OrgIds = await _positionService.GetUserOrganizeId(userId);
            List<PermissionDto> allPermission = new List<PermissionDto>();
            var userPermissions = await _permissionService.GetUserPermissionsAsync(userId);
            var rolePermissions = await _permissionService.GetRolePermissionsAsync(RoleIds);
            var personPermissions = await _permissionService.GetPersonPermissionsAsync(PersonId);
            var positionPermissions = await _permissionService.GetPositionPermissionsAsync(PositionId);
            allPermission.AddRange(userPermissions);
            allPermission.AddRange(rolePermissions);
            allPermission.AddRange(personPermissions);
            allPermission.AddRange(positionPermissions);
            return new DataScopeContext
            {
                UserId = userId,
                PersonId = PersonId,
                PositionIds = PositionId?.ToHashSet(),
                OrganizationUnitIds = OrgIds?.ToHashSet(),
                RoleIds = RoleIds.ToHashSet(),
                Permissions = allPermission.ToHashSet(),
                //userPermissions = userPermissions.ToHashSet(),
                //rolePermissions = rolePermissions.ToHashSet(),
                //personPermissions = personPermissions.ToHashSet(),
                //positionPermissions = positionPermissions.ToHashSet(),
            };
        }
    }

}
