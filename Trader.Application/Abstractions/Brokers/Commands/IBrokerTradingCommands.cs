using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Trader.Application.Dtos;
using Trader.ExternalApiClient;

namespace Trader.Application.Abstractions.Brokers.Commands
{
        public interface IBrokerTradingCommands : IBroker
        {
            Task<BrokerOrderResultDto> SendBuyOrderAsync(
                ExternalSystemSession session, string symbolIsin, long price, long quantity,
                CancellationToken ct = default);

            Task<BrokerOrderResultDto> SendSellOrderAsync(
                ExternalSystemSession session, string symbolIsin, long price, long quantity,
                CancellationToken ct = default);

            /// <summary>ساخت request بدون ارسال — برای fire لحظه‌ای.</summary>
            HttpRequestMessage BuildOrderRequest(
                ExternalSystemSession session, string symbolIsin, long price, long quantity, int side);

            /// <summary>ارسال request آماده‌شده.</summary>
            Task<BrokerOrderResultDto> SendOrderWithRequestAsync(
                ExternalSystemSession session, HttpRequestMessage request, string symbolIsin,
                CancellationToken ct = default);
        }
    
}
