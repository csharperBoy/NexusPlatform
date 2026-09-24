using Core.Application.Results;
using Core.Shared.Results;
using MediatR;
using Microsoft.Extensions.Logging;
using Trader.Application.Abstractions;
using Trader.Application.Dtos;

namespace Trader.Application.Queries.SchedulePlan
{
    public record GetSchedulePlanByIdQuery(Guid Id)
        : IRequest<Result<SchedulePlanInfoView>>;

    public class GetSchedulePlanByIdQueryHandler
        : IRequestHandler<GetSchedulePlanByIdQuery, Result<SchedulePlanInfoView>>
    {
        private readonly ISchedulePlanQueryService _planQueryService;
        private readonly ILogger<GetSchedulePlanByIdQueryHandler> _logger;

        public GetSchedulePlanByIdQueryHandler(
            ISchedulePlanQueryService planQueryService,
            ILogger<GetSchedulePlanByIdQueryHandler> logger)
        {
            _planQueryService = planQueryService;
            _logger = logger;
        }

        public async Task<Result<SchedulePlanInfoView>> Handle(
            GetSchedulePlanByIdQuery request,
            CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogDebug(
                    "Getting Schedule Plan by Id: {Id}", request.Id);

                var plan = await _planQueryService.GetSchedulePlanByIdAsync(request.Id);

                if (plan is null)
                    return Result<SchedulePlanInfoView>.Fail("SchedulePlan not found.");

                return Result<SchedulePlanInfoView>.Ok(plan);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to get Schedule Plan by Id: {Id}", request.Id);
                return Result<SchedulePlanInfoView>.Fail(ex.Message);
            }
        }
    }
}