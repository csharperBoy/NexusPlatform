# Trading Module — Backend Architecture

> مستند جامع جریان کار، تصمیم‌گیری، و اجرا در ماژول Trading.
> آخرین به‌روزرسانی: ۱۴۰۴/۰۸/۰۵

---

## فهرست

1. [معرفی کلی](#1-معرفی-کلی)
2. [مفاهیم پایه](#2-مفاهیم-پایه)
3. [نقشه‌ی معماری](#3-نقشه-ی-معماری)
4. [Data Model](#4-data-model)
5. [Runtime Worker و Stages](#5-runtime-worker-و-stages)
6. [Stage 1 — Market Data Ingestion](#6-stage-1--market-data-ingestion)
7. [Stage 2 — Strategy Evaluation](#7-stage-2--strategy-evaluation)
8. [Stage 3 — Execution](#8-stage-3--execution)
9. [Stage 4 — Order Lifecycle](#9-stage-4--order-lifecycle)
10. [Startup Recovery](#10-startup-recovery)
11. [Indicatorها](#11-indicatorها)
12. [Strategyها](#12-strategyها)
13. [Trading Schedule (ساعت معاملاتی)](#13-trading-schedule-ساعت-معاملاتی)
14. [Currency Transformation](#14-currency-transformation)
15. [Persistence برای AI/ML](#15-persistence-برای-aiml)
16. [پیکربندی و DI](#16-پیکربندی-و-di)
17. [جریان یک Tick کامل](#17-جریان-یک-tick-کامل)
18. [Lifecycle یک معامله](#18-lifecycle-یک-معامله)
19. [Recovery Scenarios](#19-recovery-scenarios)
20. [نکات مهم و محدودیت‌ها](#20-نکات-مهم-و-محدودیت‌ها)

---

## 1. معرفی کلی

Trading Module یک **موتور معاملات الگوریتمی** است که داخل `NexusPlatform` به عنوان یک Module مستقل عمل می‌کند.

### هدف

اتصال به کارگزاری → دریافت داده‌ی بازار → اجرای استراتژی → تولید سیگنال → ثبت سفارش → مدیریت موقعیت.

### اصول معماری

| اصل | توضیح |
|---|---|
| **Strategy از Account جداست** | Strategy فقط تحلیل می‌کند، نمی‌داند چند حساب دارد |
| **Market Data از Execution جداست** | یک منبع داده برای همه‌ی Runها |
| **TradingRun مسئول Execution Context است** | تنظیمات Capital، SL/TP، Accountها در Run |
| **Broker پشت Abstraction** | Mofid/Agah/سایر از طریق `IBroker*` Interface |
| **Future-ready, Simple Phase 1** | Extensible بدون Overengineering |

### چرخه‌ی کلی

```
Broker API
    │
    ▼
Market Data Ingestion (Stage 1)
    │
    ▼
Strategy Evaluation (Stage 2) ──→ Indicator (RSI، MACD، ...)
    │
    ▼
Signal (تولید شده)
    │
    ▼
Execution (Stage 3) ──→ OrderIntent ──→ BrokerOrder
    │                                        │
    │                                        ▼
    │                                   Broker API
    │                                        │
    ▼                                        ▼
Order Lifecycle (Stage 4) ──→ OrderTrade ──→ Position
    │
    ▼
Position Events (PnL، Close، ...)
```

---

## 2. مفاهیم پایه

| مفهوم | تعریف |
|---|---|
| **Instrument** | هویت نماد (فولاد، شپنا، ...). ISIN یکتا. |
| **TraderAccount** | حساب کارگزاری. BrokerType + Username + Session. |
| **TradingSchedule** | ساعت معاملاتی نماد (سهام عادی ۹-۱۲:۳۰، عیار ۱۲:۳۰-۱۸، ...) |
| **TradingRun** | یک "پلن". یک Strategy + چند Symbol + چند Account + تنظیمات Execution. |
| **Strategy** | منطق تولید Signal. هیچ I/O. |
| **Indicator** | تابع محاسباتی خالص (RSI، MACD). بدون I/O، بدون Strategy. |
| **Signal** | خروجی Strategy: `Action (Buy/Sell) + Strength`. بدون قیمت، بدون مقدار. |
| **OrderIntent** | تصمیم اجرا برای یک Account خاص. شامل Price، Quantity، SL/TP. |
| **BrokerOrder** | سفارش واقعی که به Broker فرستاده شد. |
| **OrderTrade** | یک Fill (بخشی) از BrokerOrder. |
| **Position** | موقعیت باز یک (Run, Account, Isin). |
| **MarketSnapshot** | قیمت لحظه‌ای از Broker. |
| **Candle** | OHLCV یک بازه‌ی زمانی. |

---

## 3. نقشه‌ی معماری

### لایه‌ها

```
┌─────────────────────────────────────────────────────┐
│  Presentation (Controllers + MediatR)                │
└─────────────────────────────────────────────────────┘
                         │
┌────────────────────────▼─────────────────────────────┐
│  Application (Commands, Queries, Interfaces, DTOs)   │
│  - IStrategy, IIndicator, IBrokerSessionProvider      │
│  - ITraderUnitOfWork, IMarketDataProvider             │
└─────────────────────────────────────────────────────┘
                         │
┌────────────────────────▼─────────────────────────────┐
│  Domain (Entities, Value Objects, Events)            │
│  - TradingRun, Signal, OrderIntent, Position          │
│  - CandleRecord, MarketSnapshotRecord, CurrencyRate   │
└─────────────────────────────────────────────────────┘
                         │
┌────────────────────────▼─────────────────────────────┐
│  Infrastructure (EF, Brokers, Runtime, Stages)       │
│  - TraderDbContext, TraderUnitOfWork                  │
│  - EasyTrader Adapter                                 │
│  - TradingRuntimeWorker + Stages 1-4                  │
└─────────────────────────────────────────────────────┘
```

### External Dependencies

```
EasyTrader API (Broker)
    │
    │  HTTP/JSON
    ▼
IBrokerMarketDataReader ──→ MarketSnapshot / Candle
IBrokerTradingCommands  ──→ Order Submission
IBrokerOrderHistoryReader ──→ Order Status / Trades
IBrokerAccountReader    ──→ Cash Balance
```

---

## 4. Data Model

### جداول اصلی (Schema: `trader`)

| جدول | کاربرد | حجم/روز (۱ نماد M5) |
|---|---|---|
| `Instruments` | نمادها | ثابت |
| `TraderAccounts` | حساب‌ها | ثابت |
| `TradingSchedules` | ساعت معاملاتی | ۴-۱۰ ردیف ثابت |
| `TradingRuns` | پلن‌ها | کم |
| `Signals` | سیگنال‌های تولیدشده | ~۰-۱۰ |
| `OrderIntents` | تصمیم‌های اجرا | ~۰-۱۰ |
| `BrokerOrders` | سفارش‌های واقعی | ~۰-۱۰ |
| `OrderTrades` | Fillها | ~۰-۳۰ |
| `Positions` | موقعیت‌ها | ~۰-۱۰ |
| `MarketSnapshots` | قیمت لحظه‌ای | ~۷۰۰-۳۰۰۰ (با Market Hours) |
| `StrategyEvaluations` | هر Evaluate | ~۷۰۰-۳۰۰۰ |
| `Candles` | کندل‌های تاریخی | ۵-۱۰ (M5) |
| `CurrencyRates` | نرخ ارز | کم |

### روابط کلیدی

```
TradingRun ─┬── TradingRunSymbol (N) ── ISIN
            └── TradingRunAccount (N) ── TraderAccountId

TradingRun ──→ Signal (N) ──→ OrderIntent (N) ──→ BrokerOrder (N) ──→ OrderTrade (N)
                                                                          │
                                                                          ▼
                                                                      Position
Signal ──→ StrategyEvaluation (1:1)
OrderIntent ──→ StrategyEvaluation (N:1)

Instrument ──→ TradingSchedule (N:1)
```

---

## 5. Runtime Worker و Stages

### `TradingRuntimeWorker`

یک `BackgroundService` که در Startup برنامه اجرا می‌شود و تا Stop برنامه فعال است.

```
┌────────────────────────────────────────────┐
│  TradingRuntimeWorker                       │
│                                             │
│  1. Startup: RuntimeRecoveryService         │
│  2. Loop (هر 10 ثانیه):                     │
│     - Scope جدید                            │
│     - Stage 1 (Market Data)                 │
│     - Stage 2 (Strategy Evaluation)         │
│     - Stage 3 (Execution)                   │
│     - Stage 4 (Order Lifecycle, هر 5 ثانیه) │
│     - Scope Dispose                         │
└────────────────────────────────────────────┘
```

### چرا Scope جدید در هر Tick؟

- Stageها Scoped هستند (به UoW و DbContext وابسته).
- Scope per Tick → DbContext تازه → Change Tracker تمیز.
- خطای یک Tick، بقیه را نمی‌کشد.

### PeriodicTimer vs Task.Delay

`PeriodicTimer` استفاده می‌شود که Drift ندارد. اگر Tick ۲ ثانیه طول بکشد و Interval ۱۰ ثانیه باشد، Tick بعدی در ثانیه ۱۲ شروع می‌شود (نه ۱۴).

### پیکربندی

```json
"Trader": {
  "Runtime": {
    "HeartbeatInterval": "00:00:10",
    "OrderLifecycleInterval": "00:00:05",
    "SignalChannelCapacity": 1000,
    "OrderPollBatchSize": 100
  }
}
```

---

## 6. Stage 1 — Market Data Ingestion

### مسئولیت

دریافت قیمت لحظه‌ای همه‌ی Symbols از Runهای فعال، ذخیره در Cache (حافظه) و DB.

### جریان

```
RunCache.GetAllActive()
    │
    ▼
اتحاد Symbols از همه‌ی Runها (deduplicate)
    │
    ▼
BrokerMarketDataProvider.GetBatchSnapshotsAsync
    │
    ├─→ Market Hours Check per Symbol (با TradingSchedule)
    ├─→ Session از BrokerSessionProvider
    ├─→ Batch API Call به Broker
    │
    ▼
Map به MarketSnapshot
    │
    ▼
Persist به DB (MarketSnapshots table)
    │
    ▼
Update InMemoryMarketSnapshotStore (with Ids)
```

### Deduplication

اگر Run A و Run B هر دو `فولاد` را داشته باشند، فقط **یک بار** از Broker Fetch می‌شود. این در `MarketDataIngestionStage` مدیریت می‌شود:

```csharp
var isins = contexts
    .SelectMany(c => c.Symbols)
    .Select(s => s.Isin)
    .Distinct(StringComparer.OrdinalIgnoreCase)
    .ToList();
```

### Market Hours Check

اگر همه‌ی Symbols بسته باشند، Broker Call زده نمی‌شود:

```csharp
foreach (var isin in isins)
{
    var schedule = await _scheduleCache.GetForIsinAsync(isin, ct);
    if (_marketSession.IsMarketOpen(schedule, utcNow))
        openIsins.Add(isin);
}

if (openIsins.Count == 0)
{
    _logger.LogTrace("All markets closed; skipping.");
    return;
}
```

**نتیجه:** شبانه هیچ Broker Callی زده نمی‌شود → WAF Trigger نمی‌شود.

---

## 7. Stage 2 — Strategy Evaluation

### مسئولیت

برای هر (Run × Symbol): اجرای Strategy، ذخیره‌ی Evaluation، تولید Signal در صورت وجود.

### جریان

```
for each Run in RunCache.GetAllActive():
    │
    ├─→ Set User DataContext (برای Authorization)
    │
    └─→ for each Symbol in Run:
            │
            ├─→ Snapshot از InMemoryMarketSnapshotStore
            ├─→ Candle از InMemoryCandleCache
            │   ├─→ Cache hit? (TTL بر اساس کندل بعدی)
            │   └─→ Cache miss → Fetch → Persist → Cache
            ├─→ فیلتر Closed Candles (اگر Strategy.RequiresClosedCandlesOnly)
            ├─→ Currency Transform (اگر Run.TargetCurrency != null)
            ├─→ Strategy.Evaluate(input)
            │   ├─→ Indicator.Compute (RSI)
            │   └─→ منطق تصمیم (BUY/SELL/HOLD)
            ├─→ ساخت StrategyEvaluation (DB record)
            ├─→ اگر Signal تولید شد: Signal.Create + Channel Push
            │
            ▼
    SaveChanges یک بار در پایان همه‌ی Runها
    │
    ▼
Push همه‌ی Signals به Channel
```

### Candle Cache Logic

```csharp
// Cache hit شرط
if (cache.HasCandles && cache.Count >= required && now < cache.ExpiresAt)
    return cache.Candles;

// ExpiresAt محاسبه:
if (MarketClosed)
    expiresAt = GetNextMarketOpen() + buffer;
else
    expiresAt = (lastCandleTime + tfDuration) + buffer;
```

**نتیجه:**
- بازار باز: هر کندل جدید (M1: هر ۷۵ ثانیه، M5: هر ۵.۵ دقیقه، ...)
- بازار بسته: تا باز شدن بعدی

### Candle Persistence

فقط کندل‌های **بسته‌شده** در DB ذخیره می‌شوند:

```csharp
var closedCandles = candles
    .Where(c => c.OpenTime + tfDuration <= now)
    .ToList();
```

**چرا:** کندل in-progress ممکن است تا بسته شدن OHLC/Volume‌اش تغییر کند. ذخیره‌اش در DB باعث dedup اشتباه می‌شود.

### Signal Pipeline

```
StrategyEvaluationResult
    │
    ├─── Features (rsi، period، ...) → StrategyEvaluation.FeaturesJson
    │
    └─── Signals[]
            │
            ├─── Signal.Create
            ├─── AddDomainEvent(SignalGeneratedEvent)
            ├─── Signal.SourceEvaluationId = evalEntity.Id
            │
            ▼
        Channel<SignalEnvelope> (in-memory)
```

**نکته:** SaveChanges **قبل** از Push به Channel انجام می‌شود. اگر Process بین Save و Push بمیرد، Signal در DB هست ولی Stage 3 آن را ندیده — Recovery در Startup آن را می‌گیرد.

---

## 8. Stage 3 — Execution

### مسئولیت

مصرف Signalها از Channel، ساخت OrderIntent، ارسال به Broker.

### جریان

```
Channel<SignalEnvelope> (drain، بدون Blocking)
    │
    ▼
for each SignalEnvelope:
    │
    ├─→ Get Run Context از Cache (اگر نبود، Skip)
    ├─→ Set User DataContext
    │
    └─→ for each Account in Run.Accounts:
            │
            ├─→ OrderIntentBuilder.BuildAsync
            │   ├─→ Snapshot از Store (قیمت ریالی، نه converted)
            │   ├─→ SymbolAllocation از Run.Symbols
            │   ├─→ Capital Allocator (Fixed یا Dynamic)
            │   ├─→ PositionSizer.CalculateQuantity
            │   ├─→ Resolve SL/TP از Run.Settings
            │   ├─→ OrderIntent.Create (قیمت ریالی، qty، SL، TP)
            │   └─→ SourceEvaluationId (از Signal)
            │
            ├─→ BrokerSessionProvider.GetSessionAsync
            │
            ├─→ BrokerOrderDispatcher.DispatchAsync
            │   ├─→ BrokerOrder.CreatePending
            │   ├─→ intent.MarkDispatched
            │   ├─→ Broker API: SendBuyOrder / SendSellOrder
            │   ├─→ اگر موفق: BrokerOrder.AssignBrokerOrderId + intent.MarkAcknowledged
            │   └─→ اگر Fail: intent.MarkRejected/Failed
            │
            ▼
        SaveChanges یک بار در پایان Handle
```

### Capital Allocation

| Mode | منبع |
|---|---|
| **Fixed** | `Run.CapitalAllocation.FixedAmount` (مقدار ثابت) |
| **Dynamic** | `Broker.GetCashBalanceAsync().BuyPowerT0` (با Cache ۵ ثانیه) |

### Position Sizing

```csharp
// PercentPositionSizer
var price = ResolvePrice(snapshot, action);
var effectiveCapital = allocatedCapital * (1 - commissionRate);
var qty = floor(effectiveCapital / price);
```

### قیمت سفارش — **ریالی، نه تبدیل‌شده**

**مهم:** حتی اگر Run.TargetCurrency = "USD" باشد، `OrderIntent.Price` از Snapshot ریالی می‌آید:

```csharp
var snapshot = _snapshotStore.GetLatest(signal.Isin);  // ← ریالی
var price = ResolvePrice(snapshot, signal.Action);      // ← ریالی
```

**دلیل:** Broker فقط ریال قبول می‌کند. Currency Transformation فقط برای تحلیل (Candles) است.

### Idempotency

- Signal.MarkConsumed() بعد از Handle
- در Recovery، Signalهای Consumed‌نشده بررسی می‌شوند

---

## 9. Stage 4 — Order Lifecycle

### مسئولیت

Poll سفارش‌های باز از Broker، Sync Trades، اعمال روی Position، SL/TP monitoring.

### جریان

```
if (now - _lastRunAt < OrderLifecycleInterval) return;
_lastRunAt = now;
    │
    ▼
group Runهای فعال بر اساس Account
    │
    ▼
for each Account:
    │
    ├─→ Set User DataContext
    │
    ├─→ SyncAccountAsync
    │   ├─→ Orderهای باز ما (از DB)
    │   ├─→ Fetch Order History از Broker (Pagination)
    │   │
    │   └─→ for each باز Order:
    │           ├─→ SyncOrderAsync
    │           │   ├─→ BrokerOrder.ApplyStateUpdate
    │           │   └─→ اگر ExecutedQuantity افزایش یافته:
    │           │           └─→ SyncTradesAsync
    │           │               ├─→ Fetch Trades از Broker
    │           │               ├─→ for each Trade:
    │           │               │   ├─→ اگر جدید: OrderTrade.Create + AddAsync
    │           │               │   ├─→ PositionManager.ApplyTradeAsync
    │           │               │   │   ├─→ اگر Buy + Position نیست: Position.Open
    │           │               │   │   ├─→ اگر Buy + Position هست: ApplyBuyFill
    │           │               │   │   └─→ اگر Sell: ApplySellFill
    │           │               │   │
    │           │               │   └─→ اگر Position بسته شد:
    │           │               │       └─→ UpdateEvaluationOutcomeAsync
    │           │               │
    │           │               └─→ Tradeهای Cancelled: ApplyTradeCancelAsync
    │           │
    │           └─→ CheckStopsAndCloseAsync
    │               ├─→ for each Position باز:
    │               │   ├─→ Snapshot.GetLatest
    │               │   ├─→ ShouldTriggerStopLoss/TakeProfit?
    │               │   └─→ اگر بله: Sell OrderIntent + Dispatch
    │               │
    │               ▼
    │           SaveChanges
```

### Trade Idempotency

هر `OrderTrade` با `TradeNumber` (یکتای بورس) در DB ذخیره می‌شود:

```csharp
var existingTrades = await _uow.OrderTradeSpecs.ListBySpecAsync(spec);
var existingByNumber = existingTrades.ToDictionary(t => t.TradeNumber);

foreach (var dto in brokerTrades)
{
    if (existingByNumber.ContainsKey(dto.TradeNumber)) continue;  // Skip
    // Add new trade
}
```

### Position Opening — SL/TP از OrderIntent

```csharp
if (order.OrderIntentId.HasValue)
{
    var intent = await _uow.OrderIntents.GetByIdAsync(order.OrderIntentId.Value);
    sl = intent?.StopLossPrice;
    tp = intent?.TakeProfitPrice;
    sourceEvalId = intent?.SourceEvaluationId;
}

position = Position.Open(..., stopLossPrice: sl, takeProfitPrice: tp,
                         sourceEvaluationId: sourceEvalId);
```

### Outcome Tracking

وقتی Position بسته شد:

```csharp
if (position.State == PositionState.Closed && position.SourceEvaluationId.HasValue)
{
    var eval = await _uow.StrategyEvaluations.GetByIdAsync(
        position.SourceEvaluationId.Value);

    var outcome = new
    {
        entryPrice = position.AverageEntryPrice.Value,
        exitPrice = closingTrade.Price.Value,
        realizedPnl = position.RealizedPnL.Amount,
        returnPct = (exitPrice - entryPrice) / entryPrice * 100,
        exitReason = DetectExitReason(position, closingOrder),  // StopLoss/TakeProfit/Manual
        heldSeconds,
        openedAt, closedAt
    };

    eval.UpdateOutcome(JsonSerializer.Serialize(outcome));
}
```

**این داده برای AI/ML آینده حیاتی است.** ✅

---

## 10. Startup Recovery

### چه چیزی Recovery می‌کند؟

در Startup بعد از Restart:

```
RuntimeRecoveryService.RecoverAsync()
    │
    ├─→ Load Runهای State=Running از DB
    │       └─→ برای هر Run: TradingRunManager.StartAsync
    │           └─→ Context cache می‌شود
    │
    └─→ Load Signalهای ConsumedAt=null برای Runهای فعال
            │
            ├─→ اگر هیچ OrderIntent ندارد → Re-Push به Channel (سناریو B)
            │
            └─→ اگر OrderIntent دارد → MarkConsumed + Log Warning (سناریو C/D)
```

### سناریوها

| سناریو | وضعیت DB | اقدام |
|---|---|---|
| A: Crash قبل از Save | Signal نیست | هیچ |
| B: Crash بین Save و Push | Signal ذخیره، Consumed=null | Re-Push |
| C: Crash وسط Stage 3 | Signal + شاید OrderIntent | Mark Consumed (جلوگیری از Duplicate) |
| D: Crash بعد از Save | Signal + OrderIntent | Mark Consumed |
| E: Crash بعد از Stop | Run=Stopped | Stage 4 در Tick بعدی Sync |

**فلسفه:** Duplicate Order فاجعه است. Missing Order یک Event نادر است که در Tick بعد جبران می‌شود.

---

## 11. Indicatorها

### Abstraction

```csharp
public interface IIndicator<TConfig, TResult> where TConfig : class
{
    string Key { get; }
    string DisplayName { get; }
    TResult Compute(IndicatorInput input, TConfig config);
}

public sealed record IndicatorInput(
    IReadOnlyList<Candle> Candles,
    MarketSnapshot LatestSnapshot,
    TimeFrame TimeFrame,
    IReadOnlyDictionary<string, object?>? TransformationContext = null);
```

### RSI Structure

```
RsiCalculator (pure math)
    │
    │  input: IReadOnlyList<decimal> closes, int period
    │  output: decimal?
    │
    ▼
RsiIndicator (adapter)
    │
    │  input: IndicatorInput, RsiConfig
    │  - closes از Candles.Select(c => c.Close)
    │  - calls RsiCalculator
    │
    ▼
RsiStrategy (decision logic)
    │
    │  - RSI < Oversold → BUY با strength
    │  - RSI > Overbought → SELL با strength
    │  - else → HOLD
```

**چرا جدا:**
- RsiCalculator می‌تواند در AI Pipeline استفاده شود
- RsiStrategy و MacdRsiStrategy می‌توانند همان RsiIndicator را استفاده کنند

### افزودن Indicator جدید

1. ساخت `MacdConfig` (تنظیمات)
2. ساخت `MacdIndicator : IIndicator<MacdConfig, MacdResult>`
3. ثبت در DI: `services.AddSingleton<IIndicator<MacdConfig, MacdResult>, MacdIndicator>()`

---

## 12. Strategyها

### Abstraction

```csharp
public interface IStrategy
{
    string Key { get; }
    string DisplayName { get; }
    int RequiredCandleCount { get; }
    bool RequiresClosedCandlesOnly { get; }
    StrategyEvaluationResult Evaluate(StrategyInput input);
}
```

### StrategyInput / Result

```csharp
public sealed record StrategyInput(
    string Isin,
    MarketSnapshot LatestSnapshot,      // ← ریالی
    IReadOnlyList<Candle> Candles,      // ← ممکن است Converted باشد
    StrategyConfig Configuration,
    TimeFrame TimeFrame,
    DateTime EvaluatedAt);

public sealed record StrategyEvaluationResult(
    IReadOnlyList<StrategySignal> Signals,
    IReadOnlyDictionary<string, object?>? Features = null);
```

### Registration

```csharp
public sealed class RsiStrategyRegistration
    : StrategyRegistration<RsiStrategyConfig>
{
    public override string Key => "rsi";
    public override string DisplayName => "RSI";
    protected override IStrategy CreateStrategy(RsiStrategyConfig config)
        => new RsiStrategy(_rsiIndicator, config);
}
```

ثبت در DI:

```csharp
services.AddTraderStrategies();  // Assembly scanning
```

### افزودن Strategy جدید

1. `Create` config class (از `StrategyConfig`)
2. `Create` strategy class (پیاده‌سازی `IStrategy`)
3. `Create` registration class (از `StrategyRegistration<TConfig>`)
4. Assembly scanning خودکار پیدا می‌کند — **نیازی به DI خط نیست**

---

## 13. Trading Schedule (ساعت معاملاتی)

### چرا؟

ساعت معاملاتی نماد به نماد متفاوت است:

- سهام بورس تهران: 09:00 - 12:30
- صندوق اهرمی: 09:00 - 12:30
- گواهی عیار: 12:30 - 18:00
- اختیار معامله: 09:00 - 15:00

### Data Model

```
TradingSchedule
    ├── Code: "TSE-REGULAR"
    ├── PreMarketStart: 08:45
    ├── SessionStart: 09:00
    ├── SessionEnd: 12:30
    ├── AfternoonStart?: optional
    ├── AfternoonEnd?: optional
    └── AfterHoursEnd?: optional (عیار: 18:00)

Instrument
    └── TradingScheduleId: Guid? (nullable)
```

### Fallback

اگر `Instrument.TradingScheduleId = null` → `TSE-REGULAR` استفاده می‌شود (از `TradingScheduleCache`).

### کجا استفاده می‌شود؟

**Stage 1:**
```csharp
if (!_marketSession.IsMarketOpen(schedule, utcNow))
    return Array.Empty<MarketSnapshot>();
```

**Candle Cache Expiry:**
```csharp
if (MarketClosed)
    expiresAt = GetNextMarketOpen() + buffer;
```

**نتیجه:** بدون Market Hours، شبانه هزاران Call به Broker → WAF Block. با آن، صفر Call.

---

## 14. Currency Transformation

### چرا؟

تحلیل سهم در ارز دیگر مفید است — RSI روی USD نشان می‌دهد سهم واقعاً رشد کرده یا فقط ریال تضعیف شده.

### Data Model

```
TradingRun.TargetCurrency: string? ("USD", "EUR", null)

CurrencyRate (Time-series table)
    ├── FromCurrency: "USD"
    ├── ToCurrency: "IRR"
    ├── Rate: 650000
    └── EffectiveAt: timestamp
```

### Latest Rate Lookup

```sql
-- LatestCurrencyRateSpec
SELECT TOP 1 *
FROM trader.CurrencyRates
WHERE FromCurrency = 'USD' AND ToCurrency = 'IRR'
  AND EffectiveAt <= @atTime
ORDER BY EffectiveAt DESC
```

**نکته:** صبح نرخ ثبت می‌شود، ظهر همان نرخ استفاده می‌شود تا نرخ جدید ثبت شود.

### Transformation

در Stage 2، **قبل از** Strategy.Evaluate:

```csharp
if (!string.IsNullOrWhiteSpace(ctx.TargetCurrency) && candles.Count > 0)
{
    var transformed = await _candleTransformer.TransformAsync(
        candles, ctx.TargetCurrency, now, ct);
    candles = transformed;
}
```

**`CurrencyCandleTransformer`:**
```csharp
var rate = await _rateProvider.GetRateAsync("IRR", target, atTime);
foreach (var c in irrCandles)
    result.Add(c with { Open = c.Open * rate, ..., Close = c.Close * rate });
```

### تصمیمات کلیدی

| بخش | تبدیل می‌شود؟ |
|---|---|
| Candles (برای Strategy) | ✅ بله |
| MarketSnapshot (برای Sizing/Execution) | ❌ نه — ریالی |
| OrderIntent.Price | ❌ نه — ریالی |
| Position.AverageEntryPrice | ❌ نه — ریالی |
| StrategyEvaluation.FeaturesJson["lastClose"] | ✅ بله (transformed) |
| StrategyEvaluation.FeaturesJson["priceAtEvalIrr"] | ✅ جدا ذخیره می‌شود |

**نتیجه:** تحلیل در USD، سفارش در IRR.

### افزودن Rate جدید

```
POST /api/Trader/CurrencyRate/Create
{
  "fromCurrency": "USD",
  "toCurrency": "IRR",
  "rate": 650000,
  "effectiveAt": "2026-10-26T06:00:00Z",
  "source": "Manual"
}
```

یا از صفحه‌ی `/trader/currency-rates` در Frontend.

---

## 15. Persistence برای AI/ML

### سه جدول کلیدی

#### 1) `MarketSnapshots`

هر Snapshot بازار، به همراه همه‌ی فیلدهای Order Book:

```sql
SELECT * FROM trader.MarketSnapshots
WHERE Isin = 'IRO1FOLD0001'
  AND CapturedAt BETWEEN @from AND @to;
```

**کاربرد:** Historical price data با Order Book depth، برای Strategyهای OrderBook و Backtesting.

#### 2) `StrategyEvaluations`

هر Evaluate، حتی Hold:

```json
{
  "id": "...",
  "evaluatedAt": "2026-10-26T08:30:00Z",
  "isin": "IRO1FOLD0001",
  "strategyKey": "rsi",
  "marketSnapshotId": "snapshot-guid",
  "featuresJson": "{\"rsi\":45.5,\"period\":14,\"targetCurrency\":\"USD\",...}",
  "action": 0,  // Hold/Buy/Sell
  "strength": 0,
  "signalId": null,
  "outcomeJson": null
}
```

**کاربرد:** Training Dataset — هر ردیف یک نمونه با Features (ورودی) و Action (برچسب فعلی). بعد از بسته شدن Position، `outcomeJson` هم پر می‌شود:

```json
{
  "entryPrice": 50000,
  "exitPrice": 55000,
  "returnPct": 10.0,
  "exitReason": "TakeProfit",
  "heldSeconds": 3600
}
```

**با این داده می‌توان:**
- مدل Supervised Learning آموزش داد: `Features → returnPct > 0?`
- استراتژی‌های فعلی را Backtest کرد
- Thresholdهای فعلی را Tune کرد

#### 3) `Candles`

```sql
SELECT * FROM trader.Candles
WHERE Isin = 'IRO1FOLD0001' AND TimeFrame = 1440
ORDER BY OpenTime DESC LIMIT 100;
```

**کاربرد:** Historical Ground Truth (اگر Broker API عوض شد، داده موجود است). Backtesting.

### Dataset Query برای ML

```sql
SELECT
    e.EvaluatedAt,
    e.Isin,
    e.StrategyKey,
    e.FeaturesJson,
    e.Action,
    e.Strength,
    e.OutcomeJson,
    JSON_VALUE(e.OutcomeJson, '$.returnPct') AS ReturnPct,
    JSON_VALUE(e.OutcomeJson, '$.exitReason') AS ExitReason
FROM trader.StrategyEvaluations e
WHERE e.Action <> 0                     -- فقط Buy/Sell
  AND e.OutcomeJson IS NOT NULL          -- فقط ردیف‌های با نتیجه
ORDER BY e.EvaluatedAt;
```

**این Dataset آماده‌ی استفاده در Python (pandas + scikit-learn) است.**

---

## 16. پیکربندی و DI

### Config Sections

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "..."
  },
  "Brokers": {
    "EasyTrader": {
      "BaseUrl": "https://api-mts.orbis.easytrader.ir",
      "ClientId": "easy_pkce",
      "RedirectUri": "https://d.easytrader.ir/auth-callback"
    }
  },
  "Trader": {
    "Runtime": {
      "HeartbeatInterval": "00:00:10",
      "OrderLifecycleInterval": "00:00:05",
      "SignalChannelCapacity": 1000
    }
  }
}
```

### DI Registration (چکیده)

```csharp
services.AddTraderModule(configuration);
services.Trader_AddPresentation(configuration);
```

**Lifetimeها:**

| Service | Lifetime | دلیل |
|---|---|---|
| `TradingRuntimeWorker` | Hosted | Background |
| `IRunExecutionContextCache` | Singleton | State مشترک |
| `IBrokerSessionCache` | Singleton | Session Cache |
| `IMarketSnapshotStore` | Singleton | In-memory prices |
| `ICandleCache` | Singleton | Candle Cache |
| `ISignalChannel` | Singleton | Stage 2 ↔ 3 |
| `ITradingScheduleCache` | Singleton | Schedule Cache |
| `CurrencyRateCache` | Singleton | Rate Cache |
| `ITraderUnitOfWork` | Scoped | EF DbContext |
| Stageها (1-4) | Scoped | DbContext + UoW |
| `TradingRunManager` | Scoped | UoW |
| Strategy Factory | Singleton | Stateless |
| Indicator | Singleton | Stateless |

---

## 17. جریان یک Tick کامل

### فرض: بازار باز است، یک Run فعال با RSI/فولاد M5

```
T=08:30:00  Worker Tick #1 (Cold Start)
    │
    ├─→ Stage 1: Fetch Snapshot فولاد (قیمت=50000)
    │       └─→ Save MarketSnapshot در DB
    │       └─→ SnapshotStore.Upsert
    │
    ├─→ Stage 2:
    │       ├─→ CandleCache miss → Fetch 64 Candle M5 از Broker (~۳۸ دقیقه اول)
    │       ├─→ Persist Candleهای closed به DB
    │       ├─→ RSI = 45 (خنثی) → HOLD
    │       ├─→ StrategyEvaluation ذخیره (با FeaturesJson)
    │       └─→ 0 Signal → Channel خالی
    │
    ├─→ Stage 3: Channel خالی → هیچ کاری
    │
    └─→ Stage 4: هیچ Order باز → هیچ کاری
    │
    ▼
T=08:30:10  Worker Tick #2
    │
    ├─→ Stage 1: Fetch Snapshot
    ├─→ Stage 2: Candle Cache hit (معتبر تا 08:35:30) → RSI محاسبه (45) → HOLD
    └─→ ...
    │
    ▼
T=08:35:30  Worker Tick N
    │
    ├─→ Stage 2: Cache expired (کندل 08:35 بسته شد) → Fetch → RSI = 28
    │       └─→ Signal BUY (strength=0.6)
    │       └─→ SaveChanges (Evaluation + Signal)
    │       └─→ Channel Push
    │
    ├─→ Stage 3:
    │       ├─→ Consume Signal
    │       ├─→ Capital = 100M ریال
    │       ├─→ Allocated = 100M × 100% = 100M
    │       ├─→ Price = 50000 (از Snapshot ریالی)
    │       ├─→ Quantity = floor(100M / 50000) = 2000
    │       ├─→ SL = 50000 × 0.8 = 40000
    │       ├─→ TP = 50000 × 1.5 = 75000
    │       ├─→ OrderIntent + BrokerOrder.CreatePending
    │       └─→ Broker API: SendBuyOrder → BrokerOrderId=abc123
    │
    └─→ Stage 4: Order باز نیست در DB (همین الان ساخته شد)
            └─→ next Tick
    │
    ▼
T=08:35:35  Worker Tick N+1
    │
    └─→ Stage 4:
            ├─→ Fetch Order History از Broker
            ├─→ Order abc123: State=Open, Executed=0 → هیچ
    │
    ▼
T=08:40:00  Worker Tick M
    │
    └─→ Stage 4:
            ├─→ Fetch Order History
            ├─→ Order abc123: State=PartiallyExecuted, Executed=500
            ├─→ Fetch Trades → Trade #1 (qty=500, price=50000)
            ├─→ OrderTrade.Create
            ├─→ PositionManager.ApplyTradeAsync:
            │       ├─→ Buy + Position نیست → Position.Open
            │       │   qty=500, entryPrice=50000, SL=40000, TP=75000
            │       └─→ PositionChangedEvent
    │
    ▼
T=10:15:00  Worker Tick ...
    │
    └─→ Stage 4:
            ├─→ Snapshot: price=76000 (> TP=75000)
            ├─→ CheckStopsAndCloseAsync:
            │       ├─→ ShouldTriggerTakeProfit → true
            │       ├─→ Sell OrderIntent (qty=500, price=76000)
            │       ├─→ Dispatch → BrokerOrder
            │       └─→ Reason: "TakeProfit triggered"
    │
    ▼
T=10:15:30  Worker Tick ...
    │
    └─→ Stage 4:
            ├─→ Fetch Order History → Sell order filled
            ├─→ OrderTrade (Sell, qty=500, price=76000)
            ├─→ ApplySellFill:
            │       ├─→ Position.Quantity = 0
            │       ├─→ State = Closed
            │       └─→ RealizedPnL = (76000-50000) × 500 = 13M ریال
            ├─→ UpdateEvaluationOutcomeAsync:
            │       └─→ StrategyEvaluation.OutcomeJson = {
            │             "entryPrice": 50000,
            │             "exitPrice": 76000,
            │             "returnPct": 52.0,
            │             "exitReason": "TakeProfit",
            │             "heldSeconds": 6000
            │           }
            └─→ ✅ دیتای کامل برای AI/ML
```

---

## 18. Lifecycle یک معامله

```
[1] Signal
    └─→ Action=Buy, Strength=0.6, SourceEvaluationId
    │
    ▼
[2] OrderIntent
    └─→ Run, Account, Isin, Price, Quantity, SL, TP
    └─→ State: Pending → Dispatched → Acknowledged
    │
    ▼
[3] BrokerOrder
    └─→ BrokerOrderId, State: Pending → Open → PartiallyFilled → Filled
    └─→ SubmittedAt, LastSyncedAt
    │
    ▼
[4] OrderTrade (multiple)
    └─→ TradeNumber (unique), Price, Quantity, TradeDate
    └─→ IsCanceled (for cancellations)
    │
    ▼
[5] Position (accumulated from trades)
    └─→ State: Open → PartiallyClosed → Closed
    └─→ AverageEntryPrice, Quantity, RealizedPnL
    └─→ StopLossPrice, TakeProfitPrice
    │
    ▼
[6] Outcome (when closed)
    └─→ Stored in StrategyEvaluation.OutcomeJson
    └─→ Ready for ML training
```

---

## 19. Recovery Scenarios

### Scenario A: Worker Crash قبل از Stage 2 Save

```
State: هیچ Signal در DB
Action: هیچ کاری — Tick بعدی عادی
```

### Scenario B: Crash بین Stage 2 Save و Channel Push

```
State: Signal در DB با ConsumedAt=null
Action: Recovery Service → Re-Push به Channel
        Stage 3 آن را می‌بیند و اجرا می‌کند
```

### Scenario C: Crash وسط Stage 3 (بعد از Broker Call، قبل از Save)

```
State: Signal با ConsumedAt=null، OrderIntent شاید ساخته شده
       سفارش شاید در Broker ثبت شده
Action: Recovery → Signal را MarkConsumed (جلوگیری از Duplicate)
        ⚠️ Risk: سفارش در Broker هست ولی در DB نیست
        Mitigation: Broker Reconciliation (Phase 2)
```

### Scenario D: Crash بعد از Stage 3 Save، قبل از MarkConsumed

```
State: Signal ConsumedAt=null، OrderIntent + BrokerOrder موجود
Action: Recovery → Signal را MarkConsumed
        Stage 3 دوباره اجرا نمی‌شود
```

### Scenario E: Worker Restart بعد از User Stop

```
State: Run State=Stopped، Positionهای باز شاید وجود دارند
Action: Recovery → Run را فعال نمی‌کند
        Stage 4 (در Tick بعدی) positionهای باز را Sync می‌کند
        ولی سفارش جدید نمی‌فرستد
```

### Scenario F: Run در DB Running ولی Cache خالی (Restart)

```
State: Run State=Running، Cache خالی
Action: Recovery → TradingRunManager.StartAsync
        Context ساخته و Cache می‌شود
        از Tick بعدی Stage 1-4 کار می‌کنند
```

---

## 20. نکات مهم و محدودیت‌ها

### آنچه در Phase 1 پیاده شد

✅ Domain Model کامل (Instrument، TradingRun، Signal، Order، Position)
✅ Multi-Broker با Capability Segregation
✅ Session Management (Auto-Relogin)
✅ Market Data با Deduplication
✅ Candle Cache هوشمند (بسته شدن کندل)
✅ Strategy Framework + RSI
✅ Indicator Abstraction (Jدا از Strategy)
✅ TradingSchedule per Instrument
✅ Currency Transformation (فقط Candles، نه Orders)
✅ Signal → OrderIntent → BrokerOrder → OrderTrade → Position
✅ SL/TP Monitoring با Auto-Close
✅ Recovery در Startup
✅ Persistence برای AI/ML (Snapshot، Evaluation، Candle)
✅ Market Hours Check (WAF protection)
✅ Outbox Pattern (از NexusPlatform)

### آنچه در Phase 2 اضافه می‌شود

🔲 **Broker Reconciliation** — Position Match با Portfolio
🔲 **ClosePositions on Stop Run** (تکمیل)
🔲 **MarketCloseBehavior** (بستن موقعیت‌ها آخر روز)
🔲 **Paper Trading Mode**
🔲 **Multiple Indicators** (MACD، EMA، ATR)
🔲 **Composite Strategies** (RSI + MACD)
🔲 **Backtesting Engine**
🔲 **Frontend Dashboard** (Charts، Live Status)
🔲 **Retention Policy** (پاک‌سازی دیتای قدیمی)

### محدودیت‌های فعلی

⚠️ **H4 پشتیبانی نمی‌شود** — EasyTrader `resolution` فقط 1, 5, 15, 30, 60, 1D, 1W, 1M
⚠️ **Concurrent Login** — اگر چند Instance، Broker ممکن است قبول نکند (Phase 2: Redis)
⚠️ **Multi-Currency Pairs** — فعلاً فقط X→IRR (یا معکوس)
⚠️ **Cache Invalidation** — بعد از Update Run، Context Cache Refresh می‌شود ولی همه‌ی موارد
⚠️ **No Fill Handling** — اگر سفارش Fill نشد، در Tick بعدی دوباره بررسی می‌شود (نه Cancel)

### نکات مهم عملیاتی

🔸 **Logs:** همه‌چیز در `Logs/app-*.log` با CorrelationId
🔸 **Debug:** `Trader.Infrastructure.*` را در `appsettings.Development.json` روی Debug
🔸 **DB Queries:**
```sql
-- مشاهده Run فعال
SELECT * FROM trader.TradingRuns WHERE State = 2;

-- آخرین Signalها
SELECT TOP 20 * FROM trader.Signals ORDER BY ProducedAt DESC;

-- آخرین Evaluateها با RSI
SELECT TOP 20 EvaluatedAt, Isin,
       JSON_VALUE(FeaturesJson, '$.rsi') AS RSI,
       Action, Strength
FROM trader.StrategyEvaluations
ORDER BY EvaluatedAt DESC;

-- Positions باز
SELECT * FROM trader.Positions WHERE State <> 2;
```

### هشدارهای Production

⚠️ **قبل از پول واقعی** — با مبلغ کم تست کن (۱-۱۰ میلیون ریال)
⚠️ **Target Currency** — تست کن که Order با IRR ثبت می‌شود (نه Converted)
⚠️ **SL/TP** — یک Position باز کن و Trigger کن (مصنوعی)
⚠️ **Recovery** — برنامه را Restart کن وسط معامله، ببین چه می‌شود
⚠️ **WAF** — بعد از ساعت ۱۲:۳۵ (بسته بازار) لاگ را چک کن که WAF Error نباشد

---

## پیوست: فایل‌های کلیدی

### Domain

```
Trader.Domain/
├── Entities/
│   ├── TradingRun.cs               # Aggregate Root
│   ├── Instrument.cs               # هویت نماد
│   ├── TradingSchedule.cs          # ساعت معاملاتی
│   ├── Signal.cs                   # خروجی Strategy
│   ├── OrderIntent.cs              # تصمیم اجرا
│   ├── BrokerOrder.cs              # سفارش واقعی
│   ├── OrderTrade.cs               # یک Fill
│   ├── Position.cs                 # موقعیت باز
│   ├── MarketSnapshotRecord.cs     # تاریخچه قیمت
│   ├── StrategyEvaluation.cs       # تاریخچه تحلیل
│   ├── CandleRecord.cs             # کندل تاریخی
│   ├── CurrencyRate.cs             # نرخ ارز
│   └── TraderAccount.cs            # حساب کارگزاری
└── Enums/
    ├── TradingRunState.cs
    ├── OrderSide.cs
    ├── SignalAction.cs
    └── TimeFrame.cs
```

### Application

```
Trader.Application/
├── Abstractions/
│   ├── Strategies/
│   │   ├── IStrategy.cs
│   │   └── StrategyRegistration.cs
│   ├── Indicators/
│   │   ├── IIndicator.cs
│   │   └── IndicatorInput.cs
│   ├── MarketData/
│   │   ├── IMarketDataProvider.cs
│   │   ├── ICandleCache.cs
│   │   ├── IMarketSessionProvider.cs
│   │   ├── ITradingScheduleCache.cs
│   │   ├── ICandleTransformer.cs
│   │   └── ICurrencyRateProvider.cs
│   ├── Execution/
│   │   ├── IOrderIntentBuilder.cs
│   │   ├── ICapitalAllocator.cs
│   │   ├── IPositionSizer.cs
│   │   └── IBrokerOrderDispatcher.cs
│   ├── Positions/
│   │   ├── IPositionManager.cs
│   │   └── IPositionCloser.cs
│   ├── Runtime/
│   │   ├── IMarketDataIngestionStage.cs
│   │   ├── IStrategyEvaluationStage.cs
│   │   ├── IExecutionStage.cs
│   │   └── IOrderLifecycleStage.cs
│   └── Brokers/
│       ├── IBroker.cs
│       ├── IBrokerSessionProvider.cs
│       └── Commands/Queries/...
├── Strategies/
│   └── Rsi/
│       ├── RsiStrategyConfig.cs
│       ├── RsiStrategy.cs
│       └── RsiStrategyRegistration.cs
├── Indicators/
│   └── Rsi/
│       ├── RsiConfig.cs
│       ├── RsiCalculator.cs        # Pure math
│       └── RsiIndicator.cs         # Adapter
└── Runtime/
    ├── MarketSnapshot.cs
    ├── Candle.cs
    ├── SignalEnvelope.cs
    ├── RunRuntimeContext.cs
    └── TradingRuntimeOptions.cs
```

### Infrastructure

```
Trader.Infrastructure/
├── Data/
│   ├── TraderDbContext.cs
│   ├── TraderUnitOfWork.cs
│   └── TraderSeedData.cs
├── Runtime/
│   ├── TradingRuntimeWorker.cs
│   ├── TradingRunManager.cs
│   ├── RunExecutionContextCache.cs
│   ├── SignalChannel.cs
│   ├── Stages/
│   │   ├── MarketDataIngestionStage.cs
│   │   ├── StrategyEvaluationStage.cs
│   │   ├── ExecutionStage.cs
│   │   └── OrderLifecycleStage.cs
│   └── Recovery/
│       └── RuntimeRecoveryService.cs
├── MarketData/
│   ├── BrokerMarketDataProvider.cs
│   ├── InMemoryMarketSnapshotStore.cs
│   ├── InMemoryCandleCache.cs
│   ├── CandlePersistenceService.cs
│   ├── IranMarketSessionProvider.cs
│   ├── TradingScheduleCache.cs
│   ├── CurrencyCandleTransformer.cs
│   ├── DbCurrencyRateProvider.cs
│   └── CurrencyRateCache.cs
├── Execution/
│   ├── OrderIntentBuilder.cs
│   ├── FixedCapitalAllocator.cs
│   ├── DynamicCapitalAllocator.cs
│   ├── CompositeCapitalAllocator.cs
│   ├── PercentPositionSizer.cs
│   ├── BrokerOrderDispatcher.cs
│   └── ExecutionCoordinator.cs
├── Positions/
│   ├── PositionManager.cs
│   └── PositionCloser.cs
├── Brokers/
│   ├── BrokerSessionProvider.cs
│   ├── InMemoryBrokerSessionCache.cs
│   └── EasyTrader/
│       └── Capabilities/...
└── Services/
    ├── InstrumentService.cs
    ├── TradingRunService.cs
    ├── SignalQueryService.cs
    ├── PositionQueryService.cs
    ├── TradingScheduleService.cs
    └── CurrencyRateService.cs
```

---

**پایان مستند**

> برای سؤالات یا ابهامات، به Logs، DB، و کد مراجعه کن.
> اگر باگ پیدا کردی، ابتدا در `Trader.Infrastructure.*` Debug Logging را فعال کن.