using Authorization.Application.DTOs.Resource;
using Authorization.Application.Interfaces;
using Authorization.Application.Queries.Resource;
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
    public class GetResourceByKeyQueryHandler
         : IRequestHandler<GetResourceByKeyQuery, Result<ResourceDto>>
    {
        private readonly IResourceService _resourceService;
        private readonly ILogger<GetResourceByKeyQueryHandler> _logger;

        public GetResourceByKeyQueryHandler(
            IResourceService resourceService,
            ILogger<GetResourceByKeyQueryHandler> logger)
        {
            _resourceService = resourceService;
            _logger = logger;
        }

        public async Task<Result<ResourceDto>> Handle(
            GetResourceByKeyQuery request,
            CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogDebug("Getting resource by key: {Key}", request.Key);

                var resource = await _resourceService.GetResourceByKeyAsync(request.Key);

                if (resource == null)
                {
                    return Result<ResourceDto>.Fail($"Resource with key '{request.Key}' not found");
                }

                return Result<ResourceDto>.Ok(resource);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get resource by key: {Key}", request.Key);
                return Result<ResourceDto>.Fail(ex.Message);
            }
        }
    }
}
