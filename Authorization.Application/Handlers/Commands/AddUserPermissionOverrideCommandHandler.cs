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
    public class AddUserPermissionOverrideCommandHandler
          : IRequestHandler<AddUserPermissionOverrideCommand, Result<bool>>
    {
        private readonly IPermissionService _service;

        public AddUserPermissionOverrideCommandHandler(IPermissionService service)
        {
            _service = service;
        }

        public async Task<Result<bool>> Handle(AddUserPermissionOverrideCommand cmd, CancellationToken ct)
        {
            await _service.AddUserOverrideAsync(cmd.UserId, cmd.PermissionId, cmd.Granted, cmd.Scope, ct);
            return Result<bool>.Success(true);
        }
    }
}
