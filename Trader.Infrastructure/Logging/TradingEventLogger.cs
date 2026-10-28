using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Trader.Application.Abstractions.Logging;
using Trader.Application.Runtime;
using Trader.Domain.Entities;
using Trader.Domain.Enums;
using Trader.Infrastructure.Data;

namespace Trader.Infrastructure.Logging;

public sealed class TradingEventLogger : ITradingEventLogger
{
    private const int MaxBufferSize = 5000;

    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        WriteIndented = false,
    };

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly TradingEventSeverity _minSeverity;
    private readonly ILogger<TradingEventLogger> _logger;
    private readonly List<TradingEvent> _buffer = new();

    public TradingEventLogger(
        IServiceScopeFactory scopeFactory,
        IOptionsMonitor<TradingLoggingOptions> options,
        ILogger<TradingEventLogger> logger)
    {
        _scopeFactory = scopeFactory;
        _minSeverity = options.CurrentValue.GetMinSeverityEnum();
        _logger = logger;
    }

    public void Log(
        TradingStage stage,
        TradingEventType type,
        TradingEventSeverity severity,
        string message,
        object? payload = null,
        Guid? runId = null,
        string? isin = null,
        Guid? accountId = null,
        Guid? signalId = null,
        Guid? orderIntentId = null,
        Guid? brokerOrderId = null,
        Guid? tradeId = null,
        Guid? positionId = null)
    {
        if (severity < _minSeverity) return;

        try
        {
            string? payloadJson = null;
            if (payload is not null)
                payloadJson = JsonSerializer.Serialize(payload, JsonOpts);

            var evt = TradingEvent.Create(
                stage: stage,
                type: type,
                severity: severity,
                message: message,
                payloadJson: payloadJson,
                runId: runId,
                isin: isin,
                accountId: accountId,
                signalId: signalId,
                orderIntentId: orderIntentId,
                brokerOrderId: brokerOrderId,
                tradeId: tradeId,
                positionId: positionId);

            _buffer.Add(evt);

            if (_buffer.Count > MaxBufferSize)
            {
                _logger.LogWarning(
                    "TradingEvent buffer overflow ({Count} > {Max}); dropping oldest.",
                    _buffer.Count, MaxBufferSize);
                _buffer.RemoveRange(0, _buffer.Count - MaxBufferSize);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to buffer trading event: {Type} {Message}", type, message);
        }
    }

    public async Task FlushAsync(CancellationToken ct = default)
    {
        if (_buffer.Count == 0) return;

        var batch = _buffer.ToList();

        try
        {
            // ─── Scope جدید: DbContext تازه و جدا از Stageها ───
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<TraderDbContext>();

            await db.TradingEvents.AddRangeAsync(batch, ct);
            await db.SaveChangesAsync(ct);

            _buffer.Clear();
            _logger.LogDebug("Flushed {Count} trading events.", batch.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to persist {Count} trading events. Buffer retained for next flush.",
                batch.Count);
        }
    }
}