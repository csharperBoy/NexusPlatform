namespace Trader.Infrastructure.PriceCollection.Tgju;

/// <summary>
/// شکل ConfigJson یک PriceSource با Type=Tgju.
/// در ConfigJson ست می‌شود؛ در منطق Persist ازش خوانده می‌شود.
/// </summary>
public sealed record TgjuConfigModel(
    string Indicator,
    decimal? MinRate = null,
    decimal? MaxRate = null,
    decimal? MaxDailyChangePct = null);