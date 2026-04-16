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
    public class UpdateRoleCommandHandler : IRequestHandler<UpdateRoleCommand, Result<bool>>
    {
        private readonly IRoleInternalService _RoleService;
        private readonly ILogger<UpdateRoleCommandHandler> _logger;

        public UpdateRoleCommandHandler(
            IRoleInternalService RoleService,
            ILogger<UpdateRoleCommandHandler> logger)
        {
            _RoleService = RoleService;
            _logger = logger;
        }

        public async Task<Result<bool>> Handle(UpdateRoleCommand request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Updating Role: {RoleId}", request.Id);

                await _RoleService.UpdateRoleAsync(request);

                _logger.LogInformation("Role updated successfully: {RoleId}", request.Id);

                return Result<bool>.Ok(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update Role: {RoleId}", request.Id);
                return Result<bool>.Fail(ex.Message);
            }
        }
    }

}
