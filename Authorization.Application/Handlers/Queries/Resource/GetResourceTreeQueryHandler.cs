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
        private readonly IResourceTreeBuilder _resourceTreeBuilder;
        private readonly ILogger<GetResourceTreeQueryHandler> _logger;

        public GetResourceTreeQueryHandler(
            IResourceTreeBuilder resourceTreeBuilder,
            ILogger<GetResourceTreeQueryHandler> logger)
        {
            _resourceTreeBuilder = resourceTreeBuilder;
            _logger = logger;
        }

        public async Task<Result<IReadOnlyList<ResourceTreeDto>>> Handle(
            GetResourceTreeQuery request,
            CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogDebug("Building resource tree with root: {RootId}", request.RootId?.ToString() ?? "null");

                var allTrees = await _resourceTreeBuilder.BuildTreeAsync();

                if (request.RootId.HasValue)
                {
                    var rootTree = FindTreeByRootId(allTrees, request.RootId.Value);
                    if (rootTree is null)
                    {
                        return Result<IReadOnlyList<ResourceTreeDto>>.Fail($"Root resource with ID {request.RootId} not found");
                    }

                    // Return the found subtree as a single-item list to match the signature
                    return Result<IReadOnlyList<ResourceTreeDto>>.Ok(new List<ResourceTreeDto> { rootTree });
                }
                else
                {
                    // Return the full forest (list of root trees)
                    return Result<IReadOnlyList<ResourceTreeDto>>.Ok(allTrees);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to build resource tree with root: {RootId}", request.RootId?.ToString() ?? "null");
                return Result<IReadOnlyList<ResourceTreeDto>>.Fail(ex.Message);
            }
        }

        private ResourceTreeDto? FindTreeByRootId(IReadOnlyList<ResourceTreeDto> trees, Guid rootId)
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
        }
    }
}
