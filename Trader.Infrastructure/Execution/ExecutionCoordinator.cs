using Core.Application.Context;
using Microsoft.Extensions.Logging;
using Trader.Application.Abstractions;
using Trader.Application.Abstractions.Brokers;
using Trader.Application.Abstractions.Execution;
using Trader.Application.Abstractions.Logging;
using Trader.Application.Abstractions.TradingRuns;
using Trader.Application.Runtime;
using Trader.Domain.Enums;
using Trader.Infrastructure.Data;

namespace Trader.Infrastructure.Execution;

public sealed class ExecutionCoordinator : IExecutionCoordinator
{
    private readonly IRunExecutionContextCache _runCache;
    private readonly IOrderIntentBuilder _intentBuilder;
    private readonly IBrokerOrderDispatcher _dispatcher;
    private readonly IBrokerSessionProvider _sessionProvider;
    private readonly IBrokerClientFactory _brokerFactory;
    private readonly IUserContextCache _userContextCache;
    private readonly UserDataContext _userContext;
    private readonly ITraderUnitOfWork<TraderDbContext> _uow;
    private readonly ITradingEventLogger _eventLogger;
    private readonly ILogger<ExecutionCoordinator> _logger;

    public ExecutionCoordinator(
        IRunExecutionContextCache runCache,
        IOrderIntentBuilder intentBuilder,
        IBrokerOrderDispatcher dispatcher,
        IBrokerSessionProvider sessionProvider,
        IBrokerClientFactory brokerFactory,
        IUserContextCache userContextCache,
        UserDataContext userContext,
        ITraderUnitOfWork<TraderDbContext> uow,
    ITradingEventLogger eventLogger,
        ILogger<ExecutionCoordinator> logger)
    {
        _runCache = runCache;
        _intentBuilder = intentBuilder;
        _dispatcher = dispatcher;
        _sessionProvider = sessionProvider;
        _brokerFactory = brokerFactory;
        _userContextCache = userContextCache;
        _userContext = userContext;
        _uow = uow;
        _eventLogger = eventLogger;
        _logger = logger;
    }

    public async Task HandleAsync(SignalEnvelope envelope, CancellationToken ct = default)
    {
        var ctx = _runCache.Get(envelope.RunId);
        if (ctx is null)
        {
            _logger.LogDebug(
                "Run {RunId} not active; skipping signal {SignalId}.",
                envelope.RunId, envelope.Signal.Id);
            return;
        }

        if (envelope.Signal.Action == SignalAction.Hold)
            return;

        // ─── Set User Context ───
        try
        {
            var userCtx = await _userContextCache.GetOrLoadAsync(ctx.OwnerUserId, ct);
            _userContext.ApplyFrom(userCtx);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to load user context for run {RunId}. Skipping signal.",
                envelope.RunId);
            return;
        }

        // ─── برای هر Account ───
        var successCount = 0;

        foreach (var accountId in ctx.AccountIds)
        {
            try
            {
                var built = await ExecuteForAccountAsync(envelope, ctx, accountId, ct);
                if (built) successCount++;
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Order execution failed: run={RunId} account={AccountId} isin={Isin}",
                    ctx.RunId, accountId, envelope.Signal.Isin);
            }
        }

        // ─── Save همه چیز در یک تراکنش ───
        if (successCount > 0 || envelope.Signal.Action != SignalAction.Hold)
        {
            envelope.Signal.MarkConsumed();

            try
            {
                await _uow.SaveChangesAsync(ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to persist execution results for signal {SignalId}.",
                    envelope.Signal.Id);
            }
        }
    }

    public Task CloseRunPositionsAsync(Guid runId, CancellationToken ct = default)
    {
        // در Stage بعد پیاده‌سازی می‌شود. برای Phase 1، Stop فقط Cache را خالی می‌کند.
        _logger.LogInformation(
            "CloseRunPositions requested for run {RunId} — not yet implemented.",
            runId);
        return Task.CompletedTask;
    }

    /* ═══════════════════ Helpers ═══════════════════ */

    private async Task<bool> ExecuteForAccountAsync(
    SignalEnvelope envelope,
    RunRuntimeContext ctx,
    Guid accountId,
    CancellationToken ct)
    {
        // ۱. Build OrderIntent
        var intent = await _intentBuilder.BuildAsync(envelope.Signal, ctx, accountId, ct);

        if (intent is null)
        {
            _eventLogger.Log(
                stage: TradingStage.Execution,
                type: TradingEventType.OrderIntentSkipped,
                severity: TradingEventSeverity.Debug,
                message: $"OrderIntent برای «{envelope.Signal.Isin}» ساخته نشد. " +
                         $"احتمالاً سرمایه کافی نیست یا Signal رد شد.",
                runId: ctx.RunId,
                isin: envelope.Signal.Isin,
                accountId: accountId,
                signalId: envelope.Signal.Id);
            return false;
        }

        // ─── OrderIntentCreated Event ───
        _eventLogger.Log(
            stage: TradingStage.Execution,
            type: TradingEventType.OrderIntentCreated,
            severity: TradingEventSeverity.Info,
            message: $"تصمیم اجرا: {OrderSideToPersian(intent.Side)} " +
                     $"{intent.Quantity.Value:N0} سهم «{intent.Isin}» " +
                     $"به قیمت {intent.Price.Value:N0} ریال" +
                     (intent.StopLossPrice.HasValue
                        ? $" (SL: {intent.StopLossPrice.Value:N0})"
                        : "") +
                     (intent.TakeProfitPrice.HasValue
                        ? $" (TP: {intent.TakeProfitPrice.Value:N0})"
                        : ""),
            payload: new
            {
                side = intent.Side.ToString(),
                quantity = intent.Quantity.Value,
                price = intent.Price.Value,
                stopLossPrice = intent.StopLossPrice?.Value,
                takeProfitPrice = intent.TakeProfitPrice?.Value,
            },
            runId: ctx.RunId,
            isin: intent.Isin,
            accountId: accountId,
            signalId: envelope.Signal.Id,
            orderIntentId: intent.Id);

        // ۲. Account
        var account = await _uow.Accounts.GetByIdAsync(accountId);
        if (account is null)
        {
            _eventLogger.Log(
                stage: TradingStage.Execution,
                type: TradingEventType.OrderDispatchFailed,
                severity: TradingEventSeverity.Error,
                message: $"حساب معاملاتی پیدا نشد: {accountId}",
                runId: ctx.RunId,
                isin: intent.Isin,
                accountId: accountId,
                orderIntentId: intent.Id);
            return false;
        }

        var session = await _sessionProvider.GetSessionAsync(accountId, ct);

        // ۳. Dispatch
        try
        {
            var brokerOrder = await _dispatcher.DispatchAsync(intent, account.Broker, session, ct);

            // ─── OrderDispatched Event ───
            _eventLogger.Log(
                stage: TradingStage.Execution,
                type: TradingEventType.OrderDispatched,
                severity: TradingEventSeverity.Info,
                message: $"سفارش {OrderSideToPersian(intent.Side)} " +
                         $"«{intent.Isin}» به کارگزاری {account.Broker} ارسال شد. " +
                         $"شماره: {brokerOrder.BrokerOrderId ?? "—"}",
                payload: new
                {
                    brokerOrderId = brokerOrder.BrokerOrderId,
                    broker = account.Broker.ToString(),
                },
                runId: ctx.RunId,
                isin: intent.Isin,
                accountId: accountId,
                orderIntentId: intent.Id,
                brokerOrderId: brokerOrder.Id);

            return true;
        }
        catch (Exception ex)
        {
            _eventLogger.Log(
                stage: TradingStage.Execution,
                type: TradingEventType.OrderDispatchFailed,
                severity: TradingEventSeverity.Error,
                message: $"ارسال سفارش «{intent.Isin}» به کارگزاری {account.Broker} " +
                         $"ناموفق بود: {ex.Message}",
                payload: new
                {
                    errorType = ex.GetType().Name,
                    errorMessage = ex.Message,
                    broker = account.Broker.ToString(),
                },
                runId: ctx.RunId,
                isin: intent.Isin,
                accountId: accountId,
                orderIntentId: intent.Id);

            throw;
        }
    }
    private static string OrderSideToPersian(OrderSide side) => side switch
    {
        OrderSide.Buy => "خرید",
        OrderSide.Sell => "فروش",
        _ => side.ToString()
    };
}