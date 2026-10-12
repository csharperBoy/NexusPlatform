using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Trader.Application.Dtos.Finance;
using Trader.ExternalApiClient;

namespace Trader.Application.Abstractions.Brokers.Commands
{
        public interface IBrokerFinanceCommands : IBroker
        {
            Task<WithdrawalResultDto> RequestWithdrawalAsync(
                ExternalSystemSession session, WithdrawalRequestDto request,
                CancellationToken ct = default);

            Task<PaymentCancelResultDto> CancelPaymentAsync(
                ExternalSystemSession session, long paymentId,
                CancellationToken ct = default);
        }
    
}
