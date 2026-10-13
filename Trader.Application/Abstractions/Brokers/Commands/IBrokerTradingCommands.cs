using Trader.Application.Dtos;
using Trader.ExternalApiClient;

namespace Trader.Application.Abstractions.Brokers.Commands
{
        public interface IBrokerTradingCommands : IBroker
        {
        /// <summary>
        /// ارسال سفارش خرید به کارگزاری
        /// </summary>
        /// <param name="session"></param>
        /// <param name="symbolIsin"></param>
        /// <param name="price"></param>
        /// <param name="quantity"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<BrokerOrderResultDto> SendBuyOrderAsync(
                ExternalSystemSession session, string symbolIsin, long price, long quantity,
                CancellationToken ct = default);

        /// <summary>
        /// ارسال سفارش فروش به کارگزاری
        /// </summary>
        /// <param name="session"></param>
        /// <param name="symbolIsin"></param>
        /// <param name="price"></param>
        /// <param name="quantity"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<BrokerOrderResultDto> SendSellOrderAsync(
                ExternalSystemSession session, string symbolIsin, long price, long quantity,
                CancellationToken ct = default);

        /// <summary>
        /// ارسال سفارش خرید یا فروش به کارگزاری با استفاده از درخواست آماده شده
        /// </summary>
        /// <param name="session"></param>
        /// <param name="request"></param>
        /// <param name="symbolIsin"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<BrokerOrderResultDto> SendOrderWithRequestAsync(
           ExternalSystemSession session,
           HttpRequestMessage request,
           string symbolIsin,
           CancellationToken ct = default);

        /// <summary>
        ///  ساخت درخواست ارسال سفارش خرید یا فروش به کارگزاری
        /// </summary>
        /// <param name="session"></param>
        /// <param name="symbolIsin"></param>
        /// <param name="price"></param>
        /// <param name="quantity"></param>
        /// <param name="side"></param>
        /// <returns></returns>
        HttpRequestMessage BuildOrderRequest(
           ExternalSystemSession session,
           string symbolIsin,
           long price,
           long quantity,
           int side);

        }
    
}
