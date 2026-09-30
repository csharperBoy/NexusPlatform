using Trader.Application.Brokers;
using Trader.Application.Dtos;
using Trader.Application.Dtos.MarketData;
using Trader.Domain.Enums;

namespace Trader.Application.Abstractions
{
    /// <summary>
    /// قرارداد عمومی برای هر سامانه‌ی کارگزاری.
    /// </summary>
    public interface IBrokerClient
    {
        BrokerType BrokerType { get; }

        /* ═══════════ AUTH ═══════════ */
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
        Task<BrokerTimeMeasurement> MeasureLatencyAsync(
                         BrokerSession session,
                         CancellationToken ct = default);

        /// <summary>
        /// Serialize کردن session برای ذخیره در DB.
        /// </summary>
        string SerializeSession(BrokerSession session);

        /// <summary>
        /// Deserialize کردن session از DB.
        /// </summary>
        BrokerSession DeserializeSession(string json);

        /* ═══════════ ORDER ═══════════ */
        HttpRequestMessage BuildOrderRequest(
            BrokerSession session,
            string symbolIsin,
            long price,
            long quantity,
            int side);
        /// <summary>
        /// ارسال سفارش خرید.
        /// </summary>
        Task<BrokerOrderResultDto> SendBuyOrderAsync(
            BrokerSession session,
            string symbolIsin,
            long price,
            long quantity,
            CancellationToken ct = default);

        /// <summary>
        /// ارسال سفارش فروش.
        /// </summary>
        Task<BrokerOrderResultDto> SendSellOrderAsync(
            BrokerSession session,
            string symbolIsin,
            long price,
            long quantity,
            CancellationToken ct = default);

        Task<BrokerOrderResultDto> SendOrderWithRequestAsync(
    BrokerSession session,
    HttpRequestMessage request,
    string symbolIsin,
    CancellationToken ct = default);

        /* ═══════════ SYMBOL ═══════════ */
        /// <summary>
        /// اطلاعات لحظه‌ای نماد.
        /// </summary>
        Task<SymbolMarketDataDto> GetSymbolInfoAsync(
            BrokerSession session,
           string symbolIsin,
            CancellationToken ct = default);

        Task<ReturnChartDto> GetReturnChartAsync(
            BrokerSession session, string symbolIsin, CancellationToken ct = default);

        /* ═══════════ CANDLES ═══════════ */
        Task<List<CandleDto>> GetCandlesAsync(
            BrokerSession session,
            string symbolIsin,
            int days = 1,
            int intervalMinutes = 1,
            CancellationToken ct = default);

        /* ═══════════ ANALYSIS ═══════════ */
        Task<TechnicalAnalysisDto> GetTechnicalAnalysisAsync(
            BrokerSession session, string symbolIsin, CancellationToken ct = default);

        Task<IndInstTradeDto> GetIndInstTradeAsync(
            BrokerSession session, string symbolIsin, CancellationToken ct = default);

        Task<IndInstAnalysisDto> GetIndInstAnalysisAsync(
            BrokerSession session, string symbolIsin, CancellationToken ct = default);

        Task<List<IndTradingTrendDto>> GetIndTradingTrendAsync(
            BrokerSession session, string symbolIsin, CancellationToken ct = default);

        /* ═══════════ MARKET SHEET ═══════════ */
        Task<MarketSheetSumDto> GetMarketSheetSumAsync(
            BrokerSession session, string symbolIsin, CancellationToken ct = default);

    }
}