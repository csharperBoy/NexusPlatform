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
        /// <summary>
        /// درخواست برداشت وجه از حساب کاربری در کارگزاری
        /// </summary>
        /// <param name="session"></param>
        /// <param name="request"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<WithdrawalResultDto> RequestWithdrawalAsync(
                ExternalSystemSession session, WithdrawalRequestDto request,
                CancellationToken ct = default);

        /// <summary>
        /// درخواست لغو پرداخت از کارگزاری
        /// </summary>
        /// <param name="session"></param>
        /// <param name="paymentId"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<PaymentCancelResultDto> CancelPaymentAsync(
                ExternalSystemSession session, long paymentId,
                CancellationToken ct = default);
        }
    
}
