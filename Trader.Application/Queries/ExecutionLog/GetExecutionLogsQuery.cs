using Core.Shared.Results;
using MediatR;
using Microsoft.Extensions.Logging;
using Trader.Application.Abstractions;
using Trader.Application.Dtos;

namespace Trader.Application.Queries.ExecutionLog
{
    public record GetExecutionLogsQuery(
        Guid? PlanId = null,
        string? Level = null,
        string? Search = null,
        int Skip = 0,
        int Take = 200)
        : IRequest<Result<ExecutionLogQueryResult>>;

    public class GetExecutionLogsQueryHandler
        : IRequestHandler<GetExecutionLogsQuery, Result<ExecutionLogQueryResult>>
    {
        private readonly IExecutionLogQueryService _logQueryService;
        private readonly ILogger<GetExecutionLogsQueryHandler> _logger;

        public GetExecutionLogsQueryHandler(
            IExecutionLogQueryService logQueryService,
            ILogger<GetExecutionLogsQueryHandler> logger)
        {
            _logQueryService = logQueryService;
            _logger = logger;
        }

        public async Task<Result<ExecutionLogQueryResult>> Handle(
            GetExecutionLogsQuery request,
            CancellationToken cancellationToken)
        {
            try
            {
                var take = request.Take <= 0 ? 200 : Math.Min(request.Take, 1000);
                var skip = Math.Max(request.Skip, 0);

                var result = await _logQueryService.GetLogsAsync(
                    request.PlanId,
                    request.Level,
                    request.Search,
                    skip,
                    take,
                    cancellationToken);

                return Result<ExecutionLogQueryResult>.Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get execution logs");
                return Result<ExecutionLogQueryResult>.Fail(ex.Message);
            }
        }
    }
}