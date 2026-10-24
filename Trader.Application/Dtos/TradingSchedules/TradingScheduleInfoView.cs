namespace Trader.Application.Dtos.TradingSchedules;

public sealed record TradingScheduleInfoView(
    Guid Id,
    string Code,
    string Name,
    string? Description,
    string PreMarketStart,
    string SessionStart,
    string SessionEnd,
    string? AfternoonStart,
    string? AfternoonEnd,
    string? AfterHoursEnd,
    bool IsActive);

public sealed record TradingScheduleSelectionView(
    string Value,
    string Label,
    string Display);