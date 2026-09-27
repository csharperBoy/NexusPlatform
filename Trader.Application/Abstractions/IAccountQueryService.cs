using Trader.Application.Dtos;

namespace Trader.Application.Abstractions
{
    public interface IAccountQueryService
    {
        Task<IReadOnlyList<AccountInfoView>> GetAccountListAsync();
    }
}