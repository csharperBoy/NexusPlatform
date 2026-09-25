using Core.Application.Abstractions;
using Core.Application.Abstractions.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Trader.Application.Abstractions;
using Trader.Application.Brokers;
using Trader.Application.Dtos;
using Trader.Domain.Entities;
using Trader.Domain.Enums;
using Trader.Infrastructure.Data;

namespace Trader.Infrastructure.Services
{
    public class ServerClockService : IServerClockCommandService, IServerClockQueryService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IBrokerClientFactory _brokerFactory;
        private readonly ILogger<ServerClockService> _logger;

        private readonly object _lock = new();
        private readonly Queue<LatencySample> _samples = new();
        private const int MAX_SAMPLES = 30;
        private const int WINDOW_SECONDS = 300;

        private long _diff;
        private long _oneWayLatency;
        private long _lastUpdatedAt;
        private int _samplesCount;
        private string? _lastError;

        public ServerClockService(
            IServiceScopeFactory scopeFactory,
            IBrokerClientFactory brokerFactory,
            ILogger<ServerClockService> logger)
        {
            _scopeFactory = scopeFactory;
            _brokerFactory = brokerFactory;
            _logger = logger;
        }

        public async Task<ServerClockInfoView> SyncAsync(int samples)
        {
            try
            {
                var (brokerClient, session) = await GetDefaultSessionAsync();

                var results = new List<long>();
                for (int i = 0; i < samples; i++)
                {
                    try
                    {
                        var latency = await brokerClient.MeasureLatencyAsync(session);
                        results.Add(latency);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex,
                            "Sample {Index}/{Total} failed", i + 1, samples);
                    }
                }

                if (results.Count == 0)
                {
                    lock (_lock) { _lastError = "All samples failed"; }
                    return GetStatusSnapshot();
                }

                lock (_lock)
                {
                    var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

                    while (_samples.Count > 0 &&
                           now - _samples.Peek().Timestamp > WINDOW_SECONDS)
                    {
                        _samples.Dequeue();
                    }

                    foreach (var latency in results)
                    {
                        _samples.Enqueue(new LatencySample(latency, now));
                        while (_samples.Count > MAX_SAMPLES)
                            _samples.Dequeue();
                    }

                    var values = _samples
                        .Select(s => s.LatencyMs)
                        .OrderBy(x => x)
                        .ToList();
                    var median = ComputeMedian(values);

                    _oneWayLatency = median;
                    _diff = median;
                    _lastUpdatedAt = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                    _samplesCount = _samples.Count;
                    _lastError = null;
                }

                _logger.LogInformation(
                    "Server clock synced: median={Median}ms samples={Count}",
                    _oneWayLatency, _samplesCount);

                return GetStatusSnapshot();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Server clock sync failed");
                lock (_lock) { _lastError = ex.Message; }
                return GetStatusSnapshot();
            }
        }

        public Task<ServerClockInfoView> GetStatusAsync()
            => Task.FromResult(GetStatusSnapshot());

        private ServerClockInfoView GetStatusSnapshot()
        {
            lock (_lock)
            {
                return new ServerClockInfoView
                {
                    Diff = _diff,
                    Offset = _diff,
                    OneWayLatency = _oneWayLatency,
                    LastUpdatedAt = _lastUpdatedAt,
                    SamplesCount = _samplesCount,
                    LastError = _lastError,
                };
            }
        }

        private async Task<(IBrokerClient Client, BrokerSession Session)>
            GetDefaultSessionAsync()
        {
            using var scope = _scopeFactory.CreateScope();
            var accountRepo = scope.ServiceProvider
                .GetRequiredService<IRepository<TraderDbContext, TraderAccount, Guid>>();
            var protector = scope.ServiceProvider
                .GetRequiredService<ISecretProtector>();

            var accounts = await accountRepo.GetAllAsync();

            var account = accounts
                .Where(a => a.GetSessionStatus() == SessionStatus.Valid)
                .OrderBy(a => a.Name)
                .FirstOrDefault()
                ?? throw new Exception(
                    "No account with valid session available.");

            var brokerClient = _brokerFactory.GetClient(account.Broker);
            var sessionJson = protector.Unprotect(account.EncryptedSession!);
            var session = brokerClient.DeserializeSession(sessionJson);

            return (brokerClient, session);
        }

        private static long ComputeMedian(List<long> sorted)
        {
            if (sorted.Count == 0) return 0;
            var mid = sorted.Count / 2;
            return sorted.Count % 2 == 0
                ? (sorted[mid - 1] + sorted[mid]) / 2
                : sorted[mid];
        }

        private record LatencySample(long LatencyMs, long Timestamp);
    }
}