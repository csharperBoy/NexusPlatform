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
    public class GetResourceTreeQueryHandler
      : IRequestHandler<GetResourceTreeQuery, Result<IReadOnlyList<ResourceTreeDto>>>
    {
        private readonly IResourceInternalService _resourceService;
        private readonly ILogger<GetResourceTreeQueryHandler> _logger;

        public GetResourceTreeQueryHandler(IResourceInternalService resourceService,
            ILogger<GetResourceTreeQueryHandler> logger)
        {
            _logger = logger;
            _resourceService = resourceService;
        }

        public async Task<Result<IReadOnlyList<ResourceTreeDto>>> Handle(
            GetResourceTreeQuery request,
            CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogDebug("Building resource tree with root: {RootId}", request.RootId?.ToString() ?? "null");

                var allTrees = await _resourceService.GetByTreeStructure(request.RootId);

                return Result<IReadOnlyList<ResourceTreeDto>>.Ok(allTrees);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to build resource tree with root: {RootId}", request.RootId?.ToString() ?? "null");
                return Result<IReadOnlyList<ResourceTreeDto>>.Fail(ex.Message);
            }
        }

        /*private ResourceTreeDto? FindTreeByRootId(IReadOnlyList<ResourceTreeDto> trees, Guid rootId)
        {
            foreach (var tree in trees)
            {
                if (tree.Id == rootId)
                    return tree;

                if (tree.Children is { Count: > 0 })
                {
                    var foundInChildren = FindTreeByRootId(tree.Children, rootId);
                    if (foundInChildren != null)
                        return foundInChildren;
                }
            }

            return null;
        }*/
    }
}
