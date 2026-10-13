using Trader.Domain.Enums;

namespace Trader.Application.Abstractions
{
    public interface IAccountCommandService
    {
        /// <summary>
        /// ساخت اکانت داخل دیتابیس
        /// </summary>
        /// <param name="broker"></param>
        /// <param name="name"></param>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        Task<Guid> CreateAccountAsync(
            BrokerType broker,
            string name,
            string username,
            string password);

        /// <summary>
        /// بروزرسانی اکانت داخل دیتابیس
        /// </summary>
        /// <param name="id"></param>
        /// <param name="name"></param>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        Task<Guid> UpdateAccountAsync(
            Guid id,
            string name,
            string username,
            string? password);

        /// <summary>
        /// حذف اکانت از دیتابیس
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<bool> DeleteAccountAsync(Guid id);

        /// <summary>
        /// ورود به اکانت و دریافت توکن و ذخیره در دیتابیس
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task LoginAsync(Guid id);

        /// <summary>
        /// ذخیره تغییرات در دیتابیس ( داخل unit of work)
        /// </summary>
        /// <returns></returns>
        Task SaveAsync();
    }
}