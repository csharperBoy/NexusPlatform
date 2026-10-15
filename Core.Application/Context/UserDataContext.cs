using Core.Shared.DTOs.Authorization;
namespace Core.Application.Context
{
    public sealed class UserDataContext
    {

        public Guid UserId { get; init; }
        public Guid UserPermissionAssigneeId { get; init; }
        public string UserName { get; init; }
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
