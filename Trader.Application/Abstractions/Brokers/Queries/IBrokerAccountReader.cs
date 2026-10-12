using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Trader.Application.Dtos.Account;
using Trader.Application.Dtos.Finance;
using Trader.ExternalApiClient;

namespace Trader.Application.Abstractions.Brokers.Queries
{
        public interface IBrokerAccountReader : IBroker
        {
            Task<MoneyDto> GetCashBalanceAsync(
                ExternalSystemSession session, CancellationToken ct = default);

            Task<List<PortfolioPerformanceDto>> GetPortfolioPerformanceAsync(
                ExternalSystemSession session, CancellationToken ct = default);

            Task<ClientAppSettingDto> GetClientAppSettingAsync(
                ExternalSystemSession session, CancellationToken ct = default);

            Task<bool> IsCreditCustomerAsync(
                ExternalSystemSession session, CancellationToken ct = default);

            Task<PaymentAccountBalancesDto> GetPaymentAccountBalancesAsync(
                ExternalSystemSession session, CancellationToken ct = default);
        }
    
}
