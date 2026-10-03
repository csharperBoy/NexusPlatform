using Trader.Application.Abstractions;
using Trader.Application.Dtos.Finance;

namespace Trader.Application.Brokers;

public interface ISafeWithdrawalService
{
    Task<WithdrawalResultDto> WithdrawAsync(
        IBrokerClient broker,
        BrokerSession session,
        WithdrawalRequestDto request,
        CancellationToken ct = default);
}

public class SafeWithdrawalService : ISafeWithdrawalService
{
    private readonly IWithdrawalGuard _guard;

    public SafeWithdrawalService(IWithdrawalGuard guard)
    {
        _guard = guard;
    }

    public async Task<WithdrawalResultDto> WithdrawAsync(
        IBrokerClient broker,
        BrokerSession session,
        WithdrawalRequestDto request,
        CancellationToken ct = default)
    {
        // ۱. هویت مشتری رو از session بگیر
        // ⚠️ این متد باید توی IBrokerClient باشه — احتمالاً CustomerIsin توی session هست
        var customerIsin = GetCustomerIsin(session);

        // ۲. چک تکراری
        if (await _guard.IsDuplicateAsync(customerIsin, request, ct))
            throw new DuplicateWithdrawalException(
                $"Duplicate withdrawal detected: bank={request.BankAccountId}, " +
                $"amount={request.AmountRial}, date={request.PerformDate}");

        // ۳. اعتبارسنجی (اگه balance داری، validator رو هم صدا بزن)
        // ...

        // ۴. ارسال
        var result = await broker.RequestWithdrawalAsync(session, request, ct);

        // ۵. ثبت برای idempotency
        await _guard.RecordAsync(customerIsin, request, result, ct);

        return result;
    }

    private static string GetCustomerIsin(BrokerSession session)
    {
        // ⚠️ این متد رو باید بسته به ساختار session پیاده کنی
        // فرض: session یه CustomerIsin داره
        return session switch
        {
            EasyTraderSession et => et.CustomerIsin ?? throw new InvalidOperationException(
                "CustomerIsin not available in session"),
            _ => throw new NotSupportedException("Unknown session type")
        };
    }
}

public class DuplicateWithdrawalException : Exception
{
    public DuplicateWithdrawalException(string message) : base(message) { }
}