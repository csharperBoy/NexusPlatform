# Handoff Document — Project NexusPlatform / Trading Module


```markdown
# NEXUSPLATFORM — TRADING MODULE — HANDOFF DOCUMENT

> این سند وضعیت کامل پروژه را در نقطه‌ی فعلی توضیح می‌دهد.
> برای ادامه‌ی کار در چت جدید، این فایل + فایل‌های لیست‌شده در انتها را بفرست.
> آخرین به‌روزرسانی: ۱۴۰۵/۰۸/۱۳ (Phase A3 — Graceful Shutdown)

---

## 1. PROJECT OVERVIEW

**NexusPlatform** یک پلتفرم Modular .NET است که شامل Moduleهای زیر است:
- Core, Event, StreamBus, Identity, Base, Audit, Navigation, Authorization, Cach (Redis), Scheduler, Trader

**Trading Module** یکی از Moduleهای داخل همین پلتفرم است و به‌عنوان یک Module ساخته می‌شود، نه سیستم مستقل.

### هدف Trading Module
یک Algorithmic Trading System برای بازار سرمایه ایران که:
1. Market Data از Broker/TSETMC دریافت کند
2. Strategy‌ها را روی داده اجرا کند
3. Signal تولید کند
4. بر اساس Account + Execution Settings سفارش بده
5. Order Lifecycle را مدیریت کند
6. Position/PnL را دنبال کند
7. برای AI/ML آینده داده‌ی تاریخی جمع کند

### توسعه‌پذیری آینده
- بازارها: Forex, Crypto
- Instrumentها: Stock (فعلاً), Option (فاز ۱ باید پشتیبانی شود), Future, Bond
- Brokerها: Mofid/EasyTrader (فعال), Agah, سایر
- Market Data: Broker API (فعال), TSETMC, Providers دیگر
- Strategyها: RSI (فعال), MACD, OrderBook, AI

---

## 2. TECH STACK + CONVENTIONS

### Backend
- **.NET 8** + **ASP.NET Core**
- **Clean Architecture** (Domain / Application / Infrastructure / Presentation)
- **DDD** + **CQRS** (MediatR) + **Result<T>** pattern
- **EF Core 8** + **SQL Server**
- **Outbox Pattern** (از Event Module)
- **Scheduler Module** (Recurring + One-Off Jobs)
- **Redis** (Cach Module — فعلاً استفاده نمی‌شود در Trading hot path)

### Frontend
- **React 19** + **TypeScript** + **Vite** + **pnpm workspace**
- **Zustand** + **TanStack Query** + **React Router 7**
- **Tailwind CSS 4**
- **Recharts** (backtest charts)
- Modular Architecture (feature-based)

### Conventions (CRITICAL — پایبند باش)
1. **هر Update از الگوی `Optional<T>` استفاده کند** — فیلدهای ارسال‌نشده تغییر نمی‌کنند
2. **Handler → CommandService/QueryService → UnitOfWork** (نه مستقیم به UoW)
3. **هر Contract جدید:** Interface در Application، Impl در Infrastructure
4. **هر Aggregate Root** از `BaseEntity` + `IAuditableEntity` (+ `IOwnerableEntity` اگر لازم)
5. **Domain Events** به Outbox در `SaveChangesAsync` منتقل می‌شوند (EfUnitOfWork)
6. **Shared Logic** (Strategy, Indicator, OrderSizeCalculator) بدون I/O است تا Backtest + Live + Paper از همان استفاده کنند
7. **`_uow.Context` direct access** فقط در Backtest/Reconciliation (خواندن انبوه، بدون tracking)
8. **همه‌چیز Log شود** — `ITradingEventLogger` روی Stages، `ILogger` روی Services
9. **قبل از کد، طرح + تأیید** — Overengineering ممنوع

---

## 3. ARCHITECTURE — PIPELINE

```
┌──────────────────────────────────────────────────────┐
│ TradingRuntimeWorker (BackgroundService, هر 10s)     │
├──────────────────────────────────────────────────────┤
│ Stage 1: MarketDataIngestionStage                    │
│   - Snapshot از Broker → InMemoryMarketSnapshotStore │
│                                                       │
│ Stage 2: StrategyEvaluationStage                     │
│   - Candle (Primary + Sub-Candle via ICandleCache)   │
│   - Currency Transform (اگر Run.TargetCurrency set)  │
│   - Strategy.Evaluate → Signal                       │
│   - Save StrategyEvaluation + Signal + Channel Push  │
│                                                       │
│ Stage 3: ExecutionStage                              │
│   - Channel drain (drain → ExecutionCoordinator)    │
│   - OrderIntentBuilder (Stepped Buying + MinDelta)   │
│   - IOrderDispatcherResolver (Live vs Paper)         │
│   - Save OrderIntent + BrokerOrder                   │
│                                                       │
│ Stage 4: OrderLifecycleStage (هر 5s داخلی)          │
│   - Poll Order History از Broker                     │
│   - Sync Trades → PositionManager.ApplyTradeAsync    │
│   - SL/TP Monitoring (auto-close)                    │
└──────────────────────────────────────────────────────┘
```

### Lifecycle
- **Startup Recovery**: Runهای `Running` را Restore می‌کند + Unconsumed Signals Re-push + Broker Reconciliation
- **Graceful Shutdown**: (Phase A3 — پیاده نشده)

### Modes
- **Live**: سفارش واقعی به Broker، پول واقعی
- **Paper**: `PaperBrokerOrderDispatcher` — Fill آنی، بدون Broker، `IsPaper=1` در DB
- **Backtest**: `BacktestEngine` — Offline replay از Candleهای DB

---

## 4. DOMAIN MODEL — Entities

### Aggregate Roots / Entities
| Entity | مسئولیت | وضعیت |
|---|---|---|
| `Instrument` | هویت نماد (ISIN, Type, Market, Commission, Min/Max Order, TradingSchedule) | ✅ |
| `TradingRun` | Run با Strategy + Symbols + Accounts + ExecutionSettings + ExecutionMode (Live/Paper) | ✅ |
| `Signal` | Action + Strength + StrategyKey + SourceEvaluationId | ✅ |
| `OrderIntent` | تصمیم اجرا (Side, Price, Qty, SL/TP, IsPaper) | ✅ |
| `BrokerOrder` | سفارش واقعی/شبیه (BrokerOrderId, State, FilledQuantity, IsPaper) | ✅ |
| `OrderTrade` | یک Fill از BrokerOrder (TradeNumber, Price, Qty) | ✅ |
| `Position` | موقعیت باز/بسته (AverageEntryPrice, RealizedPnL, SL/TP, IsPaper) | ✅ |
| `StrategyEvaluation` | هر Evaluate (FeaturesJson, Action, Strength, OutcomeJson) | ✅ |
| `MarketSnapshotRecord` | Snapshot تاریخی | ✅ |
| `CandleRecord` | Candle تاریخی (per Isin, TF, OpenTime) | ✅ |
| `CurrencyRate` | نرخ ارز (FromCurrency, ToCurrency, Rate, EffectiveAt) | ✅ |
| `TradingSchedule` | ساعت معاملاتی (TSE-REGULAR, TSE-OPTIONS, TSE-LEVERAGED, TSE-AYAR) | ✅ |
| `TradingEvent` | Event Logging برای Live (Stage, Type, Severity, Message, PayloadJson) | ✅ |
| `BacktestRun` | Snapshot Config + Metrics + Status | ✅ |
| `BacktestSymbol` | نمادهای Backtest + Allocation | ✅ |
| `BacktestTrade` | Round-trip Trade (EntryFillsJson, NetPnL, ExitReason) | ✅ |
| `BacktestDailyEquity` | Equity Curve روزانه | ✅ |
| `BacktestEvent` | Event Logging برای Backtest | ✅ |
| `HistoricalDataCollectionJob` | Job جمع‌آوری Candle تاریخی | ✅ |
| `InstrumentOrderPreset` | تنظیمات پیش‌فرض سفارش (UI ندارد) | ✅ |

### Enums مهم
`ExecutionMode { Live=0, Paper=1 }`, `TradingRunState`, `SignalAction { Hold=0, Buy=1, Sell=2 }`, `OrderSide`, `OrderStateKind`, `OrderIntentState`, `PositionState`, `TimeFrame`, `CapitalAllocationMode`, `OrderMode`, `MarketCloseBehavior`, `StopRunBehavior`, `TradingStage`, `TradingEventType` (~43 نوع), `TradingEventSeverity { Debug, Info, Warning, Error, Critical }`, `BacktestStatus`, `BacktestExitReason`, `CollectionJobStatus`, `InstrumentType`, `MarketType`, `BrokerType`, `OptionType`, `BacktestEventType`, `BacktestEventSeverity`

---

## 5. KEY INTERFACES (مرزها)

### Broker (External)
```
IBroker { BrokerType }
  IBrokerSessionManager     → Login, Serialize, Deserialize, MeasureLatency
  IBrokerTradingCommands    → SendBuyOrder, SendSellOrder, BuildOrderRequest
  IBrokerAccountReader      → GetCashBalance, GetPortfolioPerformance
  IBrokerMarketDataReader   → GetSymbolInfo, GetCandles, GetCandlesUntil, GetBatchMarketData
  IBrokerOrderHistoryReader → GetOrderHistory, GetOrderTrades
  IBrokerAnalysisReader     → GetTechnicalAnalysis, GetFundamentalAnalysis, ...
  IBrokerWatchlistReader    → GetMarketWatch, GetIndustryPositiveNegative
  IBrokerFinanceCommands    → RequestWithdrawal, CancelPayment
IBrokerClientFactory        → T Get<T>(BrokerType)
IBrokerSessionProvider      → GetSessionAsync(accountId), Invalidate(accountId)
IBrokerSessionCache         → TryGet, Set, Invalidate, AcquireLockAsync
```

### Market Data
```
IMarketDataProvider         → GetSnapshot, GetBatchSnapshots, GetCandles(isin, tf, count, toTime?)
IMarketDataProviderResolver → Resolve(MarketType)
IMarketSnapshotStore        → Upsert, GetLatest, GetLatestBatch, RetainOnly
ICandleCache                → GetOrLoadAsync(isin, tf, requiredCount)
ICandlePersistenceService   → PersistAsync (closed only)
ICandleTransformer          → TransformAsync(isin, candles, targetCurrency, atTime) → CandleTransformationResult
ICurrencyRateProvider       → GetRateAsync, GetRateHistoryAsync
ITradingScheduleCache       → GetForIsinAsync(isin) → TradingSchedule?
IMarketSessionProvider      → IsMarketOpen, GetNextMarketOpen
```

### Strategy / Indicator
```
IStrategy {
  Key, DisplayName, RequiredCandleCount, RequiresClosedCandlesOnly,
  ExtraCandleRequirements: IReadOnlyList<CandleRequirement>,   // ← A2
  Evaluate(StrategyInput) → StrategyEvaluationResult
}
IIndicator<TConfig, TResult> { Compute(IndicatorInput, TConfig) → TResult }
IStrategyFactory { Create(key, config), DeserializeConfig(key, json), GetRegisteredStrategies() }
IStrategyRegistration (marker، در DI Register می‌شود)
StrategyInput {
  Isin, LatestSnapshot?, Candles (primary),
  ExtraTimeFrames: IReadOnlyDictionary<TimeFrame, IReadOnlyList<Candle>>?,  // ← A2
  Configuration, TimeFrame, EvaluatedAt
}
StrategyEvaluationResult { Signals, Features? }
CandleRequirement(TimeFrame, Count, ClosedOnly=true)   // ← A2
```

### Execution
```
IOrderSizeCalculator {
  CalculateBuy(BuySizeInput) → OrderSizeResult
  CalculateSell(SellSizeInput) → OrderSizeResult
}
ICapitalAllocator             → GetAllocatableCapitalAsync (Fixed/Dynamic/Composite)
ICommittedCapitalCalculator   → CalculateForSymbolAsync
IOrderIntentBuilder           → BuildAsync(signal, context, accountId) → OrderIntent?
IBrokerOrderDispatcher        → DispatchAsync(intent, brokerType, session) → BrokerOrder
IOrderDispatcherResolver      → Resolve(isPaper) → IBrokerOrderDispatcher   // ← Paper
IExecutionCoordinator         → HandleAsync(SignalEnvelope), CloseRunPositionsAsync
```

### Position
```
IPositionManager    → ApplyTradeAsync, ApplyTradeCancelAsync, (UpdateEvaluationOutcome internal)
IPositionCloser     → CloseRunPositionsAsync(runId), ClosePositionsAsync(ids)
```

### Runtime
```
IMarketDataIngestionStage, IStrategyEvaluationStage, IExecutionStage, IOrderLifecycleStage
IRunExecutionContextCache   (Singleton)
ISignalChannel              (Singleton, Channel<SignalEnvelope>)
ITradingRunManager          → StartAsync (Idempotent), StopAsync, IsRunning
IRuntimeRecoveryService     → RecoverAsync
```

### Logging / Persistence
```
ITradingEventLogger         → Log(stage, type, severity, message, payload, correlations), FlushAsync
ITradingEventQueryService   → QueryAsync(filter), GetByIdAsync
IBacktestEventLogger        → Log(...), FlushAsync
IBacktestEventQueryService  → QueryAsync(filter)
IReconciliationService      → ReconcileAllAsync, ReconcileAccountAsync   // ← A1
```

### UnitOfWork (الگو)
```
ITraderUnitOfWork<TDbContext> : IUnitOfWork<TDbContext> {
  // Repositories (Direct): Accounts, Instruments, InstrumentOrderPresets,
  //   TradingRuns, Signals, OrderIntents, BrokerOrders, OrderTrades, Positions,
  //   SchedulePlans, ScheduledOrders, ExecutionLogs, MarketSnapshots, CandleRecords,
  //   StrategyEvaluations, TradingEvents, TradingSchedules, CurrencyRates,
  //   BacktestRuns, BacktestSymbols, BacktestTrades, BacktestDailyEquities,
  //   BacktestEvents, HistoricalDataCollectionJobs
  // SpecRepositories: TradingRunSpecs, SignalSpecs, OrderIntentSpecs, BrokerOrderSpecs,
  //   OrderTradeSpecs, PositionSpecs, InstrumentSpecs, InstrumentOrderPresetSpecs,
  //   AccountSpecs, ExecutionLogSpecs, CandleRecordSpecs, StrategyEvaluationSpecs,
  //   BacktestRunSpecs, BacktestTradeSpecs, CurrencyRateSpecs
}
```

---

## 6. RUNTIME CONFIG (appsettings.json)

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
  },
  "HistoricalData": {
    "RateLimitMs": 250,
    "MaxDaysPerJob": 730
  }
}
"Brokers": {
  "EasyTrader": {
    "BaseUrl": "https://api-mts.orbis.easytrader.ir",
    "OidcBaseUrl": "https://login.emofid.com",
    "ClientId": "easy_pkce",
    "RedirectUri": "https://d.easytrader.ir/auth-callback",
    "TimeoutSeconds": 30,
    "AppBuildNo": "53559"
  }
}
```

---

## 7. FRONTEND STRUCTURE

```
src/modules/Trader/
├── api/                    (one file per entity + backtest + currency + historical + events)
├── models/                 (one file per DTO + enums + labels/colors)
├── hooks/                  (management hooks per entity)
├── pages/
│   ├── Accounts/           ✅
│   ├── Instruments/        ✅ (with TradingSchedule, Commission, Min/Max, MinValue)
│   ├── Strategies/         ✅
│   ├── TradingRuns/        ✅ (with StrengthMultiplier, MinDelta, Currency, ExecutionMode)
│   ├── CurrencyRates/      ✅
│   ├── Positions/          ✅ (PAPER badge)
│   ├── Signals/            ✅
│   ├── Events/             ✅ (TradingEvents log viewer with filters + drawer)
│   ├── Backtests/          ✅ (Create/List/Detail/Compare + LogsTab)
│   ├── HistoricalData/     ✅ (Collection Jobs)
│   ├── SchedulePlans/      ✅ (legacy)
│   ├── ServerClock/        ✅ (legacy)
│   └── Logs/               ✅ (ExecutionLog legacy)
├── routes.tsx
└── index.ts
```

### Core Components
- `GenericCrudPage` — با `tableFeatures`: `horizontalScroll`, `stickyAction`, `classNames` (همه elementها configurable)
- `GenericAddModal`, `useGenericCrud`, `SearchableMultiSelect`, `TagInput`

---

## 8. PHASE A STATUS (Foundation Hardening)

| # | Feature | Status |
|---|---|---|
| **A1** | Broker Reconciliation | ✅ Done (Quantity + AvgPrice + 3 Mismatch kinds؛ Log-only) |
| **A2** | Sub-Candle Strategy Support | ✅ Done (`ExtraCandleRequirements` + `ExtraTimeFrames` dict) |
| **A3** | Graceful Shutdown | ✅ Done (StopAsync override + 3 shutdown events + FlushAsync retry) |
| **A4** | Health Checks per Component | ⏳ Next |
| **A5** | Retention برای BacktestEvents | ⏳ Pending |
| **A6** | Unit Tests برای Critical Logic | ⏳ Pending |

### Future Phases
- **B**: Data Infrastructure (TGJU USD Import, Data Completeness)
- **C**: Operations & Dashboard (Order Management UI, Order Preset UI, Daily PnL, Dashboard Widgets)
- **D**: Risk & Safety (Daily Loss Limit, Max Position Size, Circuit Breaker, Rate Limiter, Consecutive Loss Guard)
- **E**: Advanced Features (Multi-Indicator Strategies, Strategy Ensemble, Trailing Stop, Laddered Exit, AI/ML Pipeline, Multi-Instance Prep)

---

## 9. KEY DECISIONS RECAP

### از Master Prompt (اصول پایه)
1. Strategy از Account جداست
2. Market Data از Execution جداست
3. TradingRun مسئول Execution Context است
4. Broker پشت Abstraction با Capability Segregation
5. Future-ready, Simple Phase 1

### تصمیمات فنی مهم (نه در Master Prompt، ولی پذیرفته‌شده)
- **Stepped Buying**: `targetCapital = allocated × strength × multiplier`, delta check, `MinDeltaPercent` (default 3%)
- **Per-Candle Currency Transform**: هر Candle با نرخ تاریخی خودش (`EffectiveAt`), نه نرخ واحد
- **Candle In-Progress Filter**: `OpenTime + tfDuration <= now` قبل از دادن به RSI
- **Cache Expiry Market-Aware**: اگر بازار بسته → Cache تا باز شدن بعدی؛ اگر باز → هر Candle Close + buffer
- **OrderSizing Pure Logic**: `IOrderSizeCalculator` — Live/Paper/Backtest از همان استفاده می‌کنند
- **Paper Trading**: `PaperBrokerOrderDispatcher` — Fill آنی در `intent.Price`، بدون Slippage اضافی، `IsPaper=1`
- **Broker Reconciliation**: فقط Quantity + Weighted Avg، بدون Auto-fix در A1 (Log-only)
- **Sub-Candle**: Strategy declares `ExtraCandleRequirements`, run context has `StrategyInput.ExtraTimeFrames` (dictionary) — `null` = فقط Primary
- **Backtest ≠ Paper ≠ Live**: سه Mode مستقل از یک Domain/Service استفاده می‌کنند
- **Historical Data Collection**: On-Demand Command + Background Worker + Rate Limit
- **Event Logging**: `TradingEvents` (Live) و `BacktestEvents` جدا + Buffer + Flush در پایان Tick
- **Retention**: Scheduler Recurring Job روزانه (نه Hangfire مستقیم، نه Background Service داخلی)

### مشکلات شناخته‌شده (Backlog)
- `TraderSymbol` legacy — Refactor به `Instrument` در Phase E
- `SymbolController` legacy — حذف شده از Frontend، Backend نگه‌داری شده
- `SchedulePlan` و `ScheduledOrder` legacy — Refactor به `InstrumentOrderPreset` در Phase C
- `InstrumentOrderPreset` UI ندارد
- PnL از Broker (`TotalRealizedProfitAndLoss`) هنوز در Reconciliation مقایسه نمی‌شود (Phase A1+)
- Bonus/StockRight در Reconciliation نادیده گرفته می‌شود (ممکن است Mismatch کاذب بدهد)

- **Resilience**: هر Retry روی Database/Callهای حساس از `IReadOnlyPolicyRegistry<string>` و Polly policies پلتفرم (`DbRetry`, `DefaultRetry`) استفاده می‌کند — نه Retry سفارشی. `RetryBehavior` فقط برای MediatR؛ BackgroundServiceها (مثل `TradingEventLogger.FlushAsync`) مستقیم از Registry می‌خوانند.
---

## 10. FILES TO SEND IN NEW CHAT

### ضروری (با اولویت)
1. **این فایل (`HANDOFF.md`)**
2. **`docs/TRADING-STORY.md`** — روایت ساده‌ی Pipeline (برای context کیفی)
3. **`docs/TRADING-ARCHITECTURE.md`** — جزئیات فنی (اگر داری)

### Domain (کد کامل)
- `Trader.Domain/Enums/*.cs` (همه enumها)
- `Trader.Domain/Entities/Instrument.cs`
- `Trader.Domain/Entities/TradingRun.cs`
- `Trader.Domain/Entities/Signal.cs`
- `Trader.Domain/Entities/OrderIntent.cs`
- `Trader.Domain/Entities/BrokerOrder.cs`
- `Trader.Domain/Entities/OrderTrade.cs`
- `Trader.Domain/Entities/Position.cs`
- `Trader.Domain/Entities/TradingEvent.cs`
- `Trader.Domain/Entities/BacktestRun.cs` (+ BacktestSymbol/Trade/DailyEquity/Event)
- `Trader.Domain/Entities/HistoricalDataCollectionJob.cs`
- `Trader.Domain/Entities/CurrencyRate.cs`
- `Trader.Domain/Entities/TradingSchedule.cs`

### Application — Interfaces (الزامی)
- `Trader.Application/Abstractions/ITraderUnitOfWork.cs`
- `Trader.Application/Abstractions/Strategies/*.cs` (IStrategy, StrategyInput, StrategyEvaluationResult, IStrategyFactory, CandleRequirement)
- `Trader.Application/Abstractions/Execution/*.cs` (IOrderSizeCalculator + BuySizeInput/SellSizeInput/OrderSizeResult, ICapitalAllocator, IOrderDispatcherResolver, IOrderIntentBuilder, IBrokerOrderDispatcher)
- `Trader.Application/Abstractions/MarketData/*.cs` (IMarketDataProvider, ICandleCache, ICandleTransformer, ICurrencyRateProvider, IMarketSnapshotStore, IMarketSessionProvider, ITradingScheduleCache)
- `Trader.Application/Abstractions/Positions/*.cs`
- `Trader.Application/Abstractions/Runtime/*.cs`
- `Trader.Application/Abstractions/TradingRuns/*.cs`
- `Trader.Application/Abstractions/Backtesting/*.cs`
- `Trader.Application/Abstractions/Logging/ITradingEventLogger.cs`
- `Trader.Application/Abstractions/Reconciliation/IReconciliationService.cs`

### Infrastructure — Key Implementations (فقط برای reference، نه همه فایل‌ها)
- `Trader.Infrastructure/Runtime/Stages/StrategyEvaluationStage.cs` (بحرانی — منطق پیچیده)
- `Trader.Infrastructure/Runtime/Stages/ExecutionStage.cs`
- `Trader.Infrastructure/Runtime/Stages/OrderLifecycleStage.cs`
- `Trader.Infrastructure/Execution/OrderSizeCalculator.cs`
- `Trader.Infrastructure/Execution/OrderIntentBuilder.cs`
- `Trader.Infrastructure/Execution/ExecutionCoordinator.cs`
- `Trader.Infrastructure/Execution/PaperBrokerOrderDispatcher.cs`
- `Trader.Infrastructure/Execution/BrokerOrderDispatcher.cs`
- `Trader.Infrastructure/Positions/PositionManager.cs`
- `Trader.Infrastructure/MarketData/CurrencyCandleTransformer.cs`
- `Trader.Infrastructure/MarketData/InMemoryCandleCache.cs`
- `Trader.Infrastructure/MarketData/BrokerMarketDataProvider.cs`
- `Trader.Infrastructure/Runtime/Recovery/RuntimeRecoveryService.cs`
- `Trader.Infrastructure/Reconciliation/ReconciliationService.cs`
- `Trader.Infrastructure/Backtesting/BacktestEngine.cs`
- `Trader.Infrastructure/DependencyInjection/ServiceCollectionExtensions.cs` (Registration Mapping)

### Application — Sample Strategy
- `Trader.Application/Strategies/Rsi/RsiStrategy.cs` (+ Registration, Config, Calculator)
- `Trader.Application/Indicators/Rsi/RsiIndicator.cs`

### Persistence (فقط برای reference — اگر Migration جدید لازم شد)
- `Trader.Infrastructure/Data/TraderDbContext.cs`
- `Trader.Infrastructure/Configurations/` (اگر تعداد زیاد بود، فقط موارد اخیر)

### Frontend (فقط اگر به UI مربوطه)
- `src/modules/Trader/models/index.ts`
- `src/modules/Trader/api/index.ts`
- `src/core/components/crud/types.ts`

---

## 11. COLLABORATION MODE

### کاربر این‌ها را انتظار دارد:
1. **قبل از کد، طرح + تأیید صریح** — اگر تصمیم معماری است، اول توضیح بده
2. **کد کامل** — نه pseudo-code
3. **فایل‌به‌فایل** — namespace + path + DI + Migration، همه مشخص
4. **پاسخ مستقیم و فنی** — بدون مقدمه‌ی طولانی
5. **Commit Message در انتهای هر پیام** — user بعد از هر پیام commit می‌زند
6. **این HANDOFF را هر بار update کن** — در انتهای هر پیام، اگر تغییر کرده، نسخه‌ی به‌روز را کوتاه کن (یا فقط diff را بگو)
7. **Python/PowerShell test snippets** — اگر Diagnostic لازم بود
8. **Backward Compat**: هیچ Refactor بدون دلیل، هیچ Feature بدون نیاز

### کاربر تجربه دارد (۱۲ سال) — مثل Junior توضیح نده
- روی تصمیمات معماری مستقیم بحث کن
- اگر پیشنهاد کاربر بهینه نیست، صریح بگو چرا
- اگر Overengineering است، بگو
- اگر راه ساده‌تر هست، بگو

---

## 12. NEXT STEPS (بعد از A2)

### A3 — Graceful Shutdown (بعدی)
- `IHostedService.StopAsync` روی `TradingRuntimeWorker`
- ترتیب: Stop Heartbeat → Wait Running Tick → Flush Events → Persist Paper Positions (اگر in-memory) → Save Broker Sessions
- Outcome: بدون از دست دادن Signal/Order در Shutdown

### A4 — Health Checks
- `/health/trader` composite
- Per-component: DB, Broker Session, Market Data, Runtime Worker

### A5 — Retention برای BacktestEvents
- گسترش `TradingEventRetentionService` به `BacktestEvents` هم
- Retention جدا در Config: `Trader:Logging:BacktestEventsRetentionDays` (default 90)

### A6 — Unit Tests
- `OrderSizeCalculator` (بحرانی — Stepped Buy + MinDelta + Min/Max + Commission)
- `RsiCalculator`
- `SubCandleResolver`
- `PositionManager` (Apply Trade/Close)
- `CurrencyCandleTransformer` (Per-Candle)
- `BacktestEngine` (Simulation Loop)

---

## 13. CONTACT POINTS (اگر چت جدید)

**اگر کاربر چت جدید باز کرد و این فایل را داد:**
1. این کل context را دارد — نیاز به توضیح اضافه نیست
2. اگر فایلی کم است، از لیست بخش ۱۰ درخواست کن
3. از کاربر بپرس: **«الآن در کدام مرحله هستیم؟»** — اگر در A3، شروع کن به طراحی
4. اگر کاربر گفت «ادامه بده»، ببین `PHASE A STATUS` — اولین ⏳ = نقطه‌ی شروع
5. **قبل از هر کد، طرح کوتاه + تأیید بگیر**

---

## 14. RECENT COMPLETED WORK (آخرین ۵ پیام)

1. **A3 Graceful Shutdown** — `TradingRuntimeWorker.StopAsync` override + 3 shutdown event types (64/65/66). `FlushAsync` در `TradingEventLogger` و `BacktestEventLogger` از `IReadOnlyPolicyRegistry<string>` و Polly policy `DbRetry` (fallback: `DefaultRetry` → `NoOp`) استفاده می‌کند — **نه Retry سفارشی**. دلیل: `RetryBehavior` فقط MediatR است؛ BackgroundService از Pipeline رد نمی‌شود.
2. **A1 Reconciliation** — `ReconciliationService` + `IReconciliationService` + Command/Controller + Integration در `RuntimeRecoveryService`. Direct access به `PortfolioPerformanceDto.SymbolIsin` و `.Asset` (نه Reflection).
3. **A2 Sub-Candle** — `CandleRequirement` + `IStrategy.ExtraCandleRequirements` + `StrategyInput.ExtraTimeFrames` (dict) + `StrategyEvaluationStage` بازنویسی + `BacktestEngine` Extra TF Loading.
4. **Backtest Currency** — فقط در Virtual Mode (نه Existing Run Override).
5. **Compare UI** — multi-backtest side-by-side + multi-series Equity Chart (Recharts).
---

**پایان سند**

---

## هر بار این سند به‌روز می‌شود
- **`Last Updated`** در ابتدای فایل
- **بخش ۸ (Phase Status)**
- **بخش ۱۴ (Recent Work)** — ۵ مورد آخر
- **بخش ۱۰ (Files List)** — اگر فایل کلیدی جدید اضافه شد
```

---

## فایل‌هایی که باید در چت جدید بفرستی (خلاصه)

**همیشه:**
1. `docs/HANDOFF.md` (این سند)

**اگر Phase جدید شروع کردی:**
2. فایل‌های Domain مربوطه (اگر تغییر کرده)
3. Interfaceهای Application که مرتبط هستند
4. Impl فعلی که می‌خواهی تغییر بدهی

**اگر Migration لازم شد:**
5. `TraderDbContext.cs` + `Configurations` مربوطه

**اگر Frontend مربوطه:**
6. `models/index.ts` + `api/index.ts` + `types.ts` (Core)

**اگر مطمئن نیستی:**
- فقط `HANDOFF.md` بفرست + اسم فایل/قابلیت مورد نظر
- من از لیست بخش ۱۰ می‌پرسم کدام را لازم دارم

---

## نکته‌ی مهم برای چت جدید

**در پیام اول چت جدید بنویس:**

> «فایل HANDOFF.md را می‌فرستم. وضعیت کامل پروژه در آن است. الآن در Phase A هستیم. [اسم مرحله‌ی بعدی که در HANDOFF بخش ۸ مشخص است] ادامه بده.»

اگر مطمئن نیستی مرحله‌ی بعدی چیست، فقط بگو: «از HANDOFF ادامه بده» — من از `PHASE A STATUS` می‌بینم.

---

## از اینجا به بعد

**اعمال A1 + تست → A3 (Graceful Shutdown)**

بعد از Build + Restart، `POST /api/Trader/Reconciliation/Run` بزن و `TradingEvents` را چک کن. اگر Match/Mismatch درست Log شد → A1 تأیید. پیام بعدی: A3.

**بگو ادامه دهیم یا اول تست A1؟**