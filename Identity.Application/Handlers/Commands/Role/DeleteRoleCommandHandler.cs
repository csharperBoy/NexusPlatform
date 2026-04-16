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

    public class DeleteRoleCommandHandler : IRequestHandler<DeleteRoleCommand, Result<bool>>
    {
        private readonly IRoleInternalService _roleService;
        private readonly ILogger<DeleteRoleCommandHandler> _logger;

        public DeleteRoleCommandHandler(
            IRoleInternalService roleService,
            ILogger<DeleteRoleCommandHandler> logger)
        {
            _roleService = roleService;
            _logger = logger;
        }

        public async Task<Result<bool>> Handle(DeleteRoleCommand request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Deleting role: {RoleId}", request.Id);

                await _roleService.DeleteRoleAsync(request.Id);

                _logger.LogInformation("Role deleted successfully: {RoleId}", request.Id);

                return Result<bool>.Ok(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete role: {RoleId}", request.Id);
                return Result<bool>.Fail(ex.Message);
            }
        }
    }

}
