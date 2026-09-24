using Trader.Application.EasyTrader.Models;

namespace Trader.Application.Abstractions
{
    /// <summary>
    /// کلاینت کارگزاری مفید (EasyTrader).
    /// پیاده‌سازی: Trader.Infrastructure.EasyTrader.EasyTraderClient
    /// </summary>
    public interface IEasyTraderClient
    {
        /// <summary>
        /// لاگین کامل OIDC + PKCE + activation (same-login).
        /// </summary>
        Task<LoginResult> LoginAsync(
            string username,
            string password,
            CancellationToken ct = default);

        /// <summary>
        /// فعال‌سازی توکن (same-login) — بعد از login یا برای refresh.
        /// </summary>
        Task ActivateTokenAsync(
            string accessToken,
            CancellationToken ct = default);

        /// <summary>
        /// دریافت اختلاف زمان با سرور کارگزاری.
        /// </summary>
        Task<ServerTimeResult> GetServerTimeAsync(
            string accessToken,
            CancellationToken ct = default);

        /// <summary>
        /// اطلاعات لحظه‌ای نماد (سقف/کف مجاز، آخرین معامله و ...).
        /// </summary>
        Task<MarketSymbolInfoResult> GetSymbolInfoAsync(
            string accessToken,
            string symbolIsin,
            CancellationToken ct = default);

        /// <summary>
        /// ارسال سفارش.
        /// </summary>
        Task<OrderResult> SendOrderAsync(
            string accessToken,
            OrderPayload payload,
            CancellationToken ct = default);
    }
}