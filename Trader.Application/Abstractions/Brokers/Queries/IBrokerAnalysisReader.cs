using Trader.Application.Dtos.MarketData;
using Trader.ExternalApiClient;

namespace Trader.Application.Abstractions.Brokers.Queries
{
        public interface IBrokerAnalysisReader : IBroker
        {
        /// <summary>
        /// دریافت تحلیل تکنیکال یک نماد از کارگزاری
        /// </summary>
        /// <param name="session"></param>
        /// <param name="symbolIsin"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<TechnicalAnalysisDto> GetTechnicalAnalysisAsync(
                ExternalSystemSession session, string symbolIsin, CancellationToken ct = default);

        /// <summary>
        /// دریافت تحلیل بنیادی یک نماد از کارگزاری
        /// </summary>
        /// <param name="session"></param>
        /// <param name="symbolIsin"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<FundamentalAnalysisDto> GetFundamentalAnalysisAsync(
                ExternalSystemSession session, string symbolIsin, CancellationToken ct = default);

        /// <summary>
        /// دریافت معاملات حقیقی و حقوقی یک نماد از کارگزاری
        ///  حجم/تعداد خرید و فروش حقیقی و حقوقی (لحظه‌ای)
        /// </summary>
        /// <param name="session"></param>
        /// <param name="symbolIsin"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<IndInstTradeDto> GetIndInstTradeAsync(
                ExternalSystemSession session, string symbolIsin, CancellationToken ct = default);

        /// <summary>
        /// دریافت تحلیل معاملات حقیقی و حقوقی یک نماد از کارگزاری  — نسبت‌ها و قدرت خریدار
        /// </summary>
        /// <param name="session"></param>
        /// <param name="symbolIsin"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<IndInstAnalysisDto> GetIndInstAnalysisAsync(
                ExternalSystemSession session, string symbolIsin, CancellationToken ct = default);

        /// <summary>
        /// دریافت روند معاملات حقیقی و حقوقی یک نماد از کارگزاری
        /// </summary>
        /// <param name="session"></param>
        /// <param name="symbolIsin"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<List<IndTradingTrendDto>> GetIndTradingTrendAsync(
                ExternalSystemSession session, string symbolIsin, CancellationToken ct = default);
        }
    
}
