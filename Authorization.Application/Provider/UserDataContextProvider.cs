using Authorization.Application.Interfaces.Service;
using Core.Application.Abstractions.HR;
using Core.Application.Abstractions.Identity.PublicService;
using Core.Application.Abstractions.People;
using Core.Application.Context;
using Core.Application.Provider;
using Core.Shared.DTOs.Authorization;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace Authorization.Application.Provider
{
    public class UserDataContextProvider : IUserDataContextProvider
    {
        private readonly IUserPublicService _userService;
        private readonly IPersonPublicService _personService;

        private readonly IRolePublicService _roleService;
        private readonly IEmployeePublicService _employeeService;

        private readonly IOrgChartPublicService _positionService;
       private readonly IPermissionInternalService _permissionService;
        private readonly IHttpContextAccessor _httpContext;
        private readonly UserDataContext _userDataContext;

        public UserDataContextProvider(
           UserDataContext userDataContext,
            IHttpContextAccessor httpContext,
            IUserPublicService userService,
            IPersonPublicService personService,

            IRolePublicService roleService,
            IEmployeePublicService employeeService,

            IOrgChartPublicService positionService,
            IPermissionInternalService permissionService
            )
        {
            _httpContext = httpContext;
            _userDataContext = userDataContext;
           _userService = userService;
            _personService = personService;

            _roleService = roleService;
            _employeeService = employeeService;

            _positionService = positionService;
            _permissionService = permissionService;
        }
        public async Task<UserDataContext> GetAsync(CancellationToken ct)
        {


            var userIdstr = _httpContext.HttpContext?.User?
                       .FindFirst(ClaimTypes.NameIdentifier)?.Value;
            Guid userId = string.IsNullOrEmpty(userIdstr) ? Guid.Empty : Guid.Parse(userIdstr);
            if (userId == Guid.Empty)
                return new UserDataContext { Permissions = new HashSet<PermissionDto> { } };
            Guid userPermissionAssigneeId = await _userService.GetUserPermissionAssigneeIdAsync(userId);



            Guid? PartyId = await _userService.GetPartyId(userId);
            Guid? personId = await _personService.GetNaturalPersonIdAsync(PartyId);
            Guid? partyPermissionAssigneeId = await _personService.GetPartyPermissionAssigneeIdAsync(PartyId);
            
            Guid? EmployeeId = await _employeeService.GetEmployeeId(personId);
            
            List<Guid>? PostId = await _positionService.GetEmployeePostsId(EmployeeId);
            List<Guid>? PostPermissionAssigneeId = await _positionService.GetEmployeePostsPermissionAssigneeId(EmployeeId);

            List<Guid> RoleIds = await _roleService.GetAllUserRolesId(userId);
            List<Guid> RolePermissionAssigneeIds = await _roleService.GetAllUserRolesPermissionAssigneeId(userId);
            
            List<Guid?>? OrgIds = await _positionService.GetEmployeeOrganizeId(EmployeeId);
            var allPermission = await _permissionService.GetUserAllPermissionsAsync(userPermissionAssigneeId, partyPermissionAssigneeId, PostPermissionAssigneeId, RolePermissionAssigneeIds);



            return new UserDataContext
            {
                
                UserId = userId,
                UserPermissionAssigneeId = userPermissionAssigneeId,
                
                PartyId = PartyId,
                PartyPermissionAssigneeId = partyPermissionAssigneeId,
                
                PostIds = PostId?.ToHashSet(),
                PostPermissionAssigneeIds = PostPermissionAssigneeId?.ToHashSet(),
                
                
                RoleIds = RoleIds.ToHashSet(),
                RolePermissionAssigneeIds = RolePermissionAssigneeIds.ToHashSet(),
              
                OrganizationUnitIds = OrgIds?.ToHashSet(),
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
                .GetProperty(nameof(UserDataContext.UserPermissionAssigneeId))!
                .SetValue(_userDataContext, ctx.UserPermissionAssigneeId);

            typeof(UserDataContext)
                .GetProperty(nameof(UserDataContext.PartyId))!
                .SetValue(_userDataContext, ctx.PartyId);

            typeof(UserDataContext)
                .GetProperty(nameof(UserDataContext.PartyPermissionAssigneeId))!
                .SetValue(_userDataContext, ctx.PartyPermissionAssigneeId);

            typeof(UserDataContext)
                .GetProperty(nameof(UserDataContext.OrganizationUnitIds))!
                .SetValue(_userDataContext, ctx.OrganizationUnitIds);

            typeof(UserDataContext)
                .GetProperty(nameof(UserDataContext.PostIds))!
                .SetValue(_userDataContext, ctx.PostIds);

            typeof(UserDataContext)
                .GetProperty(nameof(UserDataContext.PostPermissionAssigneeIds))!
                .SetValue(_userDataContext, ctx.PostPermissionAssigneeIds);

            typeof(UserDataContext)
                .GetProperty(nameof(UserDataContext.RoleIds))!
                .SetValue(_userDataContext, ctx.RoleIds);

            typeof(UserDataContext)
                .GetProperty(nameof(UserDataContext.RolePermissionAssigneeIds))!
                .SetValue(_userDataContext, ctx.RolePermissionAssigneeIds);

            typeof(UserDataContext)
                .GetProperty(nameof(UserDataContext.Permissions))!
                .SetValue(_userDataContext, ctx.Permissions);
        }
    }

}
