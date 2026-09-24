using Trader.Application.Brokers;
using Trader.Application.Dtos;
using Trader.Domain.Enums;

namespace Trader.Application.Abstractions
{
    /// <summary>
    /// قرارداد عمومی برای هر سامانه‌ی کارگزاری.
    /// </summary>
    public interface IBrokerClient
    {
        BrokerType BrokerType { get; }

        /// <summary>
        /// لاگین کامل به سامانه. تمام مراحل (OIDC, activation, ...) داخل همین متد.
        /// </summary>
        Task<BrokerSession> LoginAsync(
            string username,
            string password,
            CancellationToken ct = default);

        /// <summary>
        /// اندازه‌گیری تاخیر یک‌طرفه به سرور کارگزاری (میلی‌ثانیه).
        /// </summary>
        Task<long> MeasureLatencyAsync(
            BrokerSession session,
            CancellationToken ct = default);

        /// <summary>
        /// اطلاعات لحظه‌ای نماد.
        /// </summary>
        Task<SymbolMarketDataDto> GetSymbolInfoAsync(
            BrokerSession session,
            string symbolName,
            CancellationToken ct = default);

        /// <summary>
        /// ارسال سفارش خرید.
        /// </summary>
        Task<BrokerOrderResultDto> SendBuyOrderAsync(
            BrokerSession session,
            string symbolName,
            long price,
            long quantity,
            CancellationToken ct = default);

        /// <summary>
        /// ارسال سفارش فروش.
        /// </summary>
        Task<BrokerOrderResultDto> SendSellOrderAsync(
            BrokerSession session,
            string symbolName,
            long price,
            long quantity,
            CancellationToken ct = default);

        /// <summary>
        /// Serialize کردن session برای ذخیره در DB.
        /// </summary>
        string SerializeSession(BrokerSession session);

        /// <summary>
        /// Deserialize کردن session از DB.
        /// </summary>
        BrokerSession DeserializeSession(string json);
    }
}