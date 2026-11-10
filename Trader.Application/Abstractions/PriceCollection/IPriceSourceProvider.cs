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
    /// دریافت رکوردها. اگر sinceEffectiveAt != null، فقط رکوردهای جدیدتر برگردانده می‌شوند (short-circuit).
    /// maxPages=1 → فقط صفحه اول (Recurring). maxPages=int.MaxValue → تا boundary (Backfill).
    /// </summary>
    Task<PriceFetchResult> FetchAsync(
        PriceSource source,
        DateTime? sinceEffectiveAt,
        int maxPages,
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