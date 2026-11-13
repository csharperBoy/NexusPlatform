using Core.Shared.Results;
using MediatR;
using Microsoft.Extensions.Logging;
using Trader.Application.Abstractions.Dashboard;
using Trader.Application.Dtos.Dashboard;

namespace Trader.Application.Queries.Dashboard;

public record GetDashboardSummaryQuery : IRequest<Result<DashboardSummaryView>>;

public class GetDashboardSummaryQueryHandler
    : IRequestHandler<GetDashboardSummaryQuery, Result<DashboardSummaryView>>
{
    private readonly IDashboardQueryService _service;
    private readonly ILogger<GetDashboardSummaryQueryHandler> _logger;

    public GetDashboardSummaryQueryHandler(
        IDashboardQueryService service,
        ILogger<GetDashboardSummaryQueryHandler> logger)
    {
        _service = service;
        _logger = logger;
    }

    public async Task<Result<DashboardSummaryView>> Handle(
        GetDashboardSummaryQuery request, CancellationToken ct)
    {
        try
        {
            var summary = await _service.GetSummaryAsync(ct);
            return Result<DashboardSummaryView>.Ok(summary);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to build dashboard summary");
            return Result<DashboardSummaryView>.Fail(ex.Message);
        }
    }
}