using Microsoft.Extensions.Logging;
using Trader.Application.Abstractions;
using Trader.Application.Dtos.TradingSchedules;
using Trader.Domain.Entities;
using Trader.Infrastructure.Data;

namespace Trader.Infrastructure.Services;

public sealed class TradingScheduleService : ITradingScheduleQueryService
{
    private readonly ITraderUnitOfWork<TraderDbContext> _uow;
    private readonly ILogger<TradingScheduleService> _logger;

    public TradingScheduleService(
        ITraderUnitOfWork<TraderDbContext> uow,
        ILogger<TradingScheduleService> logger)
    {
        _uow = uow;
        _logger = logger;
    }

    public async Task<IReadOnlyList<TradingScheduleInfoView>> GetListAsync(
        CancellationToken ct = default)
    {
        var schedules = await _uow.TradingSchedules.GetAllAsync();

        return schedules
            .OrderBy(s => s.SessionStart)
            .ThenBy(s => s.Code)
            .Select(MapToView)
            .ToList();
    }

    public async Task<IReadOnlyList<TradingScheduleSelectionView>> GetSelectionListAsync(
        CancellationToken ct = default)
    {
        var schedules = await _uow.TradingSchedules.GetAllAsync();

        return schedules
            .Where(s => s.IsActive)
            .OrderBy(s => s.SessionStart)
            .ThenBy(s => s.Code)
            .Select(s => new TradingScheduleSelectionView(
                Value: s.Id.ToString(),
                Label: s.Name,
                Display: $"{s.Name} ({s.SessionStart:HH:mm}–{s.SessionEnd:HH:mm})"))
            .ToList();
    }

    private static TradingScheduleInfoView MapToView(TradingSchedule s)
    {
        return new TradingScheduleInfoView(
            s.Id,
            s.Code,
            s.Name,
            s.Description,
            s.PreMarketStart.ToString("HH:mm"),
            s.SessionStart.ToString("HH:mm"),
            s.SessionEnd.ToString("HH:mm"),
            s.AfternoonStart?.ToString("HH:mm"),
            s.AfternoonEnd?.ToString("HH:mm"),
            s.AfterHoursEnd?.ToString("HH:mm"),
            s.IsActive);
    }
}