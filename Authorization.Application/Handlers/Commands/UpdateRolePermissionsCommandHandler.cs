using Authorization.Application.Commands;
using Authorization.Application.Interfaces;
using Core.Shared.Results;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Authorization.Application.Handlers.Commands
{
    public class UpdateRolePermissionsCommandHandler
         : IRequestHandler<UpdateRolePermissionsCommand, Result<bool>>
    {
        private readonly IPermissionService _service;

        public UpdateRolePermissionsCommandHandler(IPermissionService service)
        {
            _service = service;
        }

        public async Task<Result<bool>> Handle(UpdateRolePermissionsCommand cmd, CancellationToken ct)
        {
            await _service.UpdateRolePermissionsAsync(cmd.RoleId, cmd.PermissionIds, ct);
            return Result<bool>.Success(true);
        }
    }
}
