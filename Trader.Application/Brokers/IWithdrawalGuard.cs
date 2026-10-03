using Trader.Application.Dtos.Finance;

namespace Trader.Application.Brokers;

public interface IWithdrawalGuard
{
    /// <summary>
    /// بررسی می‌کنه آیا این درخواست تکراریه یا خیر.
    /// اگه تکراری باشه، true برمی‌گردونه.
    /// </summary>
    Task<bool> IsDuplicateAsync(
        string customerIsin,
        WithdrawalRequestDto request,
        CancellationToken ct = default);

    /// <summary>
    /// پس از ارسال موفق، درخواست رو ثبت می‌کنه.
    /// </summary>
    Task RecordAsync(
        string customerIsin,
        WithdrawalRequestDto request,
        WithdrawalResultDto result,
        CancellationToken ct = default);
}