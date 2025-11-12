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
        private readonly IAuthorizationService _authorizationService;
        public AssignRoleCommandHandler(IAuthorizationService authorizationService)
            => _authorizationService = authorizationService;

        public async Task<Result> Handle(AssignRoleCommand request, CancellationToken ct)
        {
            return await _authorizationService.AssignRoleToUserAsync(request.UserId, request.RoleName);
        }
    }
}
