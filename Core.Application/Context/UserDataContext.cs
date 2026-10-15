using Core.Shared.DTOs.Authorization;
namespace Core.Application.Context
{
    public sealed class UserDataContext
    {

        /// <summary>
        /// تنظیم دستی کاربر — برای BackgroundService و سناریوهای غیر-HTTP.
        /// </summary>
        public void SetUser(Guid userId, string? userName)
        {
            UserId = userId;
            UserName = userName;
        }
        public Guid UserId { get; private set; }
        public string UserName { get; private set; }
        public Guid UserPermissionAssigneeId { get; init; }
        public Guid? PartyId { get; init; }
        public Guid? PartyPermissionAssigneeId { get; init; }
        public IReadOnlySet<Guid?>? OrganizationUnitIds { get; init; } = new HashSet<Guid?>();
        public IReadOnlySet<Guid>? PostIds { get; init; } = new HashSet<Guid>();
        public IReadOnlySet<Guid>? PostPermissionAssigneeIds { get; init; } = new HashSet<Guid>();
        public IReadOnlySet<Guid>? RoleIds { get; init; } = new HashSet<Guid>();
        public IReadOnlySet<Guid>? RolePermissionAssigneeIds { get; init; } = new HashSet<Guid>();
        public IReadOnlySet<PermissionDto> Permissions { get; init; } = new HashSet<PermissionDto>();
           }


}
