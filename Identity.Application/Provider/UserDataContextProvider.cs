using Core.Application.Abstractions.Authorization.PublicService;
using Core.Application.Abstractions.HR;
using Core.Application.Abstractions.Identity.PublicService;
using Core.Application.Abstractions.People;
using Core.Application.Context;
using Core.Application.Helper;
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

namespace Identity.Application.Provider
{

    public class UserDataContextProvider : IUserDataContextProvider
    {
        private readonly IUserPublicService _userService;
        private readonly IPersonPublicService _personService;
        private readonly IRolePublicService _roleService;
        private readonly IEmploymentPublicService _employmentService;
        private readonly IPostPublicService _positionService;
        private readonly IHttpContextAccessor _httpContext;
        private readonly UserDataContext _userDataContext;

        public UserDataContextProvider(
            UserDataContext userDataContext,
            IHttpContextAccessor httpContext,
            IUserPublicService userService,
            IPersonPublicService personService,
            IRolePublicService roleService,
            IEmploymentPublicService employmentService,
            IPostPublicService positionService)
        {
            _httpContext = httpContext;
            _userDataContext = userDataContext;
            _userService = userService;
            _personService = personService;
            _roleService = roleService;
            _employmentService = employmentService;
            _positionService = positionService;
        }
        /// <summary>
        /// Snapshot کامل کاربر جاری از HttpContext.
        /// برای استفاده در Command/Query Handlerها.
        /// </summary>
        public async Task<UserDataContext> GetAsync(CancellationToken ct)
        {
            var userId = ExtractUserIdFromHttpContext();
            return await BuildAsync(userId, ct);
        }
        /* ═══════════════════ HTTP Flow ═══════════════════ */

        public async Task SetUserData(CancellationToken ct)
        {
            var userId = ExtractUserIdFromHttpContext();
            if (userId == Guid.Empty)
            {
                _userDataContext.ApplyFrom(UserDataContext.Empty());
                return;
            }

            await SetUserData(userId, ct);
        }

        /* ═══════════════════ Background Flow ═══════════════════ */

        public async Task SetUserData(Guid userId, CancellationToken ct)
        {
            var snapshot = await BuildAsync(userId, ct);
            _userDataContext.ApplyFrom(snapshot);
        }

        /* ═══════════════════ Snapshot Builder ═══════════════════ */

        public async Task<UserDataContext> BuildAsync(Guid userId, CancellationToken ct)
        {
            if (userId == Guid.Empty)
                return UserDataContext.Empty();

            var userPermissionAssigneeId =
                await _userService.GetUserPermissionAssigneeIdAsync(userId);

            Guid? partyId = null;
            Guid? personId = null;
            Guid? partyPermissionAssigneeId = null;
            Guid? employmentId = null;
            List<Guid>? postIds = null;
            List<Guid>? postPermissionAssigneeIds = null;
            List<Guid?>? orgIds = null;

            if (ModuleHelper.IsActive(ModuleEnum.People))
            {
                partyId = await _userService.GetPartyId(userId);
                personId = await _personService.GetNaturalPersonIdAsync(partyId);
                partyPermissionAssigneeId =
                    await _personService.GetPartyPermissionAssigneeIdAsync(partyId);
            }

            if (ModuleHelper.IsActive(ModuleEnum.HR))
            {
                employmentId = await _employmentService.GetEmploymentId(personId);
                postIds = await _positionService.GetEmploymentPostsId(employmentId);
                postPermissionAssigneeIds =
                    await _positionService.GetEmploymentPostsPermissionAssigneeId(employmentId);
                orgIds = await _positionService.GetEmploymentOrganizeId(employmentId);
            }

            var roleIds = await _roleService.GetAllUserRolesId(userId);
            var rolePermissionAssigneeIds =
                await _roleService.GetAllUserRolesPermissionAssigneeId(userId);

            var allPermissions = new List<PermissionDto>();

            return UserDataContext.Create(
                userId: userId,
                userPermissionAssigneeId: userPermissionAssigneeId,
                userName: null, // از Identity در صورت نیاز پر شود
                partyId: partyId,
                partyPermissionAssigneeId: partyPermissionAssigneeId,
                organizationUnitIds: orgIds,
                postIds: postIds,
                postPermissionAssigneeIds: postPermissionAssigneeIds,
                roleIds: roleIds,
                rolePermissionAssigneeIds: rolePermissionAssigneeIds,
                permissions: allPermissions);
        }

        /* ═══════════════════ Helpers ═══════════════════ */

        private Guid ExtractUserIdFromHttpContext()
        {
            var userIdStr = _httpContext.HttpContext?.User?
                .FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrWhiteSpace(userIdStr)) return Guid.Empty;
            return Guid.TryParse(userIdStr, out var id) ? id : Guid.Empty;
        }
    }
}
