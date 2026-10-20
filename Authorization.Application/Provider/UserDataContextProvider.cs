using Authorization.Application.Interfaces.Service;
using Core.Application.Abstractions.HR;
using Core.Application.Abstractions.Identity.PublicService;
using Core.Application.Abstractions.People;
using Core.Application.Context;
using Core.Application.Helper;
using Core.Application.Provider;
using Core.Domain.Enums;
using Core.Shared.DTOs.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace Authorization.Application.Provider;

public class UserDataContextProvider : IUserDataContextProvider
{
    private readonly IUserPublicService _userService;
    private readonly IPersonPublicService _personService;
    private readonly IRolePublicService _roleService;
    private readonly IEmploymentPublicService _employmentService;
    private readonly IPostPublicService _positionService;
    private readonly IPermissionInternalService _permissionService;
    private readonly IHttpContextAccessor _httpContext;
    private readonly UserDataContext _userDataContext;
    private readonly ILogger<UserDataContextProvider> _logger;

    public UserDataContextProvider(
        UserDataContext userDataContext,
        IHttpContextAccessor httpContext,
        ILogger<UserDataContextProvider> logger,
        IUserPublicService userService,
        IPersonPublicService personService,
        IRolePublicService roleService,
        IEmploymentPublicService employmentService,
        IPostPublicService positionService,
        IPermissionInternalService permissionService)
    {
        _httpContext = httpContext;
        _userDataContext = userDataContext;
        _logger = logger;
        _userService = userService;
        _personService = personService;
        _roleService = roleService;
        _employmentService = employmentService;
        _positionService = positionService;
        _permissionService = permissionService;
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

        _logger.LogInformation(
            "BuildAsync: loading user context for userId={UserId}", userId);

        var userPermissionAssigneeId =
            await _userService.GetUserPermissionAssigneeIdAsync(userId);

        Guid? partyId = null;
        Guid? personId = null;
        Guid? partyPermissionAssigneeId = null;
        Guid? employmentId = null;
        List<Guid>? postIds = null;
        List<Guid>? postPermissionAssigneeIds = null;
        List<Guid?>? orgIds = null;

        /* ─── People ─── */
        if (ModuleHelper.IsActive(ModuleEnum.People))
        {
            try
            {
                partyId = await _userService.GetPartyId(userId);
                _logger.LogDebug("BuildAsync: partyId={PartyId}", partyId);

                if (partyId.HasValue && partyId.Value != Guid.Empty)
                {
                    personId = await _personService.GetNaturalPersonIdAsync(partyId);
                    partyPermissionAssigneeId =
                        await _personService.GetPartyPermissionAssigneeIdAsync(partyId);

                    _logger.LogDebug(
                        "BuildAsync: personId={PersonId} partyPermAssigneeId={PermId}",
                        personId, partyPermissionAssigneeId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex,
                    "BuildAsync: failed to load People info for userId={UserId}. Continuing.",
                    userId);
            }
        }

        /* ─── HR ─── */
        if (ModuleHelper.IsActive(ModuleEnum.HR)
            && personId.HasValue && personId.Value != Guid.Empty)
        {
            try
            {
                employmentId = await _employmentService.GetEmploymentId(personId);
                _logger.LogDebug("BuildAsync: employmentId={EmploymentId}", employmentId);

                if (employmentId.HasValue && employmentId.Value != Guid.Empty)
                {
                    postIds = await _positionService.GetEmploymentPostsId(employmentId);
                    postPermissionAssigneeIds =
                        await _positionService.GetEmploymentPostsPermissionAssigneeId(employmentId);
                    orgIds = await _positionService.GetEmploymentOrganizeId(employmentId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex,
                    "BuildAsync: failed to load HR info for userId={UserId}. Continuing.",
                    userId);
            }
        }

        /* ─── Roles ─── */
        List<Guid> roleIds = new();
        List<Guid> rolePermissionAssigneeIds = new();
        try
        {
            roleIds = await _roleService.GetAllUserRolesId(userId) ?? new List<Guid>();
            rolePermissionAssigneeIds =
                await _roleService.GetAllUserRolesPermissionAssigneeId(userId)
                ?? new List<Guid>();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex,
                "BuildAsync: failed to load roles for userId={UserId}. Continuing.",
                userId);
        }

        /* ─── Permissions ─── */
        IEnumerable<PermissionDto> allPermissions = Enumerable.Empty<PermissionDto>();
        try
        {
            var fetched = await _permissionService.GetUserAllPermissionsAsync(
                userPermissionAssigneeId,
                partyPermissionAssigneeId,
                postPermissionAssigneeIds,
                rolePermissionAssigneeIds);

            allPermissions = fetched ?? Enumerable.Empty<PermissionDto>();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex,
                "BuildAsync: failed to load permissions for userId={UserId}. Using empty.",
                userId);
        }

        _logger.LogInformation(
            "BuildAsync: completed userId={UserId} permissions={PermCount} roles={RoleCount}",
            userId,
            allPermissions.Count(),
            roleIds.Count);

        return UserDataContext.Create(
            userId: userId,
            userPermissionAssigneeId: userPermissionAssigneeId,
            userName: null,
            partyId: partyId,
            partyPermissionAssigneeId: partyPermissionAssigneeId,
            organizationUnitIds: orgIds,
            postIds: postIds,
            postPermissionAssigneeIds: postPermissionAssigneeIds,
            roleIds: roleIds,
            rolePermissionAssigneeIds: rolePermissionAssigneeIds,
            permissions: allPermissions);
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
    /* ═══════════════════ Helpers ═══════════════════ */

    private Guid ExtractUserIdFromHttpContext()
    {
        var userIdStr = _httpContext.HttpContext?.User?
            .FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrWhiteSpace(userIdStr)) return Guid.Empty;
        return Guid.TryParse(userIdStr, out var id) ? id : Guid.Empty;
    }
}