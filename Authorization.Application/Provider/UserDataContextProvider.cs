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
        private readonly IEmployeePublicService _employeeService;

        private readonly UserDataContext _userDataContext;

        public UserDataContextProvider(
            IPermissionInternalService permissionService,
           UserDataContext userDataContext,
            IHttpContextAccessor httpContext,
            IUserPublicService userService,
            IOrgChartPublicService positionService,
            IRolePublicService roleService,
             IEmployeePublicService employeeService
            )
        {
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

            Guid? PersonId = await _userService.GetPersonId(userId);
            Guid? EmployeeId = await _employeeService.GetEmployeeId(PersonId);
            string? userName = await _userService.GetUserName(userId);
            List<Guid>? PostId = await _positionService.GetEmployeePostsId(EmployeeId);
            List<Guid> RoleIds = await _roleService.GetAllUserRolesId(userId);
            List<Guid>? OrgIds = await _positionService.GetEmployeeOrganizeId(EmployeeId);
            var allPermission = await _permissionService.GetUserAllPermissionsAsync(userId, PersonId, PostId, RoleIds);
            
            return new UserDataContext
            {
                UserId = userId,
                PersonId = PersonId,
                PostIds = PostId?.ToHashSet(),
                OrganizationUnitIds = OrgIds?.ToHashSet(),
                RoleIds = RoleIds.ToHashSet(),
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
                .GetProperty(nameof(UserDataContext.PersonId))!
                .SetValue(_userDataContext, ctx.PersonId);

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
