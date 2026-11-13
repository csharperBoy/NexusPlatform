
# Trading Module — Backend Architecture

> مستند جامع جریان کار، تصمیم‌گیری، و اجرا در ماژول Trading.
>
> **آخرین به‌روزرسانی:** ۱۴۰۴/۰۸/۱۶ — بعد از سری Refactor (PR 1-12)
>
> **توجه:** این سند **جریان کار** را توصیف می‌کند. برای **دلیل هر تصمیم معماری**،
> به [docs/TRADING-DECISIONS.md](./TRADING-DECISIONS.md) مراجعه کن. هر تصمیم با
> شناسه‌ی `D1`..`D28` در آن سند ارجاع داده شده.

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
13. [Trading Schedule](#13-trading-schedule-ساعت-معاملاتی)
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

| اصل | توضیح | شناسه |
|---|---|---|
| **Strategy از Account جداست** | Strategy فقط تحلیل می‌کند، نمی‌داند چند حساب دارد | D1 |
| **Market Data از Execution جداست** | یک منبع داده برای همه‌ی Runها | D2 |
| **TradingRun مسئول Execution Context است** | تنظیمات Capital، SL/TP، Accountها در Run | D3 |
| **Broker پشت Abstraction** | Mofid/Agah/سایر از طریق `IBroker*` Interface | D18, D19 |
| **Multi-Account = Fan-Out (بدون Coordination)** | یک Signal روی همه‌ی Accountهای همان Run | D5 |
| **FillMath = تنها منبع محاسبات Fill** | Live/Paper/Backtest همه از همان فرمول | D10 |
| **Future-ready, Simple Phase 1** | Extensible بدون Overengineering | — |

### چرخه‌ی کلی

```
Broker API
    │
    ▼
Market Data Ingestion (Stage 1)
    │
    ▼
Strategy Evaluation (Stage 2) ──→ StrategyEvaluationService
    │                                 └─→ Indicator (RSI، MACD، ...)
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
    │                                            │
    │                                            ▼
    └─→ FillMath.CalculateRealizedPnL ──→ PnL
    │
    ▼
Position Events (PnL، Close، ...)
```

---

## 2. مفاهیم پایه

| مفهوم | تعریف |
|---|---|
| **Instrument** | هویت نماد (فولاد، شپنا، ...). ISIN یکتا. Option Contract از نوع Owned Type. |
| **TraderAccount** | حساب کارگزاری. BrokerType + Username + Session. |
| **TradingSchedule** | ساعت معاملاتی نماد (سهام عادی ۹-۱۲:۳۰، عیار ۱۲:۳۰-۱۸، ...) |
| **TradingRun** | یک "پلن". یک Strategy + چند Symbol + چند Account + تنظیمات Execution. |
| **Strategy** | منطق تولید Signal. هیچ I/O. |
| **Indicator** | تابع محاسباتی خالص (RSI، MACD). بدون I/O، بدون Strategy. |
| **FillMath** | توابع خالص محاسبات Fill (Weighted Average، PnL، Commission). |
| **Signal** | خروجی Strategy: `Action (Buy/Sell) + Strength`. بدون قیمت، بدون مقدار. |
| **OrderIntent** | تصمیم اجرا برای یک Account خاص. شامل Price، Quantity، SL/TP، IsPaper. |
| **BrokerOrder** | سفارش واقعی که به Broker فرستاده شد. `IsPaper` برای Paper Mode. |
| **OrderTrade** | یک Fill (بخشی) از BrokerOrder. |
| **Position** | موقعیت باز یک (Run, Account, Isin). `IsPaper` برای Paper Mode. |
| **MarketSnapshot** | قیمت لحظه‌ای از Broker. همیشه ریالی. |
| **Candle** | OHLCV یک بازه‌ی زمانی. ممکن است تبدیل ارز شده باشد. |
| **ExecutionMode** | `Live` (پول واقعی) یا `Paper` (شبیه‌سازی). |

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
│  - IStrategy, IIndicator, IStrategyEvaluationService  │
│  - IBrokerSessionProvider, ITraderUnitOfWork          │
│  - IMarketDataProvider, IOrderSizeCalculator          │
└─────────────────────────────────────────────────────┘
                         │
┌────────────────────────▼─────────────────────────────┐
│  Domain (Entities, Value Objects, Events, Calculations)│
│  - TradingRun, Signal, OrderIntent, Position          │
│  - CandleRecord, MarketSnapshotRecord, CurrencyRate   │
│  - FillMath (Weighted Average, PnL, Commission)       │
│  - CurrencyCode (TargetCurrency Whitelist)            │
└─────────────────────────────────────────────────────┘
                         │
┌────────────────────────▼─────────────────────────────┐
│  Infrastructure (EF, Brokers, Runtime, Stages)       │
│  - TraderDbContext, TraderUnitOfWork                  │
│  - EasyTrader Adapter                                 │
│  - TradingRuntimeWorker + Stages 1-4                  │
│  - StrategyEvaluationService, OrderSizeCalculator     │
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
| `Instruments` | نمادها + Commission + Order Limits + OptionContract | ثابت |
| `TraderAccounts` | حساب‌ها | ثابت |
| `TradingSchedules` | ساعت معاملاتی | ۴-۱۰ ردیف ثابت |
| `TradingRuns` | پلن‌ها | کم |
| `Signals` | سیگنال‌های تولیدشده | ~۰-۱۰ |
| `OrderIntents` | تصمیم‌های اجرا | ~۰-۱۰ |
| `BrokerOrders` | سفارش‌های واقعی/Paper | ~۰-۱۰ |
| `OrderTrades` | Fillها | ~۰-۳۰ |
| `Positions` | موقعیت‌ها | ~۰-۱۰ |
| `MarketSnapshots` | قیمت لحظه‌ای | ~۷۰۰-۳۰۰۰ |
| `StrategyEvaluations` | هر Evaluate | ~۷۰۰-۳۰۰۰ |
| `Candles` | کندل‌های تاریخی | ۵-۱۰ (M5) |
| `CurrencyRates` | نرخ ارز | کم |
| `TradingEvents` | Event Logging | ~۵۰-۲۰۰ |

### فیلدهای کلیدی (تغییرات نسبت به نسخه‌ی قبلی)

#### `TradingRuns`

```
- State: Created / Ready / Running / Stopped / Faulted
- StrategyKey, StrategyConfigJson
- PrimaryTimeFrame (enum)
- StopLoss (Owned: Percentage یا AbsolutePrice)
- TakeProfit (Owned: Percentage یا AbsolutePrice)
- DefaultOrderMode: Quantity / TotalValue
- OnMarketClose: KeepPositions / ClosePositions
- OnStop: KeepPositions / ClosePositions
- TargetCurrency: string? (whitelisted — D4/M10)
- StrengthMultiplier: decimal (default 1.5)       ← Stepped Buying
- MinDeltaPercent: decimal (default 0.03)         ← Stepped Buying
- ExecutionMode: Live / Paper                     ← Paper Trading
```
### `TradingRunAccounts`

- TradingRunId, TraderAccountId
- CapitalAllocation (Owned):
    - Mode: Fixed / Dynamic
    - FixedAmount: Money? (فقط Fixed)
    - PercentOfBuyPower: decimal? (فقط Dynamic، (0, 100])
    - Invariant: دقیقاً یکی Set
#### `OrderIntents`

```
- RunId, TraderAccountId, SourceSignalId
- SourceEvaluationId: Guid?                       ← ML/Audit trace
- Isin, Side, Price, Quantity, OrderMode
- State: Pending / Dispatched / Acknowledged / Rejected / Failed
- StopLossPrice, TakeProfitPrice (از TradingRun.Settings در لحظه‌ی Build)
- IsPaper: bool                                   ← Paper Trading
- Reason, FailureReason, DispatchedAt, CompletedAt
```

#### `BrokerOrders`

```
- OrderIntentId, RunId, TraderAccountId
- BrokerOrderId: string? (از Broker)
- IsPaper: bool
- Isin, Side, Price, Quantity, FilledQuantity
- State: Pending / Open / PartiallyExecuted / FullyExecuted / Canceled / Rejected / Expired
- SubmittedAt, LastSyncedAt
```

#### `Positions`

```
- RunId, TraderAccountId, Isin
- SourceEvaluationId: Guid?                       ← ML/Audit trace
- IsPaper: bool
- State: Open / PartiallyClosed / Closed
- Quantity, AverageEntryPrice
- RealizedPnL (Owned: Amount + Currency)
- UnrealizedPnL (Owned: Amount + Currency, nullable)
- StopLossPrice, TakeProfitPrice
- OpenedAt, ClosedAt
```
#### `InstrumentOrderPresets` (Phase C-lite.2)
InstrumentId (FK)

Name

PriceStrategy (enum: Fixed/HighAllowed/LowAllowed/LastTraded/Closing/FirstTraded)

FixedPrice: decimal? (فقط Fixed)

QuantityStrategy (enum: Fixed/MaxOrderQuantity/MinOrderQuantity)

FixedQuantity: long? (فقط Fixed)

DefaultSide, ValidityType, Commission, DefaultOrderMode

CreatedAt

#### `PriceSources` (Phase B1 — D34)
Code (unique), DisplayName, Type (enum: Tgju/TelegramChannel/CustomApi)

ConfigJson ({"indicator":"price_dollar_rl","maxRate":...,"maxDailyChangePct":20})

DefaultFromAsset, DefaultToAsset

IsActive

#### `PriceCollectionJobs` (Phase B1 — D35)
Name, Mode (Recurring/Backfill), State (Created/Running/Stopped/Completed/Faulted)

PriceSourceId (FK), FromAsset, ToAsset

RecurrencePatternJson (Recurring)

SchedulerRecurringId (Recurring), SchedulerJobId (Backfill)

StartedAt, StoppedAt, LastRunAt

LastRunInserted, LastRunSkipped, TotalInserted

BackfillProgressPercent (nullable, Backfill only)

LastError



#### `Instruments` — Range Split (D53)
DailyPriceRangeUpPercent: decimal? (null = unlimited)

DailyPriceRangeDownPercent: decimal? (null = unlimited)

Independent up/down (asymmetric: +5%/-2% valid)



#### `HistoricalDataCollectionJobs` — Refactor (D49)
Mode (Backfill/Recurring), State (Created/Running/Stopped/Completed/Faulted)

SchedulerRecurringId (Recurring), SchedulerJobId (Backfill)

RecurrencePatternJson (Recurring)

BackfillProgressPercent, CurrentIsin, CurrentTimeFrame (Backfill only)

LastRunAt, LastRunInserted, LastRunSkipped, TotalInserted

DateFrom, DateTo (nullable — Recurring = null)



#### Backtest Entities (D55-D57)
BacktestRun:

TargetCurrency فقط بر StrategyInput اثر دارد (D57)

همیشه execution در IRR

BacktestCandleEvent (internal):

Candle (IRR) ← برای fill, SL/TP, sizing, equity

StrategyCandle (nullable) ← transformed, فقط برای IStrategy.Evaluate

SubCandles (همیشه IRR)

Config Snapshot (D22):

Run config کامل Snapshot می‌شود

SourceRunId فقط Reference



### `Value Objects` — Update:

**در لیست Value Objects، اضافه کن:**

```markdown
- `AccountCapitalAllocation` (per-account, PR 19)
- `DailyPriceRangeUpPercent` + `DailyPriceRangeDownPercent` (nullable = unlimited، D53)
- `OrderPriceStrategy` + `OrderQuantityStrategy` (enum، نه VO، ولی Semantic مشابه)
#### `Candles`

```
- Isin, TimeFrame (enum، نه int — N5), OpenTime
- Open, High, Low, Close, Volume
- Source, FetchedAt
```

| **`PriceSources`** | منابع قیمت پیکربندی‌شده (TGJU، Telegram، ...) — Phase B1 | ثابت (Seed) |
| **`PriceCollectionJobs`** | Jobهای جمع‌آوری قیمت (شبیه TradingRun) — Phase B1 | کم |

#### `PriceSources` (Phase B1)

- Code (unique), DisplayName, Type (enum: Tgju/TelegramChannel/CustomApi)
- ConfigJson (provider-specific: {"indicator":"price_dollar_rl"})
- DefaultFromAsset, DefaultToAsset
- IsActive

#### `PriceCollectionJobs` (Phase B1)

- Name, State (Created/Ready/Running/Stopped/Faulted), Mode (Recurring/Backfill)
- PriceSourceId, FromAsset, ToAsset
- CronExpression یا IntervalValue + IntervalUnit (Recurring)
- BackfillFromUtc, BackfillToUtc (Backfill)
- SchedulerRecurringId (برای Unschedulable)
- StartedAt, StoppedAt
- LastRunAt, LastRunInserted, LastRunSkipped, TotalInserted, BackfillProgressPercent
- LastError
- OwnerPersonId/OwnerUserId (Multi-user آینده)
- 
### روابط کلیدی

```
TradingRun ─┬── TradingRunSymbol (N) ── ISIN + Allocation%
            └── TradingRunAccount (N) ── TraderAccountId

TradingRun ──→ Signal (N) ──→ OrderIntent (N) ──→ BrokerOrder (N) ──→ OrderTrade (N)
                                                                          │
                                                                          ▼
                                                                      Position

                                                                      // Run با ۳ Account:
TradingRunAccount 1: Account A, Fixed(50_000_000 IRR)
TradingRunAccount 2: Account B, Dynamic(100%)   // از BuyPower B
TradingRunAccount 3: Account C, Dynamic(50%)    // از BuyPower C

Signal ──→ StrategyEvaluation (1:1 از طریق SourceEvaluationId)
OrderIntent ──→ StrategyEvaluation (N:1)
Position ──→ StrategyEvaluation (N:1)

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
│     - Flush TradingEvents                   │
│     - Scope Dispose                         │
└────────────────────────────────────────────┘
```

### چرا Scope جدید در هر Tick؟

- Stageها Scoped هستند (به UoW و DbContext وابسته).
- Scope per Tick → DbContext تازه → Change Tracker تمیز.
- خطای یک Tick، بقیه را نمی‌کشد.

### PeriodicTimer vs Task.Delay

`PeriodicTimer` استفاده می‌شود که Drift ندارد. اگر Tick ۲ ثانیه طول بکشد و Interval ۱۰ ثانیه باشد، Tick بعدی در ثانیه ۱۲ شروع می‌شود (نه ۱۴).

### Per-Stage Timing (N4)

هر Tick، زمان هر Stage جدا اندازه‌گیری می‌شود. اگر Total > 1000ms، Warning با breakdown صادر می‌شود:

```
Slow tick: total=1250ms stage1=800ms stage2=150ms stage3=200ms stage4=100ms
```

این کمک می‌کند که دقیقاً بدانیم کدام Stage کند است.

### پیکربندی

```json
"Trader": {
  "Runtime": {
    "HeartbeatInterval": "00:00:10",
    "OrderLifecycleInterval": "00:00:05",
    "SignalChannelCapacity": 1000,
    "OrderPollBatchSize": 100
  },
  "Logging": {
    "MinSeverity": "Info",
    "RetentionDays": 90
  }
}
```

---
### Scheduler Integration (Phase B1 — D39)

Runtime pipeline (Stage 1-4) برای **Trade Execution** است. Jobهای Collection (Price، Historical Candles) از `Scheduler` Module پلتفرم استفاده می‌کنند:
Scheduler (Hangfire + RecurrencePattern)
├─→ trader.price-collect → CollectPriceJobHandler
│ ├─→ Recurring (Minutely/Hourly/Daily/Cron)
│ └─→ Backfill (one-off)
│
└─→ trader.historical-collect → CollectCandleJobHandler
├─→ Recurring (from max(OpenTime) per Isin,TF)
└─→ Backfill (DateFrom..DateTo)


**چرا Scheduler جدا:**
- Runtime Stageها Scoped-per-tick هستند (DbContext + UoW fresh)
- Jobها ممکن است ۳۵ ثانیه (Backfill) یا ۲-۳ ساعت (Historical) طول بکشند
- Scheduler خودش Retry، Persistence، Recovery، Pause@10 را دارد

**OrderLifecycleStage Throttle (D59 B2):** `IRuntimeThrottleState` Singleton با `Interlocked.CompareExchange` — بدون lock، atomic claim. جایگزین `_lastRunAt` instance field (که در Scoped service reset می‌شد).
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

اگر Run A و Run B هر دو `فولاد` را داشته باشند، فقط **یک بار** از Broker Fetch می‌شود:

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

### Event Logging

- `MarketDataFetched` — با تعداد نماد و elapsed
- `MarketClosedSkipped` — Debug
- `MarketDataFetchFailed` — با error detail

---

## 7. Stage 2 — Strategy Evaluation

### مسئولیت

برای هر (Run × Symbol): اجرای Strategy، ذخیره‌ی Evaluation، تولید Signal در صورت وجود.

**توجه (M6):** از PR 11، منطق ارزیابی در `IStrategyEvaluationService` (Application Abstractions) است. Stage حالا فقط **Orchestrator** است (~140 LOC، قبلاً ~480 LOC).

### دو Component

| Component | مسئولیت |
|---|---|
| `StrategyEvaluationService` (Infra) | Load candles، Currency transform، Extra TF، Evaluate، Enrich features، ساخت `StrategyEvaluation` + `Signal`ها |
| `StrategyEvaluationStage` (Infra) | User Context، فراخوانی Service per (Run, Symbol)، Persist، Push به Channel، Logging |

### Price Collection (Phase B1 — جدید)
IPriceCollectionJobService → Create/Update/Delete/Start/Stop/GetList/GetById
IPriceCollectionService → CollectOnceAsync، CollectBackfillAsync
IPriceSourceProvider → FetchCurrentAsync، FetchHistoricalAsync
IPriceSourceProviderResolver → Resolve(PriceSourceType) → IPriceSourceProvider?
ITgjuClient → GetCurrentRateAsync، GetHistoricalRatesAsync
CollectPricePayload : IScheduledJobPayload
Key = "trader.price-collect"

**مرز (D-list):** Service به UoW و Channel دسترسی ندارد. فقط از Cacheها و Transformer استفاده می‌کند.

### جریان

```
StrategyEvaluationStage.RunOnceAsync()
    │
    ├─→ foreach Run in RunCache.GetAllActive():
    │       ├─→ Set User DataContext (برای Authorization)
    │       │
    │       └─→ foreach Symbol in Run:
    │               │
    │               ├─→ IStrategyEvaluationService.EvaluateAsync(ctx, symbol, now)
    │               │       │
    │               │       ├─→ Snapshot از InMemoryMarketSnapshotStore
    │               │       │       └─→ اگر null: return Empty
    │               │       │
    │               │       ├─→ Candle از InMemoryCandleCache
    │               │       │       └─→ اگر Strategy.RequiredCandleCount = 0: skip
    │               │       │
    │               │       ├─→ فیلتر Closed Candles
    │               │       │       └─→ اگر RequiresClosedCandlesOnly = true
    │               │       │
    │               │       ├─→ Currency Transform (اگر Run.TargetCurrency != null)
    │               │       │       └─→ Per-Candle (D13)
    │               │       │
    │               │       ├─→ Extra TimeFrames (Sub-Candle)
    │               │       │       └─→ اگر Strategy.ExtraCandleRequirements non-empty
    │               │       │
    │               │       ├─→ Strategy.Evaluate(input)
    │               │       │
    │               │       ├─→ Enrich features (اگر Currency transform شد)
    │               │       │
    │               │       ├─→ StrategyEvaluation.Create (اگر features موجود بود)
    │               │       │
    │               │       └─→ Signal.Create + LinkSignal (اگر Action != Hold)
    │               │
    │               ├─→ Persist batch.Evaluation
    │               ├─→ Persist batch.Signals
    │               ├─→ Log SignalGenerated per signal
    │               └─→ Build envelope per signal
    │
    ├─→ SaveChanges یک بار (همه Evaluations + Signals)
    │
    └─→ Push envelopes به Channel
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
    ├─── Features (rsi، period، targetCurrency، appliedRate، ...)
    │       └─→ StrategyEvaluation.FeaturesJson
    │
    └─── Signals[]
            │
            ├─── Signal.Create (SourceEvaluationId = evalEntity.Id)
            ├─── AddDomainEvent(SignalGeneratedEvent)
            │
            ▼
        Return (Evaluation, Signals) از Service
            │
            ▼
        Stage: AddAsync + SaveChanges
            │
            ▼
        Channel<SignalEnvelope> (in-memory)
```

**نکته:** SaveChanges **قبل** از Push به Channel انجام می‌شود. اگر Process بین Save و Push بمیرد، Signal در DB هست ولی Stage 3 آن را ندیده — Recovery در Startup آن را می‌گیرد.

### Extensibility: Sub-Candle (A2)

Strategy می‌تواند در `ExtraCandleRequirements` بگوید که به TimeFrameهای کوچک‌تر از Primary نیاز دارد:

```csharp
public IReadOnlyList<CandleRequirement> ExtraCandleRequirements => new[]
{
    new CandleRequirement(TimeFrame.M15, Count: 5, ClosedOnly: true)
};
```

Service این Candleها را Load می‌کند و از طریق `StrategyInput.ExtraTimeFrames` به Strategy می‌دهد. **محدودیت:** فقط TimeFrame کوچک‌تر از Primary.

---

## 8. Stage 3 — Execution

### مسئولیت

مصرف Signalها از Channel، ساخت OrderIntent، ارسال به Broker.

**توجه (D5):** برای هر Signal، **همه‌ی Accountهای همان Run** پردازش می‌شوند — Fan-Out بدون Coordination.

### جریان

```
Channel<SignalEnvelope> (drain، بدون Blocking، max 200/tick)
    │
    ▼
for each SignalEnvelope:
    │
    ├─→ Get Run Context از Cache (اگر نبود، Skip)
    ├─→ Set User DataContext
    │
    └─→ for each Account in Run.Accounts:
            │
            ├─→ GUARD: BrokerOrder در راه برای همین (Run, Account, Isin)؟
            │       └─→ اگر هست: Skip + Log OrderIntentSkipped
            │
            ├─→ OrderIntentBuilder.BuildAsync
            │   ├─→ Snapshot از Store (قیمت ریالی، نه converted)
            │   ├─→ Instrument lookup (برای Commission + Limits)
            │   ├─→ SymbolAllocation از Run.Symbols
            │   ├─→ ICapitalAllocator (Fixed یا Dynamic)
            │   ├─→ ICommittedCapitalCalculator (Positionهای باز + Buy Orderهای در راه)
            │   ├─→ IOrderSizeCalculator.CalculateBuy / CalculateSell
            │   │       └─→ FillMath.CalculateMaxBuyQuantity
            │   ├─→ Resolve SL/TP از Run.Settings
            │   ├─→ OrderIntent.Create (با IsPaper از Run.ExecutionMode)
            │   └─→ SourceEvaluationId (از Signal)
            │
            ├─→ BrokerSessionProvider.GetSessionAsync
            │
            ├─→ IOrderDispatcherResolver.Resolve(intent.IsPaper)
            │       ├─→ Live: BrokerOrderDispatcher
            │       │       └─→ Broker API: SendBuyOrder / SendSellOrder
            │       └─→ Paper: PaperBrokerOrderDispatcher
            │               └─→ Immediate Fill + Position ApplyTradeAsync
            │
            ▼
        SaveChanges یک بار در پایان Handle
```

### Capital Allocation (per-account, PR 19)

هر `TradingRunAccount` مستقل تصمیم می‌گیرد:

| Mode | منبع |
|---|---|
| **Fixed** | `TradingRunAccount.CapitalAllocation.FixedAmount` (مقدار ثابت ریالی) |
| **Dynamic** | `IAccountCashCache.BuyPowerT0 × PercentOfBuyPower / 100` |

`IAccountCashCache` Invalidate-Driven است:
- Refresh از Broker فقط بعد از Dispatch موفق یا Fill
- `GetOrRefreshAsync` اگر Cache Miss یا RefreshedAt > 30 دقیقه
- یک Account می‌تواند در چند Run باشد — Cash مشترک (D30)

`CompositeCapitalAllocator` per-account تصمیم می‌گیرد (نه per-run):
- Account A Fixed → `FixedCapitalAllocator`
- Account B Dynamic → `DynamicCapitalAllocator`
- 
### Order Sizing (D10 + M7)

**فرمول دقیق (PR 6):**

```csharp
// 1. Target از Capital × Strength × Multiplier
var targetCapital = allocatedCapital
    * signalStrength
    * strengthMultiplier;
if (targetCapital > allocatedCapital) targetCapital = allocatedCapital;  // cap

// 2. Delta = Target - Committed (Positionهای باز + Buy Orderهای در راه)
var delta = targetCapital - committedCapital;
if (delta <= 0) return Skip("DeltaLessThanOrEqualZero");

// 3. MinDelta Guard (Stepped Buying)
var minDelta = allocatedCapital * minDeltaPercent;
if (delta < minDelta) return Skip("DeltaBelowMinThreshold");

// 4. Quantity = floor(delta / (price × (1 + commissionRate)))
var qty = FillMath.CalculateMaxBuyQuantity(delta, price, commissionRate);

// 5. AvailableCash Guard (اختیاری، فقط Backtest — null = Live)
if (availableCash.HasValue)
{
    var maxAffordable = FillMath.CalculateMaxBuyQuantity(
        availableCash.Value, price, commissionRate);
    if (maxAffordable < qty) qty = maxAffordable;
}

// 6. Min/Max Limits
if (qty < minOrderQuantity) return Skip("QuantityBelowMin");
if (qty > maxOrderQuantity) qty = maxOrderQuantity;
```

**فرمول قدیمی (approximation) که ترک شد:**

```csharp
// ❌ قبلاً: qty = floor(delta × (1 - c) / price)
// این کمی بیشتر qty می‌داد (O(c²) خطا)
```

### Stepped Buying (M7 + MinDeltaPercent)

اگر Strategy هر tick Buy صادر کند و Position باز هم Buy ادامه دهد:

- **Step 1:** StrengthMultiplier = 1.5، Capital = 100M
  - Target = 100M × 0.6 × 1.5 = 90M
  - Committed = 0
  - Delta = 90M → qty = floor(90M / 50000) = 1800
- **Step 2 (tick بعد):** Same Signal
  - Committed = 1800 × 50000 = 90M
  - Target = 90M (همان)
  - Delta = 0 → **Skip** (DeltaLessThanOrEqualZero)
- **Step 3 (قیمت افت، Signal Stronger):** Strength = 0.9
  - Target = 100M × 0.9 × 1.5 = 135M → cap 100M
  - Committed = 90M
  - Delta = 10M
  - MinDelta = 100M × 3% = 3M ✓
  - qty = floor(10M / (50000 × 1.0037)) ≈ 199

**نتیجه:** به‌جای خرید مکرر، فقط وقتی Delta از MinDelta بیشتر شود خرید انجام می‌شود.

### قیمت سفارش — **ریالی، نه تبدیل‌شده**

**مهم:** حتی اگر `Run.TargetCurrency = "USD"` باشد، `OrderIntent.Price` از Snapshot ریالی می‌آید:

```csharp
var snapshot = _snapshotStore.GetLatest(signal.Isin);  // ← ریالی
var price = ResolvePrice(snapshot, signal.Action);      // ← ریالی
```

**دلیل:** Broker فقط ریال قبول می‌کند. Currency Transformation فقط برای تحلیل (Candles) است.

### Paper Trading (D23)

`PaperBrokerOrderDispatcher`:
- Fill **فوری** در `intent.Price` (بدون Slippage)
- `IsPaper = true` روی `BrokerOrder` و `Position`
- Stage 4 آن را Skip می‌کند (چون `IsTerminal` است)

**Paper Limit:** در Paper Mode، `AvailableCash` هنوز از Account واقعی (BuyPower) خوانده می‌شود. فقط سفارش به Broker نمی‌رود.

### Idempotency

- `Signal.MarkConsumed()` بعد از Handle
- در Recovery، Signalهای Consumed‌نشده بررسی می‌شوند
- اگر `BrokerOrder` در راه برای همان (Run, Account, Isin) باشد، **Skip می‌شود** (جلوگیری از Duplicate Order)

---

## 9. Stage 4 — Order Lifecycle

### مسئولیت

Poll سفارش‌های باز از Broker، Sync Trades، اعمال روی Position، SL/TP monitoring.

**توجه (C2 — D8):** از PR 2، SL/TP monitoring **مستقل از Order Activity** است. حتی اگر هیچ Order بازی نباشد، Positionهای باز در هر tick بررسی می‌شوند.

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
    └─→ SyncAccountAsync:
            │
            ├─→ [1] Order Sync (اگر Order باز داریم)
            │       ├─→ Orderهای باز ما (از DB، spec-based)
            │       ├─→ Fetch Order History از Broker (Pagination)
            │       │
            │       └─→ foreach باز Order:
            │               ├─→ SyncOrderAsync
            │               │       ├─→ BrokerOrder.ApplyStateUpdate
            │               │       └─→ اگر ExecutedQuantity افزایش یافته:
            │               │               └─→ SyncTradesAsync:
            │               │                       ├─→ Fetch Trades از Broker
            │               │                       ├─→ foreach Trade:
            │               │                       │       ├─→ اگر جدید: OrderTrade.Create + AddAsync
            │               │                       │       ├─→ PositionManager.ApplyTradeAsync
            │               │                       │       │       ├─→ Buy + Position نیست: Position.Open
            │               │                       │       │       ├─→ Buy + Position هست: ApplyBuyFill
            │               │                       │       │       └─→ Sell: ApplySellFill
            │               │                       │       │
            │               │                       │       └─→ اگر Position بسته شد:
            │               │                       │               └─→ UpdateEvaluationOutcomeAsync
            │               │                       │
            │               │                       └─→ Tradeهای Cancelled: ApplyTradeCancelAsync
            │
            ├─→ [2] SaveChanges (اگر Order تغییر کرد)
            │
            └─→ [3] SL/TP Monitoring (همیشه، مستقل از Order activity)
                    ├─→ CountBySpecAsync(OpenPositionsByAccountSpec)
                    └─→ اگر > 0:
                            └─→ CheckStopsAndCloseAsync:
                                    ├─→ foreach Position باز:
                                    │       ├─→ Snapshot.GetLatest
                                    │       ├─→ ShouldTriggerStopLoss/TakeProfit?
                                    │       └─→ اگر بله: Sell OrderIntent + Dispatch
                                    │               (با IsPaper از Position.IsPaper)
                                    │
                                    └─→ SaveChanges
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
                         sourceEvaluationId: sourceEvalId,
                         isPaper: order.IsPaper);
```

### Position Overshoot Detection (C4 — D9)

**از PR 2:** اگر Broker یک Sell بزرگ‌تر از Position ثبت‌شده گزارش دهد:

```csharp
if (trade.Quantity.Value > position.Quantity.Value)
{
    // 1. Fill اعمال نمی‌شود (Position در known good state می‌ماند)
    // 2. TradingEvent با Severity=Critical ثبت می‌شود
    // 3. Run → Faulted
    // 4. Run از Cache خارج می‌شود
    // 5. Trade record هنوز persist می‌شود (audit trail)
    return;
}
```

**علت‌های ممکن:** Manual Trade در Broker، Cross-Account Bug، Data Race، Position Phantom.

**چرا Faulted و نه Clamp:** ادامه دادن با State خراب، اختلاف را بزرگ‌تر می‌کند. بررسی دستی لازم است.

### SL/TP Independence (C2 — D8)

**قبل از PR 2 (باگ):**
```csharp
if (openOrders.Count == 0) return;   // ← SL/TP هرگز monitor نمی‌شد
// ...
if (modifiedCount > 0)
{
    await CheckStopsAndCloseAsync(...);   // ← فقط اگر Order تغییر کرد
}
```

**بعد از PR 2:**
```csharp
// [1] Order Sync (اگر openOrders > 0)
// [2] SaveChanges (اگر modified)
// [3] SL/TP Check — همیشه، مستقل
var count = await CountBySpecAsync(OpenPositionsByAccountSpec);
if (count > 0)
    await CheckStopsAndCloseAsync(...);
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
    ├─→ Load Signalهای ConsumedAt=null برای Runهای فعال
    │       │
    │       ├─→ اگر هیچ OrderIntent ندارد → Re-Push به Channel (سناریو B)
    │       │
    │       └─→ اگر OrderIntent دارد → MarkConsumed + Log Warning (سناریو C/D)
    │
    └─→ Broker Reconciliation (A1)
            └─→ مقایسه‌ی Positionهای DB با Portfolio Broker
                └─→ TradingEvent per Mismatch (Log-only، نه Auto-fix)
```

### سناریوها

| سناریو | وضعیت DB | اقدام |
|---|---|---|
| A: Crash قبل از Save | Signal نیست | هیچ |
| B: Crash بین Save و Push | Signal ذخیره، Consumed=null | Re-Push |
| C: Crash وسط Stage 3 | Signal + شاید OrderIntent | Mark Consumed (جلوگیری از Duplicate) |
| D: Crash بعد از Save | Signal + OrderIntent | Mark Consumed |
| E: Crash بعد از Stop | Run=Stopped | Stage 4 در Tick بعدی Sync |
| F: Run=Running ولی Cache خالی | Run State=Running | `TradingRunManager.StartAsync` |

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
    │  output: decimal? (null = data کافی نیست)
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
- `RsiCalculator` می‌تواند در AI Pipeline یا Backtest مستقیم استفاده شود
- `RsiStrategy` و `MacdRsiStrategy` می‌توانند همان `RsiIndicator` را استفاده کنند

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

    /// <summary>TimeFrameهای اضافی که Strategy نیاز دارد (Sub-Candle)</summary>
    IReadOnlyList<CandleRequirement> ExtraCandleRequirements { get; }

    StrategyEvaluationResult Evaluate(StrategyInput input);
}

public sealed record CandleRequirement(
    TimeFrame TimeFrame,
    int Count,
    bool ClosedOnly = true);
```

### StrategyInput / Result

```csharp
public sealed record StrategyInput(
    string Isin,
    MarketSnapshot? LatestSnapshot,       // ← ریالی
    IReadOnlyList<Candle> Candles,         // ← ممکن است Converted باشد
    IReadOnlyDictionary<TimeFrame, IReadOnlyList<Candle>>? ExtraTimeFrames,  // ← NEW (A2)
    StrategyConfig Configuration,
    TimeFrame TimeFrame,
    DateTime EvaluatedAt);

public sealed record StrategyEvaluationResult(
    IReadOnlyList<StrategySignal> Signals,
    IReadOnlyDictionary<string, object?>? Features = null)
{
    public static StrategyEvaluationResult Empty(
        IReadOnlyDictionary<string, object?>? features = null)
        => new(Array.Empty<StrategySignal>(), features);
}
```

**نکته:** `Signals` یک **لیست** است — برای Strategy Ensemble آینده (D27). هر Strategy فعلاً یک Signal تولید می‌کند، ولی Interface محدود نمی‌کند.

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
services.AddTraderStrategies();  // Assembly scanning — IStrategyRegistration
```

**توجه:** `IStrategyFactory` در Application ثبت شده (نه Infrastructure). N1 در Review این Duplicate را حذف کرد.

### افزودن Strategy جدید

1. `Create` config class (از `StrategyConfig`)
2. `Create` strategy class (پیاده‌سازی `IStrategy`)
3. `Create` registration class (از `StrategyRegistration<TConfig>`)
4. Assembly scanning خودکار پیدا می‌کند — **نیازی به DI خط نیست**

### AI Strategy (آینده)

AI یک `IStrategy` است، نه سیستم جدا:

```csharp
public sealed class AiStrategy : IStrategy
{
    public StrategyEvaluationResult Evaluate(StrategyInput input)
    {
        var features = ExtractFeatures(input);
        var prediction = _model.Infer(features);
        // ...
    }
}
```

Feature Extraction از `input.Candles` و `input.LatestSnapshot` — همان داده‌ای که به RSI می‌رسد.

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

### Cache — بهینه‌سازی (M1 Batch 3)

از PR 12، `TradingScheduleCache.GetForIsinAsync` از `GetBySpecAsync` استفاده می‌کند و Fallback با `TOP 1` در SQL اجرا می‌شود — نه Load کردن همه Scheduleها.

---

## 14. Currency Transformation

### چرا؟

تحلیل سهم در ارز دیگر مفید است — RSI روی USD نشان می‌دهد سهم واقعاً رشد کرده یا فقط ریال تضعیف شده.

### Whitelist (M10 — D4)

از PR 8، `TargetCurrency` در `TradingRun.Create` و `BacktestRun.Create` با `CurrencyCode.NormalizeOrNull` اعتبارسنجی می‌شود:

```csharp
public static class CurrencyCode
{
    private static readonly HashSet<string> Supported = new(StringComparer.OrdinalIgnoreCase)
    {
        "IRR", "USD", "EUR", "GBP", "AED", "TRY", "CNY", "JPY", "CHF",
    };
    
    public static string? NormalizeOrNull(string? code, string paramName);
    public static string RequireValid(string? code, string paramName);
}
```

اگر `TargetCurrency` در Whitelist نبود، `Create` فوراً `ArgumentException` می‌دهد — نه در اولین Tick که `CurrencyRateNotFoundException` می‌داد.

**Phase 2:** اگر ارزهای پویا لازم شد، این کلاس به یک Entity با `IsActive` تبدیل می‌شود و اعتبارسنجی به Application Layer منتقل می‌شود.

### Data Model

```
TradingRun.TargetCurrency: string? (whitelisted)

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

**Reverse Pair:** `DbCurrencyRateProvider` اگر Pair مستقیم نبود، معکوس را جستجو می‌کند:

```csharp
var directRate = await QueryDbAsync(from, to, utcAtTime, ct);
if (directRate is not null) return directRate.Value;

var reverseRate = await QueryDbAsync(to, from, utcAtTime, ct);
if (reverseRate is not null) return 1m / reverseRate.Value;
```

**Triangulation (USD→EUR via IRR) پشتیبانی نمی‌شود.**

### Transformation (D13 — Per-Candle)

در Stage 2، **قبل از** Strategy.Evaluate:

```csharp
if (!string.IsNullOrWhiteSpace(ctx.TargetCurrency) && candles.Count > 0)
{
    var transformation = await _candleTransformer.TransformAsync(
        symbol.Isin, candles, ctx.TargetCurrency, now, ct);
    candles = transformation.Candles;
}
```

**`CurrencyCandleTransformer`:** هر Candle با نرخ ارز مربوط به **`OpenTime` همان Candle** تبدیل می‌شود (نه با نرخ آخرین Candle). برای Candleهایی که **قبل از** اولین نرخ ثبت‌شده هستند، از `baselineRate` استفاده می‌شود و `CandleCountBeforeFirstRate` در خروجی گزارش می‌شود.

### تصمیمات کلیدی

| بخش | تبدیل می‌شود؟ |
|---|---|
| Candles (برای Strategy) | ✅ بله (Per-Candle) |
| MarketSnapshot (برای Sizing/Execution) | ❌ نه — ریالی |
| OrderIntent.Price | ❌ نه — ریالی |
| Position.AverageEntryPrice | ❌ نه — ریالی |
| StrategyEvaluation.FeaturesJson["lastClose"] | ✅ بله (transformed) |
| StrategyEvaluation.FeaturesJson["priceAtEvalIrr"] | ✅ جدا ذخیره می‌شود |
| StrategyEvaluation.FeaturesJson["appliedRate"] | ✅ جدا ذخیره می‌شود |

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
  "action": 0,
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

**کاربرد:** Historical Ground Truth. Backtesting.

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
    },
    "Logging": {
      "MinSeverity": "Info",
      "RetentionDays": 90
    }
  }
}
```

### DI Registration (چکیده)

```csharp
services.AddTraderModule(configuration);      // Application + Infrastructure
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
| `IUserContextCache` | Singleton | User context cache (TTL 5min) |
| `ITraderUnitOfWork` | Scoped | EF DbContext |
| Stageها (1-4) | Scoped | DbContext + UoW |
| `IStrategyEvaluationService` | Scoped | DbContext + UoW (برای Cacheها) |
| `TradingRunManager` | Scoped | UoW |
| `StrategyFactory` | Singleton | Stateless (N1: فقط در Application) |
| `IStrategyRegistration` | Singleton | Stateless |
| `IIndicator` | Singleton | Stateless |
| `IOrderSizeCalculator` | Scoped | Stateless (اما Interface Scoped برای Potential DbContext) |
| `IOrderDispatcherResolver` | Scoped | Resolve Live/Paper |

---

## 17. جریان یک Tick کامل

### فرض: بازار باز، یک Run فعال، RSI/فولاد M5، Run Capital = 100M، Strategy strength = 0.6، Multiplier = 1.5

```
T=08:30:00  Worker Tick #1 (Cold Start)
    │
    ├─→ Stage 1: Fetch Snapshot فولاد (قیمت=50000)
    │       └─→ Save MarketSnapshot در DB
    │       └─→ SnapshotStore.Upsert
    │
    ├─→ Stage 2 (via StrategyEvaluationService):
    │       ├─→ CandleCache miss → Fetch 64 Candle M5 از Broker
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
    ├─→ Stage 2: Candle Cache hit (تا 08:35:30) → RSI (45) → HOLD
    └─→ ...
    │
    ▼
T=08:35:30  Worker Tick N
    │
    ├─→ Stage 2:
    │       ├─→ Cache expired (کندل 08:35 بسته شد) → Fetch → RSI = 28
    │       ├─→ Signal BUY (strength=0.6)
    │       ├─→ SaveChanges (Evaluation + Signal)
    │       └─→ Channel Push
    │
    ├─→ Stage 3:
    │       ├─→ Consume Signal
    │       ├─→ Capital = 100M ریال (از Broker یا Fixed)
    │       ├─→ AllocatedForSymbol = 100M × 100% = 100M
    │       ├─→ TargetCapital = 100M × 0.6 × 1.5 = 90M
    │       ├─→ Committed = 0
    │       ├─→ Delta = 90M
    │       ├─→ MinDelta = 100M × 3% = 3M ✓
    │       ├─→ Price = 50000 (از Snapshot ریالی)
    │       ├─→ Quantity = floor(90M / (50000 × 1.0037)) = 1793
    │       ├─→ SL = 50000 × 0.8 = 40000
    │       ├─→ TP = 50000 × 1.5 = 75000
    │       ├─→ OrderIntent + BrokerOrder.CreatePending
    │       └─→ Broker API: SendBuyOrder → BrokerOrderId=abc123
    │
    └─→ Stage 4: Order باز نیست در DB (همین الان ساخته شد)
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
T=08:40:05  Worker Tick M+1
    │
    └─→ Stage 4:
            ├─→ Order باز دیگر نیست (Filled)
            ├─→ SL/TP Check (مستقل از Order activity — C2)
            │       ├─→ Position: qty=500, current=51000
            │       ├─→ SL hit? (51000 <= 40000) → NO
            │       ├─→ TP hit? (51000 >= 75000) → NO
            │       └─→ هیچ
    │
    ▼
T=10:15:00  Worker Tick ...
    │
    └─→ Stage 4:
            ├─→ SL/TP Check:
            │       ├─→ Snapshot: price=76000 (> TP=75000)
            │       ├─→ ShouldTriggerTakeProfit → true
            │       ├─→ Sell OrderIntent (qty=500, price=76000, IsPaper=from position)
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
            │       └─→ RealizedPnL = FillMath.CalculateRealizedPnL(
            │               avg=50000, exit=76000, qty=500) = 13M ریال
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

### تغییرات نسبت به نسخه‌ی قبلی

- **`Quantity`:** قبلاً ۲۰۰۰، حالا ۱۷۹۳ (Stepped Buying + Exact formula)
- **SL/TP check در Tick M+1:** قبلاً Skip می‌شد (چون openOrders=0)، حالا اجرا می‌شود
- **`RealizedPnL`:** از `FillMath.CalculateRealizedPnL` می‌آید (D10)

---
### Backtest Two-Stream (D57)

Live و Backtest از همان `IStrategy`, `FillMath`, `IOrderSizeCalculator` استفاده می‌کنند، ولی:

| Live Stream | Backtest Two-Stream |
|---|---|
| Snapshot (IRR) → sizing | `Candle` (IRR) → sizing |
| Candle (transformed) → strategy | `StrategyCandle` (transformed) → strategy |
| IRR fill، IRR cash، IRR PnL | IRR fill، IRR cash، IRR PnL |

**در `SimulateAsync`:**
- `primaryCandlesIrr` و `primaryCandlesStrategy` جدا نگه‌داری می‌شوند
- `BacktestCandleEvent` هر دو candle را دارد (paired by index)
- `state.LastPrices` و `ExecuteFill` و `_sltpResolver` فقط از `Candle` (IRR) استفاده می‌کنند
- `runningCandles` (ورودی Strategy) از `StrategyCandle ?? Candle`
- `SubCandles` (فقط برای SL/TP) — بدون transform، IRR
- Final close از `primaryCandlesIrr`

**پیامد:** `TargetCurrency=USD` فقط RSI روی قیمت دلاری محاسبه می‌کند، ولی خرید/فروش در ریال انجام می‌شود. تجربه‌ی واقعی: قبل از این fix، Backtest با سرمایه ۱۰۰M IRR و TargetCurrency=USD، Entry/Exit Price را ۰.۰۰۰۴ نمایش می‌داد (dollar price) — یعنی سرمایه‌ی ریالی به‌عنوان USD تفسیر می‌شد.
## 18. Lifecycle یک معامله

```
[1] Signal
    └─→ Action=Buy, Strength=0.6, SourceEvaluationId
    │
    ▼
[2] OrderIntent
    └─→ Run, Account, Isin, Price, Quantity, SL, TP, IsPaper
    └─→ State: Pending → Dispatched → Acknowledged
    │
    ▼
[3] BrokerOrder
    └─→ BrokerOrderId, IsPaper
    └─→ State: Pending → Open → PartiallyFilled → Filled
    │
    ▼
[4] OrderTrade (multiple)
    └─→ TradeNumber (unique), Price, Quantity, TradeDate
    └─→ IsCanceled (for cancellations)
    │
    ▼
[5] Position (accumulated from trades via FillMath)
    └─→ State: Open → PartiallyClosed → Closed
    └─→ AverageEntryPrice = FillMath.CalculateWeightedAveragePrice
    └─→ RealizedPnL = FillMath.CalculateRealizedPnL
    └─→ StopLossPrice, TakeProfitPrice
    └─→ IsPaper
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
        Mitigation: Broker Reconciliation (A1) — در Startup اجرا می‌شود
                    Mismatch را در TradingEvents ثبت می‌کند (Log-only)
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

### Scenario G (C4): Position Overshoot در Stage 4

```
State: Broker یک Sell بزرگ‌تر از Position DB گزارش داد
Action: 1. Fill اعمال نمی‌شود (Position در known good state)
        2. TradingEvent (Severity=Critical, Type=PositionOvershootDetected)
        3. Run → Faulted
        4. Run از Cache خارج می‌شود
        5. Trade record persist می‌شود (audit trail)
        User: TradingEvents را چک کن، Position را دستی Fix کن، Run را Start کن
```

---

## 20. نکات مهم و محدودیت‌ها

### Phase A (Foundation Hardening) — ✅ Complete

✅ Broker Reconciliation (Log-only, A1)
✅ Sub-Candle Strategy Support (A2)
✅ Graceful Shutdown (A3, Polly DbRetry)
✅ Health Checks (5 Components, A4)
✅ Retention (Scheduler Recurring, A5)
✅ Unit Tests (163 tests, A6)

### Phase D (Risk & Safety) — ✅ Complete

✅ Kill Switch (Global + Per-Account, PR 14) — Events emit شد (PR 20)
✅ Daily Loss Limit (per-account, hybrid, PR 15)
✅ Max Position Size (Run-based, in OrderSizeCalculator, PR 16)
✅ Circuit Breaker (per (Run, Account), lazy reset, PR 17)
✅ Rate Limiter (per-account sliding, PR 18)
✅ Per-Account Capital Allocation (PR 19) — Fixed/Dynamic per TradingRunAccount
✅ Risk Dashboard UI (PR 21) — Kill Switch + Circuit Breaker + Active Circuits
✅ Risk Resource Seed (Resource + Permission + Menu)

### Phase B — ✅ B1 Complete، B2 Pending

**B1 (TGJU Import + Historical Refactor) — ✅ Complete (D49, D60, D61)**
- `PriceSource` + `PriceCollectionJob` (Backfill/Recurring، Scheduler-only)
- `TgjuApiClient` در ExternalApiClient layer (نه Infrastructure — D38 extension)
- `CollectPriceJobHandler` (idempotent، Mode dispatch، MarkFaulted+rethrow)
- Historical Data Refactor همگام (D49)
- Bug fixes: minRate seed، sort order (oldest-first)، `HitBoundary` Backfill، per-source boundary، `SaveChangesAsync` بدون `UpdateAsync`، `batch.Count < BatchSize` premature break
- **B2 (پس از Paper Week):** Telegram + Multi-asset

### Phase C-lite.1 + C-lite.2 — ✅ Complete (D52, D53, D54)
- Order Management UI (`/trader/orders`)
- Order Preset با Strategy Resolution (Fixed/HighAllowed/LowAllowed/LastTraded/Closing/FirstTraded، Fixed/Max/Min Quantity)
- Resolve با live API + fallback (Closing × Range)
- Range Split (Up/Down nullable = unlimited)

### Runtime Correctness — ✅ Complete (D59)
- PositionCloser Paper/Live Segregation
- OrderLifecycleStage Throttle Singleton
- ExecutionCoordinator No-Swallow Persist

### Backtest Correctness — ✅ Complete (D55-D58)
- 8 باگ Critical/High (BC1-BC8)
- 4 باگ اضافی کشف در حین تست (781% bug، TargetCurrency leak، Recurring 0-insert، 2024 currency)
- Two-Stream Architecture
- Backtest Parity Checklist در `docs/BACKTEST-PARITY.md`

### Phase C (کامل) — آینده
- Source Filter، SchedulePlan Preset Integration، Order Modify/Cancel، Partial Fill UI

### Phase E — آینده
- Ensemble، Trailing SL، AI/ML Pipeline، Multi-Instance

### محدودیت‌های فعلی

⚠️ **Cash مشترک بین Runها** — D30
⚠️ **Cash Cache Manual Trade** — تا ۳۰ دقیقه (D33)
⚠️ **Multi-Currency Triangulation** — Pair مستقیم/معکوس فقط
⚠️ **Bonus/StockRight Reconciliation** — نادیده
⚠️ **Paper Fill فوری** — بدون Partial Fill (D23)
⚠️ **PriceCollection Source Filter در Transformer** — D40 ext → Phase C
⚠️ **Backtest Risk Guards** — KillSwitch/DailyLoss/CircuitBreaker/RateLimiter اجرا نمی‌شوند (D55)
⚠️ **Backtest SL/TP on Add** — SL/TP از avg اولیه (D56)
### نکات مهم عملیاتی

🔸 **Logs:** همه‌چیز در `Logs/app-*.log` با CorrelationId
🔸 **Debug:** `Trader.Infrastructure.*` را در `appsettings.Development.json` روی Debug
🔸 **Health Check:** `GET /health/trader` — Composite (DB + Runtime + Backtest + Historical + Broker + MarketData)
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

-- Tradeهای Paper (audit)
SELECT * FROM trader.BrokerOrders WHERE IsPaper = 1 ORDER BY SubmittedAt DESC;

-- Position Overshoot Events
SELECT TOP 20 * FROM trader.TradingEvents
WHERE Type = 80    -- PositionOvershootDetected
ORDER BY OccurredAt DESC;

-- Reconciliation Mismatches
SELECT TOP 20 * FROM trader.TradingEvents
WHERE Type IN (71, 73)   -- ReconciliationCompleted، ReconciliationMismatchFound
ORDER BY OccurredAt DESC;
```

### هشدارهای Production

⚠️ **قبل از پول واقعی** — با مبلغ کم تست کن (۱-۱۰ میلیون ریال)
⚠️ **Paper Mode** — یک هفته Paper Trading قبل از Live واقعی
⚠️ **Target Currency** — تست کن که Order با IRR ثبت می‌شود (نه Converted)
⚠️ **SL/TP** — یک Position باز کن و Trigger کن (مصنوعی)
⚠️ **Recovery** — برنامه را Restart کن وسط معامله، ببین چه می‌شود
⚠️ **WAF** — بعد از ساعت ۱۲:۳۵ (بسته بازار) لاگ را چک کن که WAF Error نباشد
⚠️ **Reconciliation** — بعد از یک هفته، `POST /api/Trader/Reconciliation/Run` بزن و Mismatchها را بررسی کن

---
---

## Appendix — Platform Dependencies

### Scheduler Module Capabilities

Trader از `Scheduler` Module برای Recurring Jobها استفاده می‌کند. Capabilityهای آن:

| Capability | Status | Used In Trader |
|---|---|---|
| `ScheduleRecurringAsync(payload, pattern, startAt, tz, maxFailures)` | ✅ | A5 (Retention), B1 (PriceCollection) |
| `ScheduleAsync(payload, fireAt, queue)` (One-off) | ✅ | B1 (Backfill) |
| `Pause/Resume/Cancel RecurringAsync(id)` | ✅ | B1 (Job Start/Stop) |
| `QueryRecurringAsync(filter)` | ✅ | B1 (Job List) |
| `RecurrencePattern.Daily/Weekly/Monthly` | ✅ | A5 |
| **`RecurrencePattern.Minutely/Hourly`** | ✅ (2026-11-09) | B1 (Telegram polling، Live rates) |
| **`RecurrencePattern.Cron(expression)`** | ✅ (Cronos 0.8.4, 2026-11-09) | B1+ (Complex schedules) |
| Pre-fire buffer (default 15s) + PreciseDelay | ✅ | تمام Recurring |
| Auto-reschedule بعد از success/failure | ✅ | تمام Recurring |
| Max failures (default 10) → auto Pause | ✅ | تمام Recurring |

**Semantics:**
- **Minutely/Houly:** Real-time interval، DST-free (anchor + k×interval in UTC)
- **Daily/Weekly/Monthly:** Time-of-day anchor، DST-aware (local time preserved)
- **Cron:** Cronos-based، DST-aware، 5-field (minute hour day-of-month month day-of-week)

**Backward Compat:** enum values 0/1/2 (Daily/Weekly/Monthly) بدون تغییر. ردیف‌های DB بدون Migration Parse می‌شوند.

**Scheduler.Tests:** 28 تست (Minutely/Hourly/Cron + Daily/Weekly/Monthly regression + JSON compat).

## پیوست: فایل‌های کلیدی

### Domain

```
Trader.Domain/
├── Entities/
│   ├── TradingRun.cs               # Aggregate Root
│   ├── Instrument.cs               # هویت نماد + OptionContract (Owned)
│   ├── TradingSchedule.cs          # ساعت معاملاتی
│   ├── Signal.cs                   # خروجی Strategy
│   ├── OrderIntent.cs              # تصمیم اجرا
│   ├── BrokerOrder.cs              # سفارش واقعی
│   ├── OrderTrade.cs               # یک Fill
│   ├── Position.cs                 # موقعیت باز
│   ├── MarketSnapshotRecord.cs     # تاریخچه قیمت
│   ├── StrategyEvaluation.cs       # تاریخچه تحلیل
│   ├── CandleRecord.cs             # کندل تاریخی (TimeFrame enum)
│   ├── CurrencyRate.cs             # نرخ ارز
│   ├── TradingEvent.cs             # Event Logging
│   ├── TraderAccount.cs            # حساب کارگزاری
│   └── Backtest*.cs                # Backtest entities
├── Calculations/
│   └── FillMath.cs                 # ← NEW (M5، D10): Pure fill math
├── ValueObjects/
│   ├── Price.cs                    # readonly struct
│   ├── Quantity.cs                 # readonly struct
│   ├── Money.cs                    # class (EF OwnsOne)
│   ├── CapitalAllocation.cs        # Fixed/Dynamic
│   ├── StopLossConfig.cs           # Percentage/Absolute
│   ├── TakeProfitConfig.cs         # Percentage/Absolute
│   ├── SymbolAllocation.cs         # Percentage
│   ├── SignalStrength.cs           # [0, 1]
│   ├── OptionContract.cs           # Option details
│   └── CurrencyCode.cs             # ← NEW (M10، D4): Whitelist
└── Enums/
    ├── TradingRunState.cs
    ├── ExecutionMode.cs             # ← NEW: Live/Paper
    ├── OrderSide.cs
    ├── SignalAction.cs
    ├── TimeFrame.cs
    ├── TradingEventType.cs          # شامل PositionOvershootDetected (80)
    └── ...
```

### Application

```
Trader.Application/
├── Abstractions/
│   ├── Strategies/
│   │   ├── IStrategy.cs            # شامل ExtraCandleRequirements (A2)
│   │   ├── StrategyRegistration.cs
│   │   ├── StrategyInput.cs        # شامل ExtraTimeFrames (A2)
│   │   ├── StrategyEvaluationResult.cs
│   │   ├── CandleRequirement.cs    # ← NEW (A2)
│   │   └── ...
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
│   │   ├── ICommittedCapitalCalculator.cs
│   │   ├── IOrderSizeCalculator.cs        # BuySizeInput (AvailableCash: decimal?)
│   │   ├── IOrderDispatcherResolver.cs
│   │   ├── IBrokerOrderDispatcher.cs
│   │   └── IExecutionCoordinator.cs
│   ├── Positions/
│   │   ├── IPositionManager.cs
│   │   └── IPositionCloser.cs
│   ├── Runtime/
│   │   ├── IMarketDataIngestionStage.cs
│   │   ├── IStrategyEvaluationStage.cs
│   │   ├── IStrategyEvaluationService.cs  # ← NEW (M6): منطق ارزیابی
│   │   ├── StrategyEvaluationBatchResult.cs # ← NEW
│   │   ├── IExecutionStage.cs
│   │   └── IOrderLifecycleStage.cs
│   └── Brokers/
│       ├── IBroker.cs
│       ├── IBrokerClientFactory.cs
│       ├── IBrokerSessionProvider.cs
│       ├── IBrokerSessionCache.cs
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
├── Specifications/                 # ← SQL-side filtering
│   ├── Backtests/
│   ├── Positions/
│   ├── TradingRuns/
│   ├── Orders/
│   └── ...
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
│   ├── TradingRuntimeWorker.cs             # Per-stage timing (N4)
│   ├── TradingRunManager.cs
│   ├── RunExecutionContextCache.cs
│   ├── SignalChannel.cs
│   ├── Services/
│   │   └── StrategyEvaluationService.cs    # ← NEW (M6): منطق ارزیابی
│   ├── Stages/
│   │   ├── MarketDataIngestionStage.cs
│   │   ├── StrategyEvaluationStage.cs      # ~140 LOC (قبلاً 480)
│   │   ├── ExecutionStage.cs
│   │   └── OrderLifecycleStage.cs          # SL/TP مستقل (C2)
│   └── Recovery/
│       └── RuntimeRecoveryService.cs
├── MarketData/
│   ├── BrokerMarketDataProvider.cs
│   ├── InMemoryMarketSnapshotStore.cs
│   ├── InMemoryCandleCache.cs
│   ├── CandlePersistenceService.cs
│   ├── CandleRecordMapper.cs
│   ├── IranMarketSessionProvider.cs
│   ├── TradingScheduleCache.cs             # SQL-side (M1)
│   ├── CurrencyCandleTransformer.cs        # Per-Candle (D13)
│   ├── DbCurrencyRateProvider.cs
│   └── CurrencyRateCache.cs
├── Execution/
│   ├── OrderIntentBuilder.cs
│   ├── FixedCapitalAllocator.cs
│   ├── DynamicCapitalAllocator.cs
│   ├── CompositeCapitalAllocator.cs
│   ├── CommittedCapitalCalculator.cs
│   ├── OrderSizeCalculator.cs              # FillMath.CalculateMaxBuyQuantity (M7)
│   ├── BrokerOrderDispatcher.cs            # Live
│   ├── PaperBrokerOrderDispatcher.cs       # ← Paper (D23)
│   ├── OrderDispatcherResolver.cs          # Live/Paper
│   └── ExecutionCoordinator.cs             # Multi-Account fan-out (D5)
├── Positions/
│   ├── PositionManager.cs                  # FillMath + Overshoot (C4)
│   └── PositionCloser.cs
├── Brokers/
│   ├── BrokerSessionProvider.cs
│   ├── InMemoryBrokerSessionCache.cs
│   └── EasyTrader/
│       └── Capabilities/...
├── Backtesting/
│   ├── BacktestEngine.cs                   # FillMath (M5)
│   ├── BacktestWorker.cs
│   ├── BacktestEventLogger.cs
│   ├── SubCandleResolver.cs
│   ├── OrderFillSimulator.cs
│   └── BacktestMetricsCalculator.cs
├── Reconciliation/
│   └── ReconciliationService.cs            # Log-only (D24)
├── Health/
│   └── *HealthCheck.cs
└── Services/
    ├── InstrumentService.cs
    ├── TradingRunService.cs
    ├── SignalQueryService.cs
    ├── PositionQueryService.cs
    ├── TradingScheduleService.cs
    ├── CurrencyRateService.cs
    ├── BacktestRunService.cs
    └── ...
```

### Tests

```
Trader.Tests/
├── Domain/
│   ├── Entities/
│   │   ├── PositionTests.cs
│   │   └── TradingRunTests.cs
│   ├── ValueObjects/
│   │   ├── PriceTests.cs
│   │   ├── CurrencyCodeTests.cs
│   │   └── ...
│   └── Calculations/
│       └── FillMathTests.cs
├── Application/
│   ├── Indicators/
│   │   └── RsiCalculatorTests.cs
│   └── Strategies/
│       └── RsiStrategyTests.cs
└── Infrastructure/
    ├── Execution/
    │   └── OrderSizeCalculatorTests.cs
    ├── Positions/
    │   └── PositionManagerTests.cs          # C4 overshoot path
    └── Runtime/
        └── StrategyEvaluationStageTests.cs  # 7 integration tests
```
