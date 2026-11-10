using Trader.Domain.Entities;
using Trader.Domain.Enums;

namespace Trader.Application.Abstractions.PriceCollection;

/// <summary>
/// قرارداد یک Provider نرخ. Tgju — Phase B1. Telegram — Phase B2.
/// </summary>
public interface IPriceSourceProvider
{
    PriceSourceType SourceType { get; }

    /// <summary>
    /// fetchAllPages = true → Backfill — همه صفحات از start=0 تا انتها
    ///fetchAllPages = false → Recurring — فقط آخرین صفحه(پروب recordsTotal + fetch از start = total - PageSize)
    /// </summary>
    Task<PriceFetchResult> FetchAsync(
    PriceSource source,
    DateTime? sinceEffectiveAt,
    bool fetchAllPages,          // ← جایگزین int maxPages
    CancellationToken ct = default);
}

public sealed record PriceFetchResult(
    IReadOnlyList<PricePoint> Points,
    int PagesFetched,
    bool HitBoundary);   // true اگر Boundary (maxDb) دیده شد

/// <summary>یک رکورد نرخ — Output استاندارد Providerها.</summary>
public sealed record PricePoint(
    decimal Rate,
    DateTime EffectiveAt,
    string Source);