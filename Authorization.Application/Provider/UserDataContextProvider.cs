using Authorization.Application.DTOs.Permissions;
using Authorization.Application.Interfaces.Service;
using Core.Application.Abstractions.Authorization;
using Core.Application.Abstractions.HR;
using Core.Application.Abstractions.Identity.PublicService;
using Core.Application.Abstractions.People;
using Core.Application.Context;
using Core.Application.Provider;
using Core.Domain.Enums;
using Core.Shared.DTOs.Authorization;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Authorization.Application.Provider
{
    public class UserDataContextProvider : IUserDataContextProvider
    {
        private readonly IPermissionInternalService _permissionService;
        private readonly IHttpContextAccessor _httpContext;
        private readonly IUserPublicService _userService;
        private readonly IOrgChartPublicService _positionService;
        private readonly IRolePublicService _roleService;
        private readonly IPersonPublicService _personService;
        private readonly IEmployeePublicService _employeeService;

        private readonly UserDataContext _userDataContext;

        public UserDataContextProvider(
            IPermissionInternalService permissionService,
           UserDataContext userDataContext,
            IHttpContextAccessor httpContext,
            IUserPublicService userService,
            IOrgChartPublicService positionService,
            IRolePublicService roleService,
            IPersonPublicService personService,
             IEmployeePublicService employeeService
            )
        {
            _personService = personService;
            _permissionService = permissionService;
            _httpContext = httpContext;
            _userService = userService;
            _positionService = positionService;
            _roleService = roleService;
            _userDataContext = userDataContext;
            _employeeService = employeeService;
        }
        public async Task<UserDataContext> GetAsync(CancellationToken ct)
        {

            
            var userIdstr = _httpContext.HttpContext?.User?
                       .FindFirst(ClaimTypes.NameIdentifier)?.Value;
            Guid userId = string.IsNullOrEmpty(userIdstr) ? Guid.Empty : Guid.Parse(userIdstr);
            if (userId == Guid.Empty)
                return new UserDataContext { Permissions = new HashSet<PermissionDto> { } };

            

            Guid? PartyId = await _userService.GetPartyId(userId);
            Guid? partyPermissionAssigneeId = await _personService.GetPartyPermissionAssigneeIdAsync(PartyId);
            Guid? personId = await _personService.GetNaturalPersonIdAsync(PartyId);
            Guid userPermissionAssigneeId = await _userService.GetUserPermissionAssigneeIdAsync(userId);
            Guid? EmployeeId = await _employeeService.GetEmployeeId(personId);
            //List<Guid>? PostId = await _positionService.GetEmployeePostsId(EmployeeId);
            List<Guid>? PostPermissionAssigneeId = await _positionService.GetEmployeePostsPermissionAssigneeId(EmployeeId);
            List<Guid>? OrgIds = await _positionService.GetEmployeeOrganizeId(EmployeeId);

            List<Guid> RolePermissionAssigneeIds = await _roleService.GetAllUserRolesPermissionAssigneeId(userId);
            var allPermission = await _permissionService.GetUserAllPermissionsAsync(userPermissionAssigneeId, partyPermissionAssigneeId, PostPermissionAssigneeId, RolePermissionAssigneeIds);



            return new UserDataContext
            {
                UserId = userId,
                PartyId = PartyId,
                PostIds = PostPermissionAssigneeId?.ToHashSet(),
                OrganizationUnitIds = OrgIds?.ToHashSet(),
                RoleIds = RolePermissionAssigneeIds.ToHashSet(),
                Permissions = allPermission.ToHashSet(),
            };
        }

        public async Task SetUserData(CancellationToken ct)
        {
            var ctx = await GetAsync(ct);

            // مقداردهی Scoped Instance
            typeof(UserDataContext)
                .GetProperty(nameof(UserDataContext.UserId))!
                .SetValue(_userDataContext, ctx.UserId);

            typeof(UserDataContext)
                .GetProperty(nameof(UserDataContext.PartyId))!
                .SetValue(_userDataContext, ctx.PartyId);

            typeof(UserDataContext)
                .GetProperty(nameof(UserDataContext.OrganizationUnitIds))!
                .SetValue(_userDataContext, ctx.OrganizationUnitIds);

            typeof(UserDataContext)
                .GetProperty(nameof(UserDataContext.PostIds))!
                .SetValue(_userDataContext, ctx.PostIds);

            typeof(UserDataContext)
                .GetProperty(nameof(UserDataContext.RoleIds))!
                .SetValue(_userDataContext, ctx.RoleIds);

            typeof(UserDataContext)
                .GetProperty(nameof(UserDataContext.Permissions))!
                .SetValue(_userDataContext, ctx.Permissions);
        }
    }

}
