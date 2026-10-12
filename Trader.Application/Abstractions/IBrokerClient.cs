using Trader.Application.Brokers;
using Trader.Application.Dtos;
using Trader.Application.Dtos.Account;
using Trader.Application.Dtos.Finance;
using Trader.Application.Dtos.MarketData;
using Trader.Application.Dtos.Order;
using Trader.Domain.Enums;
using Trader.ExternalApiClient;

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
        Task<ExternalSystemSession> LoginAsync(
            string username,
            string password,
            CancellationToken ct = default);

        /// <summary>
        /// اندازه‌گیری تاخیر یک‌طرفه به سرور کارگزاری (میلی‌ثانیه).
        /// </summary>
        Task<BrokerTimeMeasurement> MeasureLatencyAsync(
                         ExternalSystemSession session,
                         CancellationToken ct = default);

        /// <summary>
        /// Serialize کردن session برای ذخیره در DB.
        /// </summary>
        string SerializeSession(ExternalSystemSession session);

        /// <summary>
        /// Deserialize کردن session از DB.
        /// </summary>
        ExternalSystemSession DeserializeSession(string json);

        /* ═══════════ ACCOUNT ═══════════ */
        Task<MoneyDto> GetCashBalanceAsync(ExternalSystemSession session, CancellationToken ct = default);
        Task<List<PortfolioPerformanceDto>> GetPortfolioPerformanceAsync(ExternalSystemSession session, CancellationToken ct = default);
        Task<ClientAppSettingDto> GetClientAppSettingAsync(ExternalSystemSession session, CancellationToken ct = default);
        Task<bool> IsCreditCustomerAsync(ExternalSystemSession session, CancellationToken ct = default);

        /* ═══════════ MARKET ═══════════ */
        Task<List<TradingTimeDto>> GetTradingTimesAsync(ExternalSystemSession session, CancellationToken ct = default);

        /* ═══════════ ORDER ═══════════ */
        HttpRequestMessage BuildOrderRequest(
            ExternalSystemSession session,
            string symbolIsin,
            long price,
            long quantity,
            int side);
        /// <summary>
        /// ارسال سفارش خرید.
        /// </summary>
        Task<BrokerOrderResultDto> SendBuyOrderAsync(
            ExternalSystemSession session,
            string symbolIsin,
            long price,
            long quantity,
            CancellationToken ct = default);

        /// <summary>
        /// ارسال سفارش فروش.
        /// </summary>
        Task<BrokerOrderResultDto> SendSellOrderAsync(
            ExternalSystemSession session,
            string symbolIsin,
            long price,
            long quantity,
            CancellationToken ct = default);

        Task<BrokerOrderResultDto> SendOrderWithRequestAsync(
    ExternalSystemSession session,
    HttpRequestMessage request,
    string symbolIsin,
    CancellationToken ct = default);

        /* ═══════════ SYMBOL ═══════════ */
        /// <summary>
        /// اطلاعات لحظه‌ای نماد.
        /// </summary>
        Task<SymbolMarketDataDto> GetSymbolInfoAsync(
            ExternalSystemSession session,
           string symbolIsin,
            CancellationToken ct = default);

        Task<ReturnChartDto> GetReturnChartAsync(
            ExternalSystemSession session, string symbolIsin, CancellationToken ct = default);

        /* ═══════════ CANDLES ═══════════ */
        Task<List<CandleDto>> GetCandlesAsync(
            ExternalSystemSession session,
            string symbolIsin,
            int days = 1,
            int intervalMinutes = 1,
            CancellationToken ct = default);

        /* ═══════════ ANALYSIS ═══════════ */
        Task<TechnicalAnalysisDto> GetTechnicalAnalysisAsync(
            ExternalSystemSession session, string symbolIsin, CancellationToken ct = default);

        Task<IndInstTradeDto> GetIndInstTradeAsync(
            ExternalSystemSession session, string symbolIsin, CancellationToken ct = default);

        Task<IndInstAnalysisDto> GetIndInstAnalysisAsync(
            ExternalSystemSession session, string symbolIsin, CancellationToken ct = default);

        Task<List<IndTradingTrendDto>> GetIndTradingTrendAsync(
            ExternalSystemSession session, string symbolIsin, CancellationToken ct = default);

        /* ═══════════ MARKET SHEET ═══════════ */
        Task<MarketSheetSumDto> GetMarketSheetSumAsync(
            ExternalSystemSession session, string symbolIsin, CancellationToken ct = default);
        /* ═══════════ MARKET LIVE ═══════════ */
        Task<List<IndustryPositiveNegativeDto>> GetIndustryPositiveNegativeAsync(
            ExternalSystemSession session, CancellationToken ct = default);

        Task<List<TseIndexDto>> GetTseIndexAsync(
            ExternalSystemSession session, CancellationToken ct = default);

        Task<List<MarketWatchCategoryDto>> GetMarketWatchAsync(
            ExternalSystemSession session, CancellationToken ct = default);


        /* ═══════════ BATCH MARKET DATA ═══════════ */
        Task<List<BatchMarketDataItemDto>> GetBatchMarketDataAsync(
            ExternalSystemSession session,
            IReadOnlyList<string> symbolIsins,
            CancellationToken ct = default);

        /* ═══════════ ORDER HISTORY ═══════════ */
        Task<PagedResult<OrderHistoryItemDto>> GetOrderHistoryAsync(
            ExternalSystemSession session,
            OrderHistoryQuery query,
            CancellationToken ct = default);

        /* ═══════════ ORDER TRADES ═══════════ */
        /// <summary>
        /// لیست fillهای یک سفارش خاص.
        /// اگه سفارش اصلاً پر نشده باشه، لیست خالی برمی‌گرده.
        /// </summary>
        Task<List<OrderTradeDto>> GetOrderTradesAsync(
            ExternalSystemSession session,
            string orderId,
            CancellationToken ct = default);

        /* ═══════════ FINANCE ═══════════ */
        /// <summary>
        /// لیست حساب‌های بانکی + موجودی قابل برداشت به تفکیک تاریخ و بانک.
        /// </summary>
        Task<PaymentAccountBalancesDto> GetPaymentAccountBalancesAsync(
            ExternalSystemSession session,
            CancellationToken ct = default);

        /// <summary>
        /// درخواست برداشت وجه.
        /// ⚠️ عملیات غیرقابل برگشت — قبل از ارسال، از <see cref="ValidateWithdrawal"/> استفاده کن.
        /// </summary>
        Task<WithdrawalResultDto> RequestWithdrawalAsync(
            ExternalSystemSession session,
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
            ExternalSystemSession session,
            long paymentId,
            CancellationToken ct = default);
        /* ═══════════ ANALYSIS ═══════════ */
        Task<FundamentalAnalysisDto> GetFundamentalAnalysisAsync(
            ExternalSystemSession session,
            string symbolIsin,
            CancellationToken ct = default);
    }
}