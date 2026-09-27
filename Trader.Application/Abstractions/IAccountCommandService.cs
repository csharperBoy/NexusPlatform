using Trader.Domain.Enums;

namespace Trader.Application.Abstractions
{
    public interface IAccountCommandService
    {
        Task<Guid> CreateAccountAsync(
            BrokerType broker,
            string name,
            string username,
            string password);

        Task<Guid> UpdateAccountAsync(
            Guid id,
            string name,
            string username,
            string? password);

        Task<bool> DeleteAccountAsync(Guid id);

        Task LoginAsync(Guid id);
        Task SaveAsync();
    }
}