using Core.Shared.Results;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Authorization.Application.Validator.Resource
{
    //public class UpdateRolePermissionsCommand : IRequest<Result<bool>>
    //{
    //    public Guid RoleId { get; set; }
    //    public List<Guid> PermissionIds { get; set; } = new();
    //}

    public record UpdateRolePermissionsCommand(Guid RoleId, List<Guid> PermissionIds)
      : IRequest<Result<bool>>;
}
