using Trader.Application.Brokers;
using Trader.Application.Dtos;
using Trader.Application.Dtos.Account;
using Trader.Application.Dtos.Finance;
using Trader.Application.Dtos.MarketData;
using Trader.Application.Dtos.Order;
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

        /* ═══════════ ACCOUNT ═══════════ */
        Task<MoneyDto> GetCashBalanceAsync(BrokerSession session, CancellationToken ct = default);
        Task<List<PortfolioPerformanceDto>> GetPortfolioPerformanceAsync(BrokerSession session, CancellationToken ct = default);
        Task<ClientAppSettingDto> GetClientAppSettingAsync(BrokerSession session, CancellationToken ct = default);
        Task<bool> IsCreditCustomerAsync(BrokerSession session, CancellationToken ct = default);

        /* ═══════════ MARKET ═══════════ */
        Task<List<TradingTimeDto>> GetTradingTimesAsync(BrokerSession session, CancellationToken ct = default);

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
        /* ═══════════ MARKET LIVE ═══════════ */
        Task<List<IndustryPositiveNegativeDto>> GetIndustryPositiveNegativeAsync(
            BrokerSession session, CancellationToken ct = default);

        Task<List<TseIndexDto>> GetTseIndexAsync(
            BrokerSession session, CancellationToken ct = default);

        Task<List<MarketWatchCategoryDto>> GetMarketWatchAsync(
            BrokerSession session, CancellationToken ct = default);


        /* ═══════════ BATCH MARKET DATA ═══════════ */
        Task<List<BatchMarketDataItemDto>> GetBatchMarketDataAsync(
            BrokerSession session,
            IReadOnlyList<string> symbolIsins,
            CancellationToken ct = default);

        /* ═══════════ ORDER HISTORY ═══════════ */
        Task<PagedResult<OrderHistoryItemDto>> GetOrderHistoryAsync(
            BrokerSession session,
            OrderHistoryQuery query,
            CancellationToken ct = default);

        /* ═══════════ ORDER TRADES ═══════════ */
        /// <summary>
        /// لیست fillهای یک سفارش خاص.
        /// اگه سفارش اصلاً پر نشده باشه، لیست خالی برمی‌گرده.
        /// </summary>
        Task<List<OrderTradeDto>> GetOrderTradesAsync(
            BrokerSession session,
            string orderId,
            CancellationToken ct = default);

        /* ═══════════ FINANCE ═══════════ */
        /// <summary>
        /// لیست حساب‌های بانکی + موجودی قابل برداشت به تفکیک تاریخ و بانک.
        /// </summary>
        Task<PaymentAccountBalancesDto> GetPaymentAccountBalancesAsync(
            BrokerSession session,
            CancellationToken ct = default);

        /// <summary>
        /// درخواست برداشت وجه.
        /// ⚠️ عملیات غیرقابل برگشت — قبل از ارسال، از <see cref="ValidateWithdrawal"/> استفاده کن.
        /// </summary>
        Task<WithdrawalResultDto> RequestWithdrawalAsync(
            BrokerSession session,
            WithdrawalRequestDto request,
            CancellationToken ct = default);

        /* ═══════════ PAYMENT CANCEL ═══════════ */
        /// <summary>
        /// کنسل کردن یک درخواست برداشت.
        /// ⚠️ فقط درخواست‌هایی که <see cref="PaymentRequestDto.Cancellable"/> = true هستن کنسل می‌شن.
        ///
        /// ⚠️ response خالیه — بعد از این متد، با GetPaymentHistoryAsync تأیید کن
        /// که state به PaymentStateKind.CancelledByCustomer تغییر کرده.
        /// </summary>
        Task<PaymentCancelResultDto> CancelPaymentAsync(
            BrokerSession session,
            long paymentId,
            CancellationToken ct = default);
        /* ═══════════ ANALYSIS ═══════════ */
        Task<FundamentalAnalysisDto> GetFundamentalAnalysisAsync(
            BrokerSession session,
            string symbolIsin,
            CancellationToken ct = default);
    }
}