using Core.Application.Abstractions;
using Core.Application.Context;
using Core.Application.Provider;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using Trader.Application.Abstractions;

namespace Trader.Infrastructure.Services;

/// <summary>
/// Singleton. BuildAsync را در Provider (Scoped) صدا می‌زند از طریق ScopeFactory.
/// TTL پیش‌فرض ۵ دقیقه.
/// </summary>
public sealed class UserContextCache : IUserContextCache
{
    private static readonly TimeSpan Ttl = TimeSpan.FromMinutes(5);

    private sealed record Entry(UserDataContext Context, DateTime LoadedAt);

    private readonly ConcurrentDictionary<Guid, Entry> _cache = new();
    private readonly ConcurrentDictionary<Guid, SemaphoreSlim> _locks = new();
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<UserContextCache> _logger;

    public UserContextCache(
        IServiceScopeFactory scopeFactory,
        ILogger<UserContextCache> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    public async Task<UserDataContext> GetOrLoadAsync(
     Guid userId, CancellationToken ct = default)
    {
        if (userId == Guid.Empty)
            return UserDataContext.Empty();

        if (_cache.TryGetValue(userId, out var entry)
            && DateTime.UtcNow - entry.LoadedAt < Ttl)
        {
            return entry.Context;
        }

        var semaphore = _locks.GetOrAdd(userId, _ => new SemaphoreSlim(1, 1));
        await semaphore.WaitAsync(ct);

        try
        {
            if (_cache.TryGetValue(userId, out entry)
                && DateTime.UtcNow - entry.LoadedAt < Ttl)
            {
                return entry.Context;
            }

            using var scope = _scopeFactory.CreateScope();
            var provider = scope.ServiceProvider
                .GetRequiredService<IUserDataContextProvider>();

            UserDataContext snapshot;
            try
            {
                snapshot = await provider.BuildAsync(userId, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "BuildAsync failed for userId={UserId}. Returning Empty() so pipeline continues.",
                    userId);
                snapshot = UserDataContext.Empty();
            }

            _cache[userId] = new Entry(snapshot, DateTime.UtcNow);

            _logger.LogDebug(
                "User context loaded: userId={UserId} permissions={Count} roles={RoleCount}",
                userId,
                snapshot.Permissions?.Count ?? 0,
                snapshot.RoleIds?.Count ?? 0);

            return snapshot;
        }
        finally
        {
            semaphore.Release();
        }
    }

    public void Invalidate(Guid userId)
    {
        if (_cache.TryRemove(userId, out _))
            _logger.LogInformation(
                "User context invalidated: {UserId}", userId);
    }

    public void InvalidateAll()
    {
        _cache.Clear();
        _logger.LogInformation("User context cache cleared.");
    }
}