using Trader.Application.Dtos;

namespace Trader.Application.Abstractions
{
    public interface IExecutionLogQueryService
    {
        Task<ExecutionLogQueryResult> GetLogsAsync(
            Guid? planId,
            string? level,
            string? search,
            int skip,
            int take,
            CancellationToken ct = default);
    }
}