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
    public class DeleteUserPermissionOverrideCommandHandler
        : IRequestHandler<DeleteUserPermissionOverrideCommand, Result<bool>>
    {
        private readonly IPermissionService _service;

        public DeleteUserPermissionOverrideCommandHandler(IPermissionService service)
        {
            _service = service;
        }

        public async Task<Result<bool>> Handle(DeleteUserPermissionOverrideCommand cmd, CancellationToken ct)
        {
            await _service.RemoveUserOverrideAsync(cmd.UserId, cmd.OverrideId, ct);
            return Result<bool>.Success(true);
        }
    }
}
