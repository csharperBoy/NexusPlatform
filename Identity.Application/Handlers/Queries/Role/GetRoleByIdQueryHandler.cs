using Core.Shared.DTOs.Authorization;
using Core.Shared.Results;
using Identity.Application.DTOs;
using Identity.Application.Interfaces;
using Identity.Application.Queries.Role;
using Identity.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Identity.Application.Handlers.Queries.Role
{
    public class GetRoleByIdQueryHandler
    : IRequestHandler<GetRoleByIdQuery, Result<RoleDto>>
    {
        private readonly IRoleInternalService _RoleService;
        private readonly ILogger<GetRoleByIdQueryHandler> _logger;

        public GetRoleByIdQueryHandler(
            IRoleInternalService RoleService,
            ILogger<GetRoleByIdQueryHandler> logger)
        {
            _RoleService = RoleService;
            _logger = logger;
        }

        public async Task<Result<RoleDto>> Handle(
            GetRoleByIdQuery request,
            CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogDebug("Getting Role by ID: {RoleId}", request.Id);

                ApplicationRole? Role = await _RoleService.GetById(request.Id);

                if (Role == null)
                {
                    return Result<RoleDto>.Fail($"Role with ID {request.Id} not found");
                }

                RoleDto result = new RoleDto()
                {
                   Description = Role.Description,
                   Id = Role.Id,
                   OrderNum = Role.OrderNum,
                   Name = Role.Name,
                };
                return Result<RoleDto>.Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get Role by ID: {RoleId}", request.Id);
                return Result<RoleDto>.Fail(ex.Message);
            }
        }
    }

}
