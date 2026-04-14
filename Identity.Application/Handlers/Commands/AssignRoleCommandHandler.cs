using Core.Shared.Results;
using Identity.Application.Commands;
using Identity.Application.Interfaces;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Identity.Application.Handlers.Commands
{
    public class AssignRoleCommandHandler : IRequestHandler<AssignRoleCommand, Result>
    {
        private readonly IRoleInternalService _roleService;
        public AssignRoleCommandHandler(IRoleInternalService roleService)
            => _roleService = roleService;

        public async Task<Result> Handle(AssignRoleCommand request, CancellationToken ct)
        {
            return await _roleService.AssignRoleToUserAsync(request.UserId, request.RoleName);
        }
    }
}
