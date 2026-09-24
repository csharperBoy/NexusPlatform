namespace Trader.Application.Abstractions
{
    public interface IAccountCommandService
    {
        Task<Guid> CreateAccountAsync(
            string name,
            string username,
            string password);

        Task<Guid> UpdateAccountAsync(
            Guid id,
            string name,
            string username,
            string? password);

        Task<bool> DeleteAccountAsync(Guid id);

        /// <summary>لاگین به کارگزاری و ذخیره‌ی توکن رمزنگاری‌شده.</summary>
        Task<bool> LoginAsync(Guid id);

        /// <summary>فعال‌سازی مجدد توکن (same-login).</summary>
        Task<bool> ActivateAsync(Guid id);

        Task SaveAsync();
    }
}