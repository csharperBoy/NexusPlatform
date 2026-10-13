using Trader.Application.Dtos;

namespace Trader.Application.Abstractions
{
    public interface IAccountQueryService
    {
        /// <summary>
        /// دریافت لیست حساب‌ها
        /// </summary>
        /// <returns></returns>
        Task<IReadOnlyList<AccountInfoView>> GetAccountListAsync();
    }
}