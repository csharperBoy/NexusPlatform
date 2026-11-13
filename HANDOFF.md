> **آخرین به‌روزرسانی:** ۱۴۰۴/۰۹/۲۳ — Phase C-lite.1 + C-lite.2 + Backtest Correctness + Runtime Fixes
> **وضعیت:** Phase A ✅ | Phase D ✅ | Scheduler ✅ | Phase B1 ✅ | C-lite.1/2 ✅ | Runtime Fixes ✅ | 258 Tests ✅ | CI ✅
> **گام بعدی:** C-lite.3 (Dashboard) → Paper Trading Week
---

## 1. PROJECT OVERVIEW

**NexusPlatform** یک پلتفرم Modular .NET است شامل Moduleها:
Core, Event, StreamBus, Identity, Base, Audit, Navigation, Authorization, Cache (Redis), Scheduler, Trader

**Trading Module** یکی از Moduleهای داخل همین پلتفرم — نه سیستم مستقل.

### هدف
Algorithmic Trading System برای بازار سرمایه ایران:
1. Market Data از Broker/TSETMC
2. اجرای Strategy روی داده
3. تولید Signal
4. تصمیم اجرا بر اساس Account + Execution Settings + Risk
5. مدیریت Order Lifecycle
6. Track Position/PnL
7. Persistence برای AI/ML آینده

### توسعه‌پذیری آینده
- **Markets:** Forex، Crypto (Enum آماده)
- **Instruments:** Stock، Option (فعال)، Future، Bond، Fund
- **Brokers:** Mofid/EasyTrader (فعال)، Agah
- **Market Data:** Broker API (فعال)، TSETMC
- **Strategies:** RSI (فعال)، MACD، OrderBook، AI، Ensemble

---

## 2. TECH STACK

### Backend
- **.NET 9** + **ASP.NET Core**
- Clean Architecture (Domain / Application / Infrastructure / Presentation)
- DDD + CQRS (MediatR) + Result<T>
- EF Core 9 + SQL Server
- Outbox Pattern (از Event Module)
- Scheduler Module (Recurring + One-Off Jobs)
- Polly (DbRetry، DefaultRetry policies از `IReadOnlyPolicyRegistry<string>`)
- Health Checks (Composite + per-component)
- xUnit + FluentAssertions 6.12 + Moq + EF InMemory (تست)

### Frontend
- React 19 + TypeScript + Vite + pnpm workspace
- Zustand + TanStack Query + React Router 7
- Tailwind CSS 4
- Recharts (backtest charts)

### CI
- GitHub Actions: `.github/workflows/trader-tests.yml`
- Build + Test روی Push/PR به `master`, `Trader`, `AkSteel`
- .NET 9.0.x، 158 Tests در ~2s

### Conventions
1. هر Update با `Optional<T>` — فیلد ارسال‌نشده تغییر نمی‌کند
2. Handler → CommandService/QueryService → UnitOfWork
3. هر Contract: Interface در Application، Impl در Infrastructure
4. Aggregate Root از `BaseEntity` + `IAuditableEntity` (+ `IOwnerableEntity` اگر لازم)
5. Domain Events → Outbox در `SaveChangesAsync` (EfUnitOfWork)
6. Shared Logic (Strategy، Indicator، **FillMath**، OrderSizeCalculator) — بدون I/O
7. `_uow.Context` direct فقط در Backtest/Reconciliation (خواندن انبوه)
8. همه‌چیز Log شود — `ITradingEventLogger` روی Stages، `ILogger` روی Services
9. قبل از کد، طرح + تأیید — Overengineering ممنوع

---

## 3. ARCHITECTURE — PIPELINE

```
┌──────────────────────────────────────────────────────────┐
│ TradingRuntimeWorker (BackgroundService, هر 10s)         │
│ + Per-Stage Timing (N4)                                  │
├──────────────────────────────────────────────────────────┤
│ Stage 1: MarketDataIngestionStage                        │
│   - Snapshot از Broker → InMemoryMarketSnapshotStore     │
│                                                           │
│ Stage 2: StrategyEvaluationStage (Orchestration فقط)     │
│   - IStrategyEvaluationService per (Run, Symbol)          │
│     → Load candles, Transform currency, Load extra TFs,  │
│       Evaluate, Enrich features, Create Entities          │
│   - Persist Evaluation + Signals + Push to Channel       │
│                                                           │
│ Stage 3: ExecutionStage                                  │
│   - Channel drain → ExecutionCoordinator                 │
│   - OrderIntentBuilder → IRiskGuardPipeline → Dispatch   │
│   - Live: BrokerOrderDispatcher                          │
│   - Paper: PaperBrokerOrderDispatcher (فوری Fill)        │
│                                                           │
│ Stage 4: OrderLifecycleStage (هر 5s داخلی)               │
│   - Poll Order History از Broker                         │
│   - Sync Trades → PositionManager.ApplyTradeAsync        │
│     (Overshoot → Fault، FillMath برای PnL)               │
│   - SL/TP Monitoring (مستقل از Order activity)           │
└──────────────────────────────────────────────────────────┘
```

### Risk Pipeline (بین Intent و Dispatch)
```
OrderIntent
    → KillSwitchGuard (Global + Per-Account)
    → DailyLossLimitGuard (per-Account، hybrid source)
    → CircuitBreakerGuard (per (Run, Account), lazy reset)
    → RateLimiterGuard (per-Account sliding window)
    → MaxPositionSize (داخل OrderSizeCalculator، نه Guard)
    → Dispatch
```

### Lifecycle
- **Startup Recovery:** Runهای `Running` Restore + Unconsumed Signals Re-push + Broker Reconciliation
- **Graceful Shutdown:** Polly DbRetry روی Flush

### Modes
- **Live:** سفارش واقعی
- **Paper:** Fill فوری، بدون Broker، `IsPaper=1`
- **Backtest:** Offline replay، Config Snapshot

---

## 4. DOMAIN MODEL

### Aggregates / Entities
| Entity | مسئولیت | وضعیت |
|---|---|---|
| `Instrument` | هویت نماد + Commission + Order Limits + OptionContract (Owned) | ✅ |
| `TradingRun` | State، Strategy، Symbols، Accounts، ExecutionSettings | ✅ |
| `TradingRunSymbol` | (Run, Isin) + Allocation% | ✅ |
| `TradingRunAccount` | (Run, Account) + CapitalAllocation (Owned, per-account، PR 19) | ✅ |
| `Signal` | Action + Strength + SourceEvaluationId | ✅ |
| `OrderIntent` | Side، Price، Quantity، SL/TP، IsPaper، State (incl. RiskBlocked) | ✅ |
| `BrokerOrder` | BrokerOrderId، State، FilledQty، IsPaper | ✅ |
| `OrderTrade` | Fill (TradeNumber unique، Price، Qty) | ✅ |
| `Position` | State، Qty، AvgPrice، RealizedPnL، SL/TP، IsPaper، SourceEvaluationId | ✅ |
| `StrategyEvaluation` | هر Evaluate (FeaturesJson، Action، Strength، OutcomeJson) | ✅ |
| `MarketSnapshotRecord` | Snapshot تاریخی | ✅ |
| `CandleRecord` | Candle تاریخی (TimeFrame enum) | ✅ |
| `CurrencyRate` | نرخ ارز (Reverse supported) | ✅ |
| `TradingSchedule` | ساعت معاملاتی (TSE-REGULAR/OPTIONS/LEVERAGED/AYAR) | ✅ |
| `TradingEvent` | Event Logging (Correlation IDs، Persian messages) | ✅ |
| **`RiskKillSwitch`** | Global + Per-Account Emergency Stop (PR 14) | ✅ |
| **`RunAccountDailyEquitySnapshot`** | Opening Equity per (Run, Account, Day) (PR 15) | ✅ |
| **`RunAccountLossStreak`** | Consecutive Losses per (Run, Account) + Circuit state (PR 17) | ✅ |
| `BacktestRun` | Snapshot Config + Metrics | ✅ |
| `BacktestSymbol`/`Trade`/`DailyEquity`/`Event` | Backtest children | ✅ |
| `HistoricalDataCollectionJob` | Job جمع‌آوری Candle تاریخی | ✅ |
| `InstrumentOrderPreset` | تنظیمات پیش‌فرض سفارش دستی (UI ندارد) | ✅ |

### Value Objects
- `Price` (readonly struct)، `Quantity` (readonly struct)، `Money` (class، Owned)
- `CapitalAllocation` (Run-level، فقط در `BacktestRun` استفاده می‌شود)
- **`AccountCapitalAllocation`** (per-account، PR 19: Fixed/Dynamic + Amount/Percent)
- `StopLossConfig`، `TakeProfitConfig` (Percentage/AbsolutePrice)
- `SymbolAllocation` (Percentage)
- `SignalStrength` (0,1)
- `OptionContract` (Type، Strike، Expiration، Underlying)
- `CurrencyCode` (Whitelist، PR 8)

### Domain Calculations
- `Trader.Domain/Calculations/FillMath.cs` (M5)
  - `CalculateWeightedAveragePrice` — میانگین وزن‌دار
  - `CalculateRealizedPnL` — gross PnL
  - `CalculateCommission` — قیمت × تعداد × نرخ
  - `CalculateMaxBuyQuantity` — دقیق: `floor(capital / (price × (1 + c)))`

### Enums مهم
`TradingRunState`، `ExecutionMode`، `SignalAction`، `OrderSide`، `OrderStateKind`، `OrderIntentState` (incl. `RiskBlocked=5`)، `PositionState`، `TimeFrame`، `CapitalAllocationMode`، `OrderMode`، `MarketCloseBehavior`، `StopRunBehavior`، `TradingStage` (incl. `Risk=8`)، `TradingEventType` (~52 نوع)، `TradingEventSeverity`، `BacktestStatus`، `BacktestExitReason`، `CollectionJobStatus`، `InstrumentType`، `MarketType`، `BrokerType`، `OptionType`، `BacktestEventType`، `BacktestEventSeverity`

---

## 5. KEY INTERFACES

### Broker (Capability Segregation)
```
IBroker { BrokerType }
  IBrokerSessionManager     → Login, Serialize, Deserialize, MeasureLatency
  IBrokerTradingCommands    → SendBuy/Sell, BuildOrderRequest
  IBrokerAccountReader      → GetCashBalance, GetPortfolioPerformance
  IBrokerMarketDataReader   → GetSymbolInfo, GetCandles, GetBatchMarketData
  IBrokerOrderHistoryReader → GetOrderHistory, GetOrderTrades
  IBrokerAnalysisReader     → Technical, Fundamental, IndInd
  IBrokerWatchlistReader    → MarketWatch, IndustryPositiveNegative
  IBrokerFinanceCommands    → Withdrawal, CancelPayment
IBrokerClientFactory        → Get<T>(BrokerType) [Keyed]
IBrokerSessionProvider      → GetSessionAsync(accountId), Invalidate
IBrokerSessionCache         → TryGet, Set, Invalidate, AcquireLockAsync
```

### Market Data
```
IMarketDataProvider         → Snapshot/Candles (independent از Account/Broker)
IMarketDataProviderResolver → Resolve(MarketType)
IMarketSnapshotStore        → In-memory latest snapshots
ICandleCache                → GetOrLoadAsync (Market-Aware TTL)
ICandlePersistenceService   → Persist closed candles only
ICandleTransformer          → Per-Candle currency transform (D13)
ICurrencyRateProvider       → GetRate, GetRateHistory (Reverse supported)
ITradingScheduleCache       → GetForIsinAsync → TradingSchedule?
IMarketSessionProvider      → IsMarketOpen, GetNextMarketOpen
```

### Strategy / Indicator
```
IStrategy {
  Key, DisplayName, RequiredCandleCount, RequiresClosedCandlesOnly,
  ExtraCandleRequirements: IReadOnlyList<CandleRequirement>,   // A2
  Evaluate(StrategyInput) → StrategyEvaluationResult
}
IIndicator<TConfig, TResult> { Compute(IndicatorInput, TConfig) → TResult }
IStrategyFactory { Create, DeserializeConfig, GetRegisteredStrategies }
IStrategyRegistration (marker، Assembly scan)
StrategyInput {
  Isin, LatestSnapshot, Candles (primary),
  ExtraTimeFrames: IReadOnlyDictionary<TimeFrame, IReadOnlyList<Candle>>?,
  Configuration, TimeFrame, EvaluatedAt
}
```

### Execution
```
IOrderSizeCalculator { CalculateBuy, CalculateSell }       // Pure، uses FillMath
  BuySizeInput: AllocatedCapital, CommittedCapital,
                AvailableCash (decimal?، null=skip), SignalStrength,
                StrengthMultiplier, MinDeltaPercent, Price, Commission,
                MinOrderQuantity, MaxOrderQuantity, MinOrderValueRial,
                MaxPositionValue (decimal?، PR 16)
ICapitalAllocator             → GetAllocatableCapitalAsync(ctx, accountId)
ICommittedCapitalCalculator   → CalculateForSymbolAsync
IOrderIntentBuilder           → BuildAsync(signal, context, accountId)
IBrokerOrderDispatcher        → DispatchAsync(intent, brokerType, session)
IOrderDispatcherResolver      → Resolve(isPaper) → Live / Paper
IExecutionCoordinator         → HandleAsync(envelope), CloseRunPositionsAsync
```

### Risk (Phase D — جدید)
```
IRiskGuardPipeline            → CheckAsync(RiskCheckContext) → Allow / Block
IRiskGuard (interface)        → CheckAsync; Guards: KillSwitch, DailyLoss,
                                CircuitBreaker, RateLimiter (فعال‌شده‌ها)
IRiskStateTracker             → Kill Switch (Global + Per-Account)
  ActivateKillSwitchAsync, DeactivateKillSwitchAsync, RefreshAsync,
  IsGlobalKillSwitchActive, IsAccountKillSwitchActive, ActiveAccountKillSwitches

IAccountCashCache             → Invalidate-Driven، per-Account BuyPower
  TryGet, RefreshAsync, GetOrRefreshAsync, Invalidate
  Snapshot: BuyPowerT0, RefreshedAt, Source

IRunDailyEquityTracker        → Per (Run, Account) Daily Opening + Current
  GetOrCaptureOpeningAsync, GetCurrentAsync
  RunAccountDailyEquityOpening/Current: OpeningCash، PositionValue، LossPercent

IRunAccountLossStreakTracker  → Consecutive Loss per (Run, Account)
  GetAsync، OnPositionClosedAsync، ResetAsync، ResetRunAsync، ResetAccountAsync،
  GetActiveCircuitsAsync
  LossStreakState: ConsecutiveLosses، CircuitBrokenAt، CooldownExpiresAt، IsCircuitActive

IRateLimiterTracker           → Sliding window per-Account
  GetCountInWindow، RecordDispatch، Reset (in-memory، Singleton)

RiskCheckContext { Intent, RunContext, Account }
RiskDecision { Kind (Allow/Block), Reason, GuardName }
```

### Runtime
```
IMarketDataIngestionStage, IStrategyEvaluationStage,
IExecutionStage, IOrderLifecycleStage
IStrategyEvaluationService    → EvaluateAsync (Application، M6 split)
IRunExecutionContextCache     → Singleton، in-memory RunRuntimeContext
ISignalChannel                → Singleton، bounded Channel<SignalEnvelope>
ITradingRunManager            → StartAsync (Idempotent)، StopAsync، IsRunning
IRuntimeRecoveryService       → RecoverAsync (Restore + Re-push + Reconcile)
```

### Persistence / Logging
```
ITradingEventLogger           → Log + FlushAsync (buffer per tick)
ITradingEventQueryService     → Query، GetById
IBacktestEventLogger          → Log + FlushAsync
IReconciliationService        → ReconcileAllAsync، ReconcileAccountAsync
```

### UnitOfWork
```
ITraderUnitOfWork<TDbContext> : IUnitOfWork<TDbContext>
  // Repositories (Direct): Accounts، Instruments، InstrumentOrderPresets،
  //   TradingRuns، Signals، OrderIntents، BrokerOrders، OrderTrades، Positions،
  //   MarketSnapshots، CandleRecords، StrategyEvaluations، TradingEvents،
  //   TradingSchedules، CurrencyRates، BacktestRuns/Symbols/Trades/DailyEquities/Events،
  //   HistoricalDataCollectionJobs، SchedulePlans، ScheduledOrders، ExecutionLogs،
  //   RiskKillSwitches، RunAccountDailyEquitySnapshots، RunAccountLossStreaks
  // SpecRepositories: same entities (کل 32+)
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
  },
  "Risk": {
    "Enabled": true,
    "DailyLossLimit": {
      "Enabled": true,
      "MaxLossPercent": 5.0,
      "Scope": "Run",
      "BrokerRefreshInterval": "00:15:00",
      "ApplyToPaper": false
    },
    "MaxPositionSize": {
      "Enabled": true,
      "MaxPercentPerSymbol": 30.0,
      "ApplyToPaper": true
    },
    "CircuitBreaker": {
      "Enabled": true,
      "ConsecutiveLosses": 5,
      "CooldownMinutes": 60,
      "ApplyToPaper": false
    },
    "RateLimiter": {
      "Enabled": true,
      "OrdersPerMinutePerAccount": 10,
      "WindowSeconds": 60,
      "ApplyToPaper": false
    }
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
├── api/                    (account، instrument، tradingRun، currencyRate، ...)
├── models/                 (TradingRun، Risk (pending)، enums، labels)
├── hooks/                  (management hooks per entity)
├── pages/
│   ├── Accounts/           ✅ (Login-all button)
│   ├── Instruments/        ✅ (TradingSchedule، Commission، Min/Max)
│   ├── Strategies/         ✅
│   ├── TradingRuns/        ✅ (per-account capital via TradingRunAccountEditor)
│   ├── CurrencyRates/      ✅
│   ├── Positions/          ✅ (PAPER badge)
│   ├── Signals/            ✅
│   ├── Events/             ✅ (TradingEvents log viewer + drawer)
│   ├── Backtests/          ✅ (Create/List/Detail/Compare + LogsTab)
│   ├── HistoricalData/     ✅ (Collection Jobs)
│   ├── Risk/               ⏳ Next (Kill Switch + Circuit Breaker + Active)
│   ├── SchedulePlans/      ✅ (legacy)
│   ├── ServerClock/        ✅ (legacy)
│   └── Logs/               ✅ (ExecutionLog legacy)
├── routes.tsx
└── index.ts
```

### Core Components
- `GenericCrudPage` — با `tableFeatures`: `horizontalScroll`، `stickyAction`، `classNames`
- `GenericAddModal`، `useGenericCrud`، `SearchableMultiSelect`، `TagInput`

---

## 8. PHASE STATUS

### Phase A (Foundation Hardening) — ✅ COMPLETE
| # | Feature | Status |
|---|---|---|
| **A1** | Broker Reconciliation (Log-only) | ✅ |
| **A2** | Sub-Candle Strategy Support | ✅ |
| **A3** | Graceful Shutdown (Polly DbRetry) | ✅ |
| **A4** | Health Checks (5 Components) | ✅ |
| **A5** | Retention (Scheduler Recurring) | ✅ |
| **A6** | Unit Tests | ✅ (163 tests) |

### Phase D (Risk & Safety) — ✅ COMPLETE
| # | Feature | Status |
|---|---|---|
| **D1** | Kill Switch (Global + Per-Account) | ✅ PR 14 |
| **D2** | Daily Loss Limit (per-account) | ✅ PR 15 |
| **D3** | Max Position Size (Run-based) | ✅ PR 16 |
| **D4** | Circuit Breaker | ✅ PR 17 |
| **D5** | Rate Limiter | ✅ PR 18 |
| **D6** | Per-Account Capital | ✅ PR 19 |
| **D7** | Kill Switch Events | ✅ PR 20 |
| **D8** | Risk Dashboard UI | ✅ PR 21 |
| **D9** | Risk Seed (Resource/Permission/Menu) | ✅ |

> **نکته:** Risk Guards (D31-D33) در Live و Paper هردو فعال هستند (ApplyToPaper flag)، پس Paper Week از همان محافظت‌ها بهره‌مند می‌شود — تنها استثنا `DailyLossLimit.ApplyToPaper=false` و `CircuitBreaker.ApplyToPaper=false` که در Paper صرف‌نظر می‌شوند (چون BuyPower واقعی Paper tracked نیست).
### Phase B1 — ✅ COMPLETE
### Runtime Correctness — ✅ COMPLETE (D59)

**B1. PositionCloser Paper/Live Segregation**
- قبلاً `IBrokerOrderDispatcher` (default=Live) inject می‌کرد → Stop Paper Run با ClosePositions، سفارش **واقعی** به Broker می‌فرستاد
- Fix: `IOrderDispatcherResolver` + `OrderIntent.Create(isPaper: pos.IsPaper)` + skip session for Paper

**B2. OrderLifecycleStage Throttle Singleton**
- `_lastRunAt` instance field روی AddScoped → reset هر tick → throttle بی‌اثر
- Fix: `IRuntimeThrottleState` Singleton با Interlocked.CompareExchange

**B3. ExecutionCoordinator SaveChanges Failure**
- قبلاً swallow می‌شد → Signal unconsumed در DB → StartupRecovery re-push → duplicate dispatch
- Fix: rethrow بعد از log

### Backtest Correctness — ✅ COMPLETE (D55-D58)

**BC1-BC8 (از Backtest deep review):**
- BC1: Partial Sell — Position state و BacktestTrade slice درست (قبلاً کل position گم می‌شد)
- BC2: NetPnLPct با proportional commission
- BC3: Cancel در حین Running → MarkCancelled درست (قبلاً MarkFailed overwrite می‌کرد)
- BC4: SL/TP on Add — revert به Live behavior (D56)
- BC5: Slippage after clamp
- BC6: Daily equity با `TehranDate.FromUtc` (قبلاً UTC `.Date` → off-by-one)
- BC7: `BacktestRun.Create` از `EnsureUtc` (قبلاً `SpecifyKind` روی Local corrupted instant)
- BC8: `ConfigOverride` با `??` (قبلاً null override → nullify source)

**Additional (کشف در حین تست):**
- **781% bug** — duplicate `Quantity += qty` در ExecuteFill buy-add path → phantom proceeds (D58). Fix: ۳ خط + integrity check
- **TargetCurrency leak** — Backtest همه‌ی محاسبات را در USD انجام می‌داد (D57). Fix: Two-Stream Architecture
- **Recurring 0-insert** — TGJU API oldest-first (D60). Fix: `fetchAllPages` bool + last-page fetch
- **2024 currency missing** — `GetList` clamp 500 رکورد (D61). Fix: `CheckRange` endpoint
**Scope تحویل‌شده:**

| PR | Feature | Status |
|---|---|---|
| Pre-B1 | `CurrencyCode` extended: GOLD, SILVER, COPPER, PLATINUM, OIL (D37) | ✅ |
| B1.1 | `PriceSource` + `PriceCollectionJob` Domain + Migration | ✅ |
| B1.2 | `TgjuApiClient` (ExternalApiClient layer) + `TgjuPriceSourceProvider` | ✅ |
| B1.3 | `PriceCollectionService` + `CollectPriceJobHandler` + Scheduler integration | ✅ |
| B1.4 | `PriceSourceController` + `PriceCollectionJobController` + Seed (tgju-usd) | ✅ |
| B1.5 | Unit tests (Mock-first pattern): Domain (13) + Infra (28) | ✅ |
| B1 FE | Frontend `PriceCollectionPage` (Tabs: Jobs / Sources) + CreateJobModal | ✅ |

**Bugs found & fixed during B1:**
1. **`minRate=100,000` در seed config** → همه‌ی USD/IRR قبل از ۲۰۱۸ (< ۱۰۰K ریال) skip → ۱۶۷۳ رکورد گم‌شده. Fix: حذف `minRate` از seed (rate > 0 کافی است)
2. **TGJU API sort order (oldest-first)** → Recurring (`maxPages=1`) صفحه‌ی اول = قدیمی‌ترین‌ها → filter = 0. Fix: `fetchAllPages: bool` + Recurring = probe `recordsTotal` + fetch last page
3. **Backfill `HitBoundary` guard** → `MarkCompleted` هرگز trigger نمی‌شد (HitBoundary فقط Recurring). Fix: Backfill = `MarkCompleted` بی‌قید HitBoundary
4. **`GetMaxEffectiveAtAsync` بدون Source** → اگر Source دیگر همان Pair را تا تاریخ جلوتر پر کرده بود، Backfill این Source از دست می‌رفت. Fix: `LatestCurrencyRateBySourceSpec`

**Refactor جانبی (H1-H3):** Historical Data Collection هم‌ساختار با PriceCollection شد (Backfill/Recurring، Scheduler-only، حذف Worker). Bugs found & fixed:
1. **`FetchAsync` maxPages → fetchAllPages** (int → bool) — interface consistency
2. **`HistoricalDataCollector` بدون `UpdateAsync` قبل از `SaveChangesAsync`** → job State (MarkCompleted) و progress در DB persist نمی‌شد. Fix: ۶ call site
3. **`batch.Count < BatchSize` premature break** در `CollectComboAsync` → داده‌ی قدیمی‌تر گم می‌شد. Fix: `previousOldest` no-progress guard

**تصمیمات جدید:** D49 (Historical Refactor), D50 (Collection Job boilerplate — Rule of Three، defer), D51 (Scheduler = پلتفرم، نه Trader).
### Phase C-lite.1 — ✅ COMPLETE (Order Management UI)

| # | Feature | Status |
|---|---|---|
| **C-lite.1a** | BrokerOrder/OrderIntent `Query` + `GetById` endpoints | ✅ |
| **C-lite.1a** | `BrokerOrderQueryService`, `OrderIntentQueryService` (EF-side filters, batched account/symbol map) | ✅ |
| **C-lite.1a** | Seed: `trader.order` resource + permission + menu `/trader/orders` | ✅ |
| **C-lite.1b** | `models/Orders.ts` (enums, views, colors, options) | ✅ |
| **C-lite.1b** | `OrdersManagementPage` (Tabs: BrokerOrders / OrderIntents) | ✅ |
| **C-lite.1b** | `BrokerOrderDrawer` (Summary + Trades list), `OrderIntentDrawer` (incl. RiskBlocked reason) | ✅ |
| **C-lite.1b** | Filters: Isin, State, Paper/Live, Search; Load-More pagination | ✅ |

**Cancel deferred to C.6:** `IBrokerTradingCommands` فاقد Cancel، EasyTrader فاقد endpoint. برای Paper Week (Fill فوری) لازم نیست.

### Phase C-lite.2 + Preset Strategy — ✅ COMPLETE

| # | Feature | Status |
|---|---|---|
| **C-lite.2** | Backend `GetList` + enriched `OrderPresetView` (Isin, SymbolName, CreatedAt) | ✅ |
| **C-lite.2** | OrderPreset CRUD UI (`/trader/order-presets`) | ✅ |
| **Preset Strategy** | `OrderPriceStrategy` (Fixed/HighAllowed/LowAllowed/LastTraded/Closing/FirstTraded) | ✅ |
| **Preset Strategy** | `OrderQuantityStrategy` (Fixed/Max/Min) | ✅ |
| **Preset Strategy** | `ResolveAsync` با live API + fallback | ✅ |
| **Preset Strategy** | `ResolvePreviewDrawer` (نتیجه + raw API + range context + warning) | ✅ |
| **Range Split (D53)** | `DailyPriceRangeUpPercent` + `DailyPriceRangeDownPercent` (nullable = unlimited) | ✅ |
| **Range Split** | `Instrument` form + columns + Resolve sanity | ✅ |

**Bugs found & fixed in C-lite:**
1. `CreateOrderPresetCommand(Model)` wrapper → 400 Bad Request (D54 pattern)
2. `HasDefaultValue(0)` روی enum property → EF model creation failure → ۱۰ تست شکست
3. `OrderPresetModal` داشت ISIN (از `GetSelectionList().value`) به‌جای Guid می‌فرستاد → Preset با instrument نامعتبر
### Phase B2/B3, Phase C, Phase E — آینده
(B2: Telegram + more assets + Frontend; C: Operations; E: Advanced)

---

## 9. KEY DECISIONS RECAP

### از Master Prompt
1. Strategy از Account جداست
2. Market Data از Execution جداست
3. TradingRun مسئول Execution Context است
4. Broker پشت Abstraction با Capability Segregation
5. Future-ready، Simple Phase 1

### تصمیمات Phase A/B/C/D (منتقل به `docs/TRADING-DECISIONS.md`)
- **D3 (اصلاح‌شده):** Capital per (Run, Account)، نه per Run
- **D5:** Multi-Account = Fan-Out (بدون Coordination)
- **D6:** ویرایش Run در حالت Running ممنوع
- **D8 (C2):** SL/TP Monitoring مستقل از Order Activity
- **D9 (C4):** Position Overshoot → Run Faulted (نه Clamp)
- **D10 (M5):** `FillMath` تنها منبع محاسبات Fill
- **D13:** Currency Transform Per-Candle
- **D18/D19:** Broker Capability Segregation + Keyed DI
- **D22:** Backtest Config Snapshot (نه Mirror)
- **D23:** Paper Trading = Dispatcher (نه Mode جدا)
- **D24:** Reconciliation Log-only
- **D29:** Capital per-account، Invariants
- **D30:** Cash مشترک بین Runها روی یک Account
- **D31:** Risk Guard Pipeline Fail-Safe = Block
- **D32:** Kill Switch ۳ سطح (Global، Account، Run=Stop موجود)
- **D33:** Cash Cache Invalidate-Driven

### تصمیمات فنی مهم
- **Stepped Buying:** `targetCapital = allocated × strength × multiplier`، delta check، `MinDeltaPercent` (default 3%)
- **Exact Commission Formula (M7):** `1 / (1 + c)` نه `(1 - c)` — Qty حداکثر ۱ سهم کمتر در edge cases
- **OrderSizing Pure Logic:** Live/Paper/Backtest از یک `IOrderSizeCalculator`
- **Paper Trading:** Immediate Fill در `intent.Price`، بدون Slippage
- **TargetCurrency Whitelist (M10):** `CurrencyCode.NormalizeOrNull` در Create
- **Historical Data Collection:** On-Demand Command + Background Worker + Rate Limit
- **Event Logging:** `TradingEvents` و `BacktestEvents` جدا + Buffer + Flush per Tick
- **Retention:** Scheduler Recurring Job روزانه

---

## 10. KNOWN ISSUES / BACKLOG

### Known Issues / Backlog

#### Phase B1 (Code Pending)
| # | Item | Priority |
|---|---|---|
| 1 | TGJU Import — code (D34-D42 approved) | **In Progress** |

#### Phase B2 (Future)
| # | Item | Priority |
|---|---|---|
| 1 | Telegram Channel PriceSourceProvider | High |
| 2 | Multi-asset (EUR/AED/GOLD/SILVER/COPPER) | High |
| 3 | Frontend UI (Price Sources + Jobs) | Medium |
| 4 | Data Completeness Monitoring | Medium |
| 5 | `ExecutionLogService.Skip/Take` → SQL | Medium |

#### Phase C (Operations)
| # | Item | Priority |
|---|---|---|
| 1 | Order Management UI | High |
| 2 | Order Preset UI | High |
| 3 | Daily PnL Widget، Dashboard | Medium |
| 4 | Source Filter در Currency Transform (D40 ext) | Medium |
| 5 | `AssetPrice` rename if needed (D36 ext) | Low |
| 6 | Multi-Account Coordination Policy | Low |

#### Phase E (Advanced)
| # | Item | Priority |
|---|---|---|
| 1 | Strategy Ensemble (D27) | High |
| 2 | Trailing Stop / Laddered Exit | Medium |
| 3 | AI/ML Pipeline (D28) | Medium |
| 4 | Multi-Instance Prep (Redis, D20) | Low |

#### Technical Debt (Low Priority)
| # | Item |
|---|---|
| 1 | `UseQuerySplittingBehavior` commented |
| 2 | `ITraderUnitOfWork` = Registry نه UoW واقعی |
| 3 | Bonus/StockRight در Reconciliation نادیده |
| 4 | `TraderSymbol` Legacy (Refactor به Instrument) |
| 5 | `SchedulePlan`/`ScheduledOrder` Legacy |

---

## 11. FILES TO SEND IN NEW CHAT

### ضروری (با اولویت)
1. **این فایل (`HANDOFF.md`)**
2. **`docs/TRADING-DECISIONS.md`** — Decision Log (D1-D33)
3. **`docs/Backend-Architecture.md`** — Narrative

### اگر Modification
- فایل‌های Domain مربوطه
- Interfaceهای Application که مرتبط هستند
- Impl فعلی که می‌خواهی تغییر بدهی
- **Tests مربوطه** (از `Trader.Tests/`) — تا Reviewer بداند چه رفتار قفل شده

### اگر Migration
- `TraderDbContext.cs` + `Configurations` مربوطه

### اگر Frontend
- `models/*.ts` + `api/*.ts` + `pages/<feature>/` + `routes.tsx`

### اگر مطمئن نیستی
- فقط `HANDOFF.md` + `TRADING-DECISIONS.md` بفرست + اسم Feature
- Reviewer از لیست ۳۲ Interface در Section 5 می‌پرسد

---

## 12. COLLABORATION MODE

### کاربر انتظار دارد
1. **قبل از کد، طرح + تأیید صریح** — اگر تصمیم معماری است
2. **کد کامل** — نه pseudo-code
3. **فایل‌به‌فایل** — namespace + path + DI + Migration
4. **پاسخ مستقیم و فنی** — بدون مقدمه‌ی طولانی
5. **Commit Message در انتهای هر پیام**
6. **این HANDOFF را در انتهای هر Phase به‌روز کن**
7. **Backward Compat** — بدون Refactor بدون دلیل

### کاربر تجربه دارد (۱۲ سال) — Junior نباش
- روی تصمیمات معماری مستقیم بحث کن
- اگر پیشنهاد کاربر بهینه نیست، صریح بگو چرا
- اگر Overengineering است، بگو
- اگر راه ساده‌تر هست، بگو

---

## 13. NEXT STEPS

### بلافاصله — C-lite.3 Dashboard — ~۱ روز

**Scope:**
- Backend: `GET /api/Trader/Dashboard/Summary` (aggregate)
- Frontend: `/trader` (root) → `DashboardPage`
- Cards:
  - **Kill Switch** (Global + Active Accounts)
  - **Runs** (Running/Ready/Stopped counts)
  - **Orders Today** (Paper/Live split)
  - **Positions Open** (count + unrealized PnL)
  - **Recent Critical Events** (last 10، Severity>=Warning)
  - **Accounts Session** (valid/expired/empty counts)
- Auto-refresh 15s

**Permission:** `trader.dashboard` (new resource) — یا استفاده از `trader.tradingrun` (View).

### Paper Trading Week (~۱ هفته)

- Run ساده ۲-۳ نماد با بودجه‌ی محدود
- Recurring TGJU Job فعال
- Kill Switch Global در دسترس
- هر روز صبح: Dashboard + Events
- هر Bug → یادداشت، بعد fix

**معیار موفقیت:** No `PositionOvershoot`، No `KillSwitchActivated` غیرمنتظره، No `CircuitBreakerTriggered` غیرمنتظره، Reconciliation پاک، PnL منطقی.

**Reality check:** تست‌های Backtest اخیر نشان دادند RSI ساده با `MinSignalStrength=0.1` روی M1 اهرم بازنده است (−۳۶٪). برای Paper Week: `MinSignalStrength>=0.6` یا timeframe بالاتر (M5/M15) با سهم غیر اهرم.

### Phase B2 (~۲-۳ روز)

- B2.1 Telegram Channel PriceSourceProvider
- B2.2 Multi-asset (EUR, AED, GOLD, SILVER, COPPER)
- B2.3 Data Completeness Monitoring
- B2.4 Frontend refinement (Backfill progress bar)

### Phase C کامل (~۱ هفته)

- C.4 Source Filter in Currency Transform (D40 ext)
- C.5 SchedulePlan integration با Preset Strategy
- C.6 Order Modify + Cancel (نیازمند EasyTrader endpoint)
- C.7 Partial Fill Handling UI

### Phase E (~۲ هفته)

- E.1 Strategy Ensemble (D27)
- E.2 Trailing Stop / Laddered Exit
- E.3 AI/ML Pipeline (D28)
- E.4 Multi-Instance Prep (Redis، D20)

### Technical Debt

- TD1 `UseQuerySplittingBehavior` commented
- TD2 `ITraderUnitOfWork` = Registry نه UoW
- TD3 `TraderSymbol` Legacy
- TD4 `SchedulePlan`/`ScheduledOrder` Legacy
- TD5 `ExecutionLogService.Skip/Take` memory
- TD6 `CandleRecord.TimeFrame` enum → int cast
- TD7 `SchedulePlan`/`PlanExecutor` از HighAllowed/LowAllowed hardcoded، نه Preset
- TD8 `OrderPresetView` فاقد Range fields (فقط ResolvedView)
- **TD9 (new)** `CurrencyCandleTransformer` cache key شامل first+last+count (قبلاً فقط last-minute → collision در backtest طولانی) — fix اعمال شد ولی تست dedicated ندارد

### Free / Deferred ADRs

- **D50 L1**: `GenericScheduledJobPage<TConfig>` frontend (ریسک صفر)
- **D50 L2/L3**: Backend abstraction مؤکول به سومین Collection Job

## 14. RECENT COMPLETED WORK

1. **Backtest Correctness (D55-D58)** — ۸+ باگ Critical/High
   - BC1 Partial Sell، BC2 Proportional PnL، BC3 Cancel، BC4 SL/TP on Add (revert)، BC5 Slippage clamp، BC6 Tehran date، BC7 UTC conversion، BC8 ConfigOverride `??`
   - **781% bug** — duplicate quantity mutation → phantom proceeds → integrity check added
   - **TargetCurrency leak** — Two-Stream Architecture (strategy stream vs execution stream)
   - **Recurring 0-insert** — TGJU oldest-first API → last-page fetch
   - **2024 currency check** — `CheckRange` endpoint (server-side)
   - تست‌های واقعی: بعد از fix، RSI M1 اهرم با MinSignalStrength=0.1 → −۳۶٪ (ProfitFactor 0.45، WinRate 49.76%، Sharpe −4.57). با tune config → −۱.۵٪.

2. **Runtime Fixes (D59)** — B1+B2+B3
   - PositionCloser: Paper/Live segregation (خطر مال واقعی در Paper Week رفع شد)
   - OrderLifecycleStage: Throttle Singleton (IRuntimeThrottleState)
   - ExecutionCoordinator: No-swallow Persist

3. **Phase C-lite.2 + Preset Strategy** ✅
   - `OrderPriceStrategy` (Fixed/HighAllowed/LowAllowed/LastTraded/Closing/FirstTraded)
   - `OrderQuantityStrategy` (Fixed/Max/Min)
   - `ResolveAsync` با live API + fallback (Closing × Range Up/Down)
   - `ResolvePreviewDrawer` frontend
   - Range Split (D53): Up/Down nullable، unlimited

4. **Phase C-lite.1 Order Management** ✅
   - `BrokerOrderController` + `OrderIntentController` (Query + GetById)
   - `/trader/orders` UI (Tabs + drawers + filters + Load More)
   - Cancel deferred to C.6

5. **Phase B1 TGJU Import** ✅ — ۴ باگ fixed (minRate seed، sort order، Backfill completion، per-source boundary) + Historical Data Refactor (D49)

6. **ADR Documents** — D52-D61 (10 new)

**Next:** C-lite.3 Dashboard → Paper Trading Week