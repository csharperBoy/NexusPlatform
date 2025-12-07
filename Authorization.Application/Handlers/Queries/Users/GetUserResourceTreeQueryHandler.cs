using Authorization.Application.DTOs.Resource;
using Authorization.Application.Interfaces;
using Authorization.Application.Queries.Users;
using Core.Shared.Results;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Authorization.Application.Handlers.Queries.Users
{
    public class GetUserResourceTreeQueryHandler
        : IRequestHandler<GetUserResourceTreeQuery, Result<IReadOnlyList<ResourceTreeDto>>>
    {
        private readonly IResourceTreeBuilder _resourceTreeBuilder;
        private readonly ILogger<GetUserResourceTreeQueryHandler> _logger;

        public GetUserResourceTreeQueryHandler(
            IResourceTreeBuilder resourceTreeBuilder,
            ILogger<GetUserResourceTreeQueryHandler> logger)
        {
            _resourceTreeBuilder = resourceTreeBuilder;
            _logger = logger;
        }

        public async Task<Result<IReadOnlyList<ResourceTreeDto>>> Handle(
            GetUserResourceTreeQuery request,
            CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogDebug("Building resource tree for user {UserId}", request.UserId);

                var userTree = await _resourceTreeBuilder.BuildTreeForUserAsync(request.UserId);
                return Result<IReadOnlyList<ResourceTreeDto>>.Ok(userTree);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to build resource tree for user {UserId}", request.UserId);
                return Result<IReadOnlyList<ResourceTreeDto>>.Fail(ex.Message);
            }
        }
    }
}
