namespace Trader.Application.Dtos.PriceCollection;

public sealed record CreatePriceCollectionJobModel(
    Guid PriceSourceId,
    int Mode,                        // 0=Recurring, 1=Backfill
    string? RecurrencePatternJson);  // برای Recurring الزامی