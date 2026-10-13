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
        /// <summary>
        /// دریافت قدرت خرید حساب کاربر
        /// </summary>
        /// <param name="session"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<MoneyDto> GetCashBalanceAsync(
                ExternalSystemSession session, CancellationToken ct = default);

        /// <summary>
        /// دریافت پرتفوی کاربر
        /// </summary>
        /// <param name="session"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<List<PortfolioPerformanceDto>> GetPortfolioPerformanceAsync(
                ExternalSystemSession session, CancellationToken ct = default);

        ///// <summary>
        ///// دریافت تنظیمات اپلیکیشن کاربر
        ///// </summary>
        ///// <param name="session"></param>
        ///// <param name="ct"></param>
        ///// <returns></returns>
        //Task<ClientAppSettingDto> GetClientAppSettingAsync(
        //        ExternalSystemSession session, CancellationToken ct = default);

        ///// <summary>
        ///// بررسی اینکه آیا کاربر مشتری اعتباری است یا خیر
        ///// </summary>
        ///// <param name="session"></param>
        ///// <param name="ct"></param>
        ///// <returns></returns>
        //Task<bool> IsCreditCustomerAsync(
        //        ExternalSystemSession session, CancellationToken ct = default);

        ///// <summary>
        ///// دریافت موجودی قابل برداشت برای هس حساب بانکی کاربر
        ///// </summary>
        ///// <param name="session"></param>
        ///// <param name="ct"></param>
        ///// <returns></returns>
        //Task<PaymentAccountBalancesDto> GetPaymentAccountBalancesAsync(
        //        ExternalSystemSession session, CancellationToken ct = default);
        }
    
}
