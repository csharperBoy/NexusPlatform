using Core.Application.Abstractions.Authorization;
using Core.Domain.Enums;
using Core.Shared.DTOs.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Application.Context
{
    public sealed class UserDataContext
    {
        public Guid UserId { get; init; }
        public string UserName { get; init; }
        public Guid? PersonId { get; init; }
        public IReadOnlySet<Guid>? OrganizationUnitIds { get; init; } = new HashSet<Guid>();
        public IReadOnlySet<Guid>? PositionIds { get; init; } = new HashSet<Guid>();
        public IReadOnlySet<Guid>? RoleIds { get; init; } = new HashSet<Guid>();

        public IReadOnlySet<PermissionDto> Permissions { get; init; } = new HashSet<PermissionDto>();
        //public IReadOnlySet<PermissionDto> userPermissions { get; init; } = new HashSet<PermissionDto>();
        //public IReadOnlySet<PermissionDto> rolePermissions { get; init; } = new HashSet<PermissionDto>();
        //public IReadOnlySet<PermissionDto> personPermissions { get; init; } = new HashSet<PermissionDto>();
        //public IReadOnlySet<PermissionDto> positionPermissions { get; init; } = new HashSet<PermissionDto>();

    }


}
