using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Registry;
using Trader.Application.Abstractions;
using Trader.Application.Abstractions.Backtesting;
using Trader.Domain.Entities;
using Trader.Domain.Enums;
using Trader.Infrastructure.Data;

namespace Trader.Infrastructure.Backtesting;

public sealed class BacktestEventLogger : IBacktestEventLogger
{
    private const int MaxBufferSize = 20_000;

    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IAsyncPolicy _retryPolicy;
    private readonly ILogger<BacktestEventLogger> _logger;
    private readonly List<BacktestEvent> _buffer = new();

    public BacktestEventLogger(
        IServiceScopeFactory scopeFactory,
        IReadOnlyPolicyRegistry<string> policies,
        ILogger<BacktestEventLogger> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;

        if (policies.TryGet<IAsyncPolicy>("DbRetry", out var dbRetry))
            _retryPolicy = dbRetry;
        else if (policies.TryGet<IAsyncPolicy>("DefaultRetry", out var defRetry))
            _retryPolicy = defRetry;
        else
            _retryPolicy = Policy.NoOpAsync();
    }

    public void Log(
        Guid backtestRunId,
        BacktestEventType type,
        BacktestEventSeverity severity,
        string message,
        object? payload = null,
        string? isin = null,
        DateTime? candleTime = null,
        Guid? tradeId = null)
    {
        try
        {
            string? payloadJson = null;
            if (payload is not null)
                payloadJson = JsonSerializer.Serialize(payload, JsonOpts);

            _buffer.Add(BacktestEvent.Create(
                backtestRunId: backtestRunId,
                type: type,
                severity: severity,
                message: message,
                payloadJson: payloadJson,
                isin: isin,
                candleTime: candleTime,
                tradeId: tradeId));

            if (_buffer.Count > MaxBufferSize)
            {
                _logger.LogWarning(
                    "BacktestEvent buffer overflow ({Count} > {Max}); dropping oldest.",
                    _buffer.Count, MaxBufferSize);
                _buffer.RemoveRange(0, _buffer.Count - MaxBufferSize);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to buffer backtest event: {Type}", type);
        }
    }

    public async Task FlushAsync(CancellationToken ct = default)
    {
        if (_buffer.Count == 0) return;

        var batch = _buffer.ToList();

        try
        {
            await _retryPolicy.ExecuteAsync(async () =>
            {
                using var scope = _scopeFactory.CreateScope();
                var uow = scope.ServiceProvider
                    .GetRequiredService<ITraderUnitOfWork<TraderDbContext>>();

                foreach (var e in batch)
                    await uow.BacktestEvents.AddAsync(e);

                await uow.SaveChangesAsync(ct);
            });

            _buffer.Clear();
            _logger.LogDebug("Flushed {Count} backtest events.", batch.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to persist {Count} backtest events after retry policy.",
                batch.Count);
        }
    }
}