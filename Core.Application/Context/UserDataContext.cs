using Core.Shared.DTOs.Authorization;

namespace Core.Application.Context;

public sealed class UserDataContext
{
    public Guid UserId { get; private set; }
    public string? UserName { get; private set; }
    public Guid UserPermissionAssigneeId { get; private set; }
    public Guid? PartyId { get; private set; }
    public Guid? PartyPermissionAssigneeId { get; private set; }
    public IReadOnlySet<Guid?>? OrganizationUnitIds { get; private set; }
    public IReadOnlySet<Guid>? PostIds { get; private set; }
    public IReadOnlySet<Guid>? PostPermissionAssigneeIds { get; private set; }
    public IReadOnlySet<Guid>? RoleIds { get; private set; }
    public IReadOnlySet<Guid>? RolePermissionAssigneeIds { get; private set; }
    public IReadOnlySet<PermissionDto> Permissions { get; private set; }
        = new HashSet<PermissionDto>();

    public UserDataContext() { }
    /// <summary>
    /// ساخت یک Instance جدید کامل. برای BuildAsync و Seed.
    /// </summary>
    public static UserDataContext Create(
     Guid userId,
     Guid userPermissionAssigneeId,
     string? userName,
     Guid? partyId,
     Guid? partyPermissionAssigneeId,
     IEnumerable<Guid?>? organizationUnitIds,
     IEnumerable<Guid>? postIds,
     IEnumerable<Guid>? postPermissionAssigneeIds,
     IEnumerable<Guid>? roleIds,
     IEnumerable<Guid>? rolePermissionAssigneeIds,
     IEnumerable<PermissionDto>? permissions)   // ← nullable
    {
        return new UserDataContext
        {
            UserId = userId,
            UserName = userName,
            UserPermissionAssigneeId = userPermissionAssigneeId,
            PartyId = partyId,
            PartyPermissionAssigneeId = partyPermissionAssigneeId,
            OrganizationUnitIds = organizationUnitIds?.ToHashSet() ?? new HashSet<Guid?>(),
            PostIds = postIds?.ToHashSet() ?? new HashSet<Guid>(),
            PostPermissionAssigneeIds = postPermissionAssigneeIds?.ToHashSet() ?? new HashSet<Guid>(),
            RoleIds = roleIds?.ToHashSet() ?? new HashSet<Guid>(),
            RolePermissionAssigneeIds = rolePermissionAssigneeIds?.ToHashSet() ?? new HashSet<Guid>(),
            Permissions = (permissions ?? Enumerable.Empty<PermissionDto>()).ToHashSet()   // ← null-safe
        };
    }

    /// <summary>
    /// ساخت Instance خالی (کاربر ناشناس).
    /// </summary>
    public static UserDataContext Empty() => new()
    {
        Permissions = new HashSet<PermissionDto>()
    };

    /// <summary>
    /// کپی کامل State از یک Instance دیگر.
    /// جایگزین Reflection و SetValue.
    /// </summary>
    public void ApplyFrom(UserDataContext source)
    {
        ArgumentNullException.ThrowIfNull(source);

        UserId = source.UserId;
        UserName = source.UserName;
        UserPermissionAssigneeId = source.UserPermissionAssigneeId;
        PartyId = source.PartyId;
        PartyPermissionAssigneeId = source.PartyPermissionAssigneeId;
        OrganizationUnitIds = source.OrganizationUnitIds;
        PostIds = source.PostIds;
        PostPermissionAssigneeIds = source.PostPermissionAssigneeIds;
        RoleIds = source.RoleIds;
        RolePermissionAssigneeIds = source.RolePermissionAssigneeIds;
        Permissions = source.Permissions;
    }
}