using Core.Application.Context;

namespace Trader.Application.Abstractions;

/// <summary>
/// کش Snapshotهای UserDataContext.
/// برای BackgroundService که UserDataContext هر Tick نیاز دارد.
/// </summary>
public interface IUserContextCache
{
    Task<UserDataContext> GetOrLoadAsync(
        Guid userId, CancellationToken ct = default);

    void Invalidate(Guid userId);
    void InvalidateAll();
}