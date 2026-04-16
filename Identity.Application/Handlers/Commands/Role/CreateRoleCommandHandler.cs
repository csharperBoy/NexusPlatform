using Core.Shared.Results;
using Identity.Application.Commands.Role;
using Identity.Application.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Identity.Application.Handlers.Commands.Role
{
    public class CreateRoleCommandHandler : IRequestHandler<CreateRoleCommand, Result<Guid>>
    {
        private readonly IRoleInternalService _roleService;
        private readonly ILogger<CreateRoleCommandHandler> _logger;

        public CreateRoleCommandHandler(
            IRoleInternalService roleService,
            ILogger<CreateRoleCommandHandler> logger)
        {
            _roleService = roleService;
            _logger = logger;
        }

        public async Task<Result<Guid>> Handle(CreateRoleCommand request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation(
                    "Creating role: ({RoleName})",
                  request.Name);

                Guid roleId = await _roleService.CreateRoleAsync(request);

                _logger.LogInformation(
                    "Role created successfully: {RoleId} ({RoleName})",
                    roleId, request.Name);

                return Result<Guid>.Ok(roleId);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to create role:  ({RoleName})",
                   request.Name);

                return Result<Guid>.Fail(ex.Message);
            }
        }
    }

}
