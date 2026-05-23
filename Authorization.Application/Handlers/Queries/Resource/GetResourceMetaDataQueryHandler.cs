using Authorization.Application.DTOs.Resource;
using Authorization.Application.Interfaces;
using Authorization.Application.Interfaces.Service;
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
    public class GetResourceMetaDataQueryHandler
      : IRequestHandler<GetResourceMetaDataQuery, Result<List<ResourceMetadataDto>>>
    {
        private readonly IResourceMetadataProvider _metaDataProvider;
        private readonly ILogger<GetResourceMetaDataQueryHandler> _logger;

        public GetResourceMetaDataQueryHandler(
            IResourceMetadataProvider metaDataProvider,
            ILogger<GetResourceMetaDataQueryHandler> logger)
        {
            _metaDataProvider = metaDataProvider;
            _logger = logger;
        }

        public async  Task<Result<List<ResourceMetadataDto>>> Handle(
            GetResourceMetaDataQuery request,
            CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogDebug("Getting resource meta Data");

                List<ResourceMetadataDto> metaData =  _metaDataProvider.Resources.Where(r=>string.IsNullOrEmpty( request.resourceKey) ||  r.ResourceKey == request.resourceKey).ToList();

                if (metaData == null || metaData.Count() == 0 )
                {
                    return Result<List<ResourceMetadataDto>>.Fail($"Resource with Key {request.resourceKey} not found");
                }

                return Result<List<ResourceMetadataDto>>.Ok(metaData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get resource by resourceKey: {resourceKey}", request.resourceKey);
                return Result<List<ResourceMetadataDto>>.Fail(ex.Message);
            }
        }
    }

}
