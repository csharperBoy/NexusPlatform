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
        : IRequestHandler<GetResourceTreeQuery, Result<ResourceTreeDto>>
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

        public async Task<Result<ResourceTreeDto>> Handle(
            GetResourceTreeQuery request,
            CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogDebug("Building resource tree with root: {RootId}", request.RootId?.ToString() ?? "null");

                // این متد نیاز به پیاده‌سازی جدید در IResourceTreeBuilder دارد
                // یا می‌توانیم از BuildTreeAsync استفاده کنیم و سپس درخت را فیلتر کنیم
                var allTrees = await _resourceTreeBuilder.BuildTreeAsync();

                if (request.RootId.HasValue)
                {
                    // پیدا کردن درخت با ریشه مشخص
                    var rootTree = FindTreeByRootId(allTrees, request.RootId.Value);
                    if (rootTree == null)
                    {
                        return Result<ResourceTreeDto>.Fail($"Root resource with ID {request.RootId} not found");
                    }
                    return Result<ResourceTreeDto>.Ok(rootTree);
                }
                else
                {
                    // اگر RootId مشخص نشده، کل درخت را برمی‌گردانیم
                    // این نیاز به یک ResourceTreeDto ریشه مجازی دارد
                    var virtualRoot = new ResourceTreeDto
                    {
                        Id = Guid.Empty,
                        Key = "root",
                        Name = "All Resources",
                        Children = allTrees.ToList()
                    };
                    return Result<ResourceTreeDto>.Ok(virtualRoot);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to build resource tree with root: {RootId}", request.RootId?.ToString() ?? "null");
                return Result<ResourceTreeDto>.Fail(ex.Message);
            }
        }

        private ResourceTreeDto FindTreeByRootId(IReadOnlyList<ResourceTreeDto> trees, Guid rootId)
        {
            foreach (var tree in trees)
            {
                if (tree.Id == rootId)
                {
                    return tree;
                }

                // جستجو در فرزندان
                var foundInChildren = FindTreeByRootId(tree.Children, rootId);
                if (foundInChildren != null)
                {
                    return foundInChildren;
                }
            }
            return null;
        }
    }
}
