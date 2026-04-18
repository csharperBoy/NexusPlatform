using Authorization.Application.Interfaces;
using Authorization.Application.Queries.Permissions;
using Authorization.Domain.Entities;
using Core.Shared.DTOs.Authorization;
using Core.Shared.Results;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Authorization.Application.Handlers.Queries.Permissions
{

    public class GetPermissionByIdQueryHandler
 : IRequestHandler<GetPermissionByIdQuery, Result<PermissionDto>>
    {
        private readonly IPermissionInternalService _PermissionService;
        private readonly ILogger<GetPermissionByIdQueryHandler> _logger;

        public GetPermissionByIdQueryHandler(
            IPermissionInternalService PermissionService,
            ILogger<GetPermissionByIdQueryHandler> logger)
        {
            _PermissionService = PermissionService;
            _logger = logger;
        }

        public async Task<Result<PermissionDto>> Handle(
            GetPermissionByIdQuery request,
            CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogDebug("Getting Permission by ID: {PermissionId}", request.Id);

                PermissionDto? result = await _PermissionService.GetById(request.Id);

                if (result == null)
                {
                    return Result<PermissionDto>.Fail($"Permission with ID {request.Id} not found");
                }

               
                return Result<PermissionDto>.Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get Permission by ID: {PermissionId}", request.Id);
                return Result<PermissionDto>.Fail(ex.Message);
            }
        }
    }

}
