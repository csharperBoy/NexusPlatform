using Core.Shared.DTOs.Authorization;
using Core.Shared.DTOs.HR;
using Core.Shared.Results;
using HR.Application.DTOs;
using HR.Application.Interfaces;
using HR.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HR.Application.Queries.Post
{
    public record GetPostListQuery(Guid? rootId = null)
        : IRequest<Result<IReadOnlyList<PostInfoDto>>>;

    public class GetPostListQueryHandler
        : IRequestHandler<GetPostListQuery, Result<IReadOnlyList<PostInfoDto>>>
    {
        private readonly IPostInternalService _orgChartInternalService;
        private readonly ILogger<GetPostListQueryHandler> _logger;

        public GetPostListQueryHandler(
            IPostInternalService orgChartInternalService,
        ILogger<GetPostListQueryHandler> logger)
        {
            _orgChartInternalService = orgChartInternalService;
            _logger = logger;
        }

        public async Task<Result<IReadOnlyList<PostInfoDto>>> Handle(
            GetPostListQuery request,
            CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogDebug("Getting Post List:");

                var posts = await _orgChartInternalService.GetPostListAsync();
                return Result<IReadOnlyList<PostInfoDto>>.Ok(posts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get Post List");
                return Result<IReadOnlyList<PostInfoDto>>.Fail(ex.Message);
            }
        }
    }
}
