using Core.Application.Results;
using Core.Shared.Results;
using MediatR;
using Microsoft.Extensions.Logging;
using Trader.Application.Abstractions;
using Trader.Application.Dtos;

namespace Trader.Application.Queries.SchedulePlan
{
    public record GetSchedulePlanListQuery(
        string? DateFrom = null,
        string? DateTo = null,
        bool? EnabledOnly = null)
        : IRequest<Result<IReadOnlyList<SchedulePlanInfoView>>>;

    public class GetSchedulePlanListQueryHandler
        : IRequestHandler<GetSchedulePlanListQuery, Result<IReadOnlyList<SchedulePlanInfoView>>>
    {
        private readonly ISchedulePlanQueryService _planQueryService;
        private readonly ILogger<GetSchedulePlanListQueryHandler> _logger;

        public GetSchedulePlanListQueryHandler(
            ISchedulePlanQueryService planQueryService,
            ILogger<GetSchedulePlanListQueryHandler> logger)
        {
            _planQueryService = planQueryService;
            _logger = logger;
        }

        public async Task<Result<IReadOnlyList<SchedulePlanInfoView>>> Handle(
            GetSchedulePlanListQuery request,
            CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogDebug("Getting Schedule Plan List");

                var plans = await _planQueryService.GetSchedulePlanListAsync();
                return Result<IReadOnlyList<SchedulePlanInfoView>>.Ok(plans);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get Schedule Plan List");
                return Result<IReadOnlyList<SchedulePlanInfoView>>.Fail(ex.Message);
            }
        }
    }
}