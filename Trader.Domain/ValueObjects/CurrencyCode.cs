namespace Trader.Domain.ValueObjects;

/// <summary>
/// Whitelist ارزهای پشتیبانی‌شده برای TargetCurrency.
///
/// چرا Whitelist؟ بدون آن، یک Typo (مثل "USDD") در TradingRun.Create
/// ذخیره می‌شود و در اولین tick، StrategyEvaluationStage با
/// CurrencyRateNotFoundException کرش می‌کند — یک Runtime Failure
/// که می‌شد در Create-time جلوگیری کرد.
///
/// Phase 1: static whitelist. در Phase 2 اگر نیاز به ارزهای پویا شد،
/// این کلاس را به یک Entity با IsActive flag تبدیل می‌کنیم و اعتبارسنجی
/// به Application Layer منتقل می‌شود (با Query به CurrencyRate table).
/// </summary>
public static class CurrencyCode
{
    private static readonly HashSet<string> Supported = new(StringComparer.OrdinalIgnoreCase)
    {
        "IRR",  // Iranian Rial — ارز پایه‌ی بازار ایران
        "USD",  // US Dollar
        "EUR",  // Euro
        "GBP",  // British Pound
        "AED",  // UAE Dirham
        "TRY",  // Turkish Lira
        "CNY",  // Chinese Yuan
        "JPY",  // Japanese Yen
        "CHF",  // Swiss Franc
    };

    /// <summary>
    /// null/empty → null. غیر از آن → Normalize + اعتبارسنجی.
    /// برای استفاده در TradingRun.Create و UpdateSettings.
    /// </summary>
    public static string? NormalizeOrNull(string? code, string paramName)
    {
        if (string.IsNullOrWhiteSpace(code)) return null;
        return RequireValid(code, paramName);
    }

    /// <summary>
    /// Normalize + Whitelist check. اگر Invalid بود، ArgumentException.
    /// </summary>
    public static string RequireValid(string? code, string paramName)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Currency code is required.", paramName);

        var normalized = code.Trim().ToUpperInvariant();

        if (normalized.Length < 2 || normalized.Length > 10)
            throw new ArgumentException(
                $"Currency code '{code}' has invalid length. " +
                $"Expected 2-10 characters.", paramName);

        if (!Supported.Contains(normalized))
            throw new ArgumentException(
                $"Currency '{code}' is not supported. " +
                $"Supported currencies: {string.Join(", ", Supported.OrderBy(x => x))}. " +
                $"To add a new currency, extend CurrencyCode.Supported.", paramName);

        return normalized;
    }
}