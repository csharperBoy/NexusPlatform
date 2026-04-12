using Authorization.Application.DTOs.Resource;
using Authorization.Application.Interfaces;
using Authorization.Application.Queries.Resource;
using Core.Shared.DTOs.Authorization;
using Core.Shared.Results;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Authorization.Application.Handlers.Queries.Resource
{
    public class GetResourceByIdQueryHandler
        : IRequestHandler<GetResourceByIdQuery, Result<ResourceDto>>
    {
        private readonly IResourceInternalService _resourceService;
        private readonly ILogger<GetResourceByIdQueryHandler> _logger;

        public GetResourceByIdQueryHandler(
            IResourceInternalService resourceService,
            ILogger<GetResourceByIdQueryHandler> logger)
        {
            _resourceService = resourceService;
            _logger = logger;
        }

        public async Task<Result<ResourceDto>> Handle(
            GetResourceByIdQuery request,
            CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogDebug("Getting resource by ID: {ResourceId}", request.Id);

                var resource = await _resourceService.GetById(request.Id);
                
                ResourceDto result = resource.ToDto(resource);
                
                if (resource == null)
                {
                    return Result<ResourceDto>.Fail($"Resource with ID {request.Id} not found");
                }

                return Result<ResourceDto>.Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get resource by ID: {ResourceId}", request.Id);
                return Result<ResourceDto>.Fail(ex.Message);
            }
        }
    }
}
