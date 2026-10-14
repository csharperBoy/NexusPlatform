using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using Trader.Application.Abstractions.Brokers;
using Trader.ExternalApiClient;

namespace Trader.Infrastructure.Brokers;

/// <summary>
/// نسخه‌ی In-memory. در آینده با RedisSessionCache جایگزین می‌شود.
/// </summary>
public sealed class InMemoryBrokerSessionCache : IBrokerSessionCache
{
    private readonly ConcurrentDictionary<Guid, ExternalSystemSession> _sessions = new();
    private readonly ConcurrentDictionary<Guid, SemaphoreSlim> _locks = new();
    private readonly ILogger<InMemoryBrokerSessionCache> _logger;

    public InMemoryBrokerSessionCache(ILogger<InMemoryBrokerSessionCache> logger)
    {
        _logger = logger;
    }

    public bool TryGet(Guid accountId, out ExternalSystemSession? session)
    {
        var found = _sessions.TryGetValue(accountId, out var s);
        session = s;
        return found;
    }

    public void Set(Guid accountId, ExternalSystemSession session)
    {
        _sessions[accountId] = session;
        _logger.LogDebug(
            "Session cached for account {AccountId}, expires={Exp}",
            accountId, session.ExpiresAt);
    }

    public void Invalidate(Guid accountId)
    {
        if (_sessions.TryRemove(accountId, out _))
            _logger.LogInformation(
                "Session invalidated for account {AccountId}", accountId);
    }

    public async Task<IDisposable> AcquireLockAsync(
        Guid accountId, CancellationToken ct = default)
    {
        var semaphore = _locks.GetOrAdd(accountId, _ => new SemaphoreSlim(1, 1));
        await semaphore.WaitAsync(ct);
        return new Releaser(semaphore);
    }

    private sealed class Releaser : IDisposable
    {
        private readonly SemaphoreSlim _semaphore;
        private bool _disposed;

        public Releaser(SemaphoreSlim semaphore) => _semaphore = semaphore;

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            _semaphore.Release();
        }
    }
}