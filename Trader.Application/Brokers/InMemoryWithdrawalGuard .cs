using System.Collections.Concurrent;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Trader.Application.Dtos.Finance;

namespace Trader.Application.Brokers;

/// <summary>
/// محافظ in-memory برای جلوگیری از برداشت تکراری.
///
/// ⚠️ محدودیت‌ها:
///  - فقط توی یه پروسه کار می‌کنه (اگه multi-instance باشی، کافی نیست)
///  - با restart پروسه، حافظه پاک می‌شه
///
/// برای production، باید با پیاده‌سازی DB-based جایگزین بشه.
/// </summary>
public class InMemoryWithdrawalGuard : IWithdrawalGuard
{
    private readonly IMemoryCache _cache;
    private readonly WithdrawalGuardOptions _options;

    // کلید: hash از (customerIsin + amount + performDate + bankAccountId)
    // مقدار: زمان ثبت

    public InMemoryWithdrawalGuard(
        IMemoryCache cache,
        IOptions<WithdrawalGuardOptions> options)
    {
        _cache = cache;
        _options = options.Value;
    }

    public Task<bool> IsDuplicateAsync(
        string customerIsin,
        WithdrawalRequestDto request,
        CancellationToken ct = default)
    {
        var key = BuildKey(customerIsin, request);
        var isDuplicate = _cache.TryGetValue(key, out _);
        return Task.FromResult(isDuplicate);
    }

    public Task RecordAsync(
        string customerIsin,
        WithdrawalRequestDto request,
        WithdrawalResultDto result,
        CancellationToken ct = default)
    {
        // فقط اگه موفق بود، ثبت می‌کنیم.
        // اگه رد شد، کاربر باید بتونه دوباره تلاش کنه.
        if (!result.IsSuccessful)
            return Task.CompletedTask;

        var key = BuildKey(customerIsin, request);
        _cache.Set(key, DateTimeOffset.UtcNow, _options.Window);
        return Task.CompletedTask;
    }

    private static string BuildKey(string customerIsin, WithdrawalRequestDto request)
        => $"wd:{customerIsin}:{request.BankAccountId}:{request.AmountRial}:{request.PerformDate:yyyy-MM-dd}";
}

public class WithdrawalGuardOptions
{
    /// <summary>
    /// بازه‌ی زمانی که یه درخواست مشابه، تکراری حساب می‌شه.
    /// پیش‌فرض: ۱۰ دقیقه.
    /// </summary>
    public TimeSpan Window { get; set; } = TimeSpan.FromMinutes(10);
}