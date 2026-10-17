namespace Trader.Application.Abstractions;

public interface IAccountBalanceCache
{
    bool TryGet(Guid accountId, out decimal balance);
    void Set(Guid accountId, decimal balance, TimeSpan ttl);
    void Invalidate(Guid accountId);
}