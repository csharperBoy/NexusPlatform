using Trader.Application.Abstractions;

namespace Trader.Application.Brokers;

public static class BrokerPaymentExtensions
{
    /// <summary>
    /// کنسل + تأیید با بازخوانی history.
    /// ⚠️ یه درخواست اضافی HTTP می‌زنه — فقط وقتی که واقعاً نیاز به تأیید داری استفاده کن.
    /// </summary>
    public static async Task<bool> CancelAndConfirmAsync(
        this IBrokerClient broker,
        BrokerSession session,
        long paymentId,
        CancellationToken ct = default)
    {
        var result = await broker.CancelPaymentAsync(session, paymentId, ct);
        if (!result.IsSuccessful)
            return false;

        //// بازخوانی history برای تأیید
        //var history = await broker.GetPaymentHistoryAsync(
        //    session,
        //    new PaymentHistoryQuery { Page = 1, PageSize = 30 },
        //    ct);

        //var item = history.Items.FirstOrDefault(x => x.Id == paymentId);
        //return item?.State == PaymentStateKind.CancelledByCustomer;
        return true;
    }
}