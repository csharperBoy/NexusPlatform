# Trader Module — Architectural Decisions Log

> این سند **تصمیمات** را مستند می‌کند، نه پیاده‌سازی را.
> هر ورودی توضیح می‌دهد: «چه تصمیمی، چرا، و چه محدودیتی داریم».
> برای جزئیات پیاده‌سازی، به کد مراجعه کنید.
>
> **آخرین به‌روزرسانی:** [تاریخ]

---

## مرزهای Domain

### D1 — `Strategy` کاملاً از `Account` و `Broker` مستقل است

**تصمیم:** `IStrategy.Evaluate(StrategyInput) → StrategyEvaluationResult` تنها ورودی/خروجی Strategy است. هیچ `AccountId`، هیچ `BrokerOrder`، هیچ `Capital` در `StrategyInput` نیست.

**دلیل:** Strategy یک Function است: `Market Data → Signal`. هر وابستگی به Account، Strategy را به Execution گره می‌زند و Reusability بین Runها/Accountها از بین می‌رود.

**محدودیت:** اگر یک Strategy نیاز به داده‌ی خاصِ Account داشته باشد (مثلاً "Risk Limit کاربر")، نمی‌تواند داخل Strategy enforce شود. باید در Execution Layer انجام شود (Phase B — Risk Module).

### D2 — `MarketData` از `Trading Account` مستقل است

**تصمیم:** `IMarketDataProvider` هیچ آگاهی از `TraderAccount` ندارد. `BrokerMarketDataProvider` یک Account انتخاب می‌کند (اولین EasyTrader) صرفاً برای گرفتن Session — نه به‌عنوان هویت.

**دلیل:** اگر Market Data از Account جدا نباشد، در Run با چند Account، این سؤال پیش می‌آید که "از کدام Account قیمت بگیریم؟" — که سؤال غلطی است. قیمت، قیمت است.

**محدودیت:** در Phase 1، اگر هیچ Account با Session معتبر نباشد، Market Data دریافت نمی‌شود (Broker API نیاز به Auth دارد). در Phase 2 با Provider مستقیم TSETMC این محدودیت رفع می‌شود.

### D3 — TradingRun مالک Execution Context است (اصلاح‌شده در PR 19)

**تصمیم:** `ExecutionSettings` (StopLoss، TakeProfit، DefaultOrderMode، OnMarketClose، OnStop) روی `TradingRun` هستند. **Capital Allocation اما per (Run, Account)** است — هر `TradingRunAccount` خودش Mode (Fixed/Dynamic) و مقدار دارد.

**دلیل اصلاح:** باگ قبلی در `FixedCapitalAllocator` (قبل از PR 19) — Run با `FixedAmount=100M` و ۲ Account، برای هر دو Account همان 100M را برمی‌گرداند. Run عملاً 200M پخش می‌کرد، نه 100M. همچنین با اصل «پول هر Account برای خودش»، Capital باید per-account باشد.

**محدودیت:** اگر یک Account در چند Run باشد و دو Run هر دو Fixed مبلغ دارند، هر کدام مبلغ خودش را مستقل می‌خواهد — اما Cash مشترک است. اگر Run R1 اول مصرف کند، R2 مبلغ کمتری می‌بیند (D30).

### D4 — Instrument، Options را با Owned Type مدل می‌کند

**تصمیم:** `Instrument.OptionContract` (Type, Strike, Expiration, UnderlyingIsin) به‌صورت `OwnedType` روی `Instruments` table است — نه جدول جدا.

**دلیل:** Options در Phase 1 محدودیت کمی دارند (Call/Put، Strike، Expiration). OwnedType خواندن را ساده می‌کند. در آینده با اضافه شدن Future، Bond، Fund، این جدول پر از ستون Nullable می‌شود.

**Phase 2 trigger:** اگر تعداد Instrument Typeها به ۶+ رسید و فیلدهای مخصوص هر Type بیش از ۴ شد، به Table-per-Type یا TPH با Discriminator مهاجرت می‌کنیم.

---

## مرزهای Execution

### D5 — Multi-Account Signal Routing = Fan-Out (بدون Coordination)

**تصمیم:** هر Signal روی **همه‌ی Accountهای همان Run** اجرا می‌شود. `ExecutionCoordinator.HandleAsync` یک حلقه روی `ctx.AccountIds` دارد.

**دلیل:** Phase 1 استفاده شخصی است. کاربر می‌خواهد یک Strategy را روی چند Account همزمان اجرا کند (Mofid + Agah).

**محدودیت:**
- هیچ Coordination بین Accountها نیست. اگر Account A کافی نباشد و Account B کافی باشد، فقط B Order می‌دهد.
- `Signal.MarkConsumed` حتی اگر همه Accountها Skip کنند (به‌جز وقتی `successCount > 0` — در آن حالت هم Skip می‌کند). دلیل: جلوگیری از Retry یک Signal که در Tick قبل تصمیم به Skip گرفته.
- **Phase 2 trigger:** اگر نیاز به Policy (Round-Robin، First-Available، Proportional) شد، `ExecutionSettings` یک فیلد `AccountRoutingPolicy` بگیرد و `ExecutionCoordinator` بر اساس آن تصمیم بگیرد.

### D6 — ویرایش `TradingRun` در حالت Running ممنوع است

**تصمیم:** `TradingRun.UpdateSettings` و `AddSymbol`/`RemoveSymbol`/`AddAccount`/`RemoveAccount` همه `EnsureNotRunning()` را صدا می‌زنند و Throw می‌کنند.

**دلیل:** ویرایش در حال اجرا، Orderهای In-Flight را با Config قدیمی و Orderهای بعدی را با Config جدید مخلوط می‌کند. Signalهای قبل و بعد از ویرایش در یک `TradingRunId` قاطی می‌شوند — Backtest آینده گمراه‌کننده می‌شود.

**راه صحیح:** Stop → Edit → Start.

### D7 — Stop Loss / Take Profit در سطح `TradingRun` هستند

**تصمیم:** `TradingRun.StopLoss` و `TradingRun.TakeProfit` — نه روی `TraderAccount`.

**دلیل:** هم‌راستا با D3. یک Account در چند Run، SL/TP مختلف دارند.

**محدودیت:** SL/TP روی `Position` ذخیره می‌شوند (نه در `TradingRun` لحظه‌ی Close). اگر Config `TradingRun` بعد از Fill عوض شود، Positionهای باز با Config قدیمی می‌مانند. این تصادفی است: SL/TP در لحظه‌ی Fill خوانده می‌شوند، نه در لحظه‌ی Close.

### D8 — SL/TP Monitoring مستقل از Order Activity است

**تصمیم (C2):** `OrderLifecycleStage.SyncAccountAsync` بعد از Sync سفارش‌ها، **همیشه** Positionهای باز را برای SL/TP بررسی می‌کند — حتی اگر هیچ سفارش بازی وجود نداشته باشد.

**دلیل:** باگ قبلی: اگر Run یک Buy Order ارسال می‌کرد و آن فوراً Fill می‌شد، `openOrders.Count` در tick بعدی صفر می‌شد و `SyncAccountAsync` early-return می‌کرد — SL/TP هرگز اجرا نمی‌شد.

### D9 — Position Overshoot → Run Faulted (نه Clamp)

**تصمیم (C4):** اگر Broker یک Sell بزرگ‌تر از Position ثبت‌شده گزارش دهد:
1. Fill اعمال **نمی‌شود** (Position در "known good" state می‌ماند)
2. Trade record persist می‌شود (audit trail)
3. `TradingEvent` با `PositionOvershootDetected` و Severity=Critical ثبت می‌شود
4. `TradingRun` → `Faulted`
5. Run از `IRunExecutionContextCache` خارج می‌شود

**دلیل:** Overshoot یک نشانه‌ی Data Corruption است — نه یک Warning قابل تحمل. علت‌های ممکن: Manual Trade در Broker، Cross-Account Bug، Data Race، Position Phantom. ادامه دادن با State خراب، اختلاف را بزرگ‌تر می‌کند.

**چرا Faulted و نه Stop؟** Stop یک اقدام کاربر است (`RunStopped` event). Faulted یک سیگنال Data Integrity است که نیاز به بررسی دستی دارد.

### D10 — `FillMath` تنها منبع مشترک محاسبات Fill است

**تصمیم (M5):** `FillMath` (Static Class، در `Trader.Domain/Calculations/`) تمام محاسبات Pure مربوط به Fill را در خود دارد:
- `CalculateWeightedAveragePrice`
- `CalculateRealizedPnL`
- `CalculateCommission`
- `CalculateMaxBuyQuantity`

**دلیل:** قبل از M5، این فرمول‌ها در `Position.ApplyBuyFill/ApplySellFill` و `BacktestEngine.ExecuteFill` **دو نسخه** داشتند. هر Bug در یکی، به Divergence بین Backtest و Live منجر می‌شد.

**چرا Static نه Interface:** Pure arithmetic، بدون I/O، بدون state. Mock کردنش تست را بی‌معنی می‌کند. JIT inline می‌کند.

**محدودیت:** `OrderSizeCalculator.CalculateBuy` از `(1 - c)` استفاده می‌کرد (approximation). در PR 6 به `1 / (1 + c)` تغییر کرد — این **رفتار را کمی تغییر داد** (qty حداکثر ۱ سهم کمتر در edge cases). تست‌های `CalculateBuy_WithNonZeroCommission_UsesExactFormula` این را قفل می‌کنند.

---

## مرزهای Market Data

### D11 — `Market Data` از `IMarketDataProviderResolver` بر اساس Market Type حل می‌شود

**تصمیم:** `MarketType` (IranStock، IranOption، Forex، Crypto) کلید Resolve است. در Phase 1، `BrokerMarketDataProvider` تنها Provider است و IranStock + IranOption را پشتیبانی می‌کند.

**دلیل:** آماده برای افزودن Providers بیشتر (TSETMC، Coinbase، ...) بدون تغییر در `IMarketDataIngestionStage`.

### D12 — `Candle Cache` TTL با Awareness از `MarketSession` است

**تصمیم:** `InMemoryCandleCache.ComputeExpiryAsync` اگر بازار بسته است، TTL را تا باز شدن بعدی تنظیم می‌کند. اگر بازار باز است، تا Candle Close بعدی + buffer.

**دلیل:** بدون Market-Aware TTL، یا Cache در ساعات بسته بی‌جهت Refetch می‌کند (هزینه‌ی API)، یا در ساعات باز Candleهای کهنه می‌دهد.

### D13 — `Currency Transform` Per-Candle است، نه Global

**تصمیم:** هر Candle با نرخ ارز مربوط به **OpenTime همان Candle** تبدیل می‌شود (نه با نرخ آخرین Candle).

**دلیل:** اگر نرخ ارز در طول دوره‌ی Backtest تغییر کند، استفاده از نرخ یکسان، Trade‌های گذشته را با نرخ امروز می‌سنجد — که غلط است.

**محدودیت:** برای Candleهایی که **قبل از** اولین نرخ ثبت‌شده در DB هستند، از `baselineRate` (اولین نرخ) استفاده می‌شود. `CurrencyCandleTransformer` یک `CandleCountBeforeFirstRate` در خروجی برمی‌گرداند تا کاربر بداند چند Candle با نرخ تخمینی تبدیل شده.

---

## مرزهای Runtime

### D14 — Pipeline چهار Stage مستقل با Failure Isolation

**تصمیم:** `TradingRuntimeWorker.TickAsync` چهار Stage را به ترتیب اجرا می‌کند:
1. `MarketDataIngestionStage` — Snapshot از Broker → Store
2. `StrategyEvaluationStage` — Candle + Evaluate → Signal → Channel
3. `ExecutionStage` — Channel Drain → OrderIntent → Broker
4. `OrderLifecycleStage` — Poll Order Status → Position Update → SL/TP Check

خطای یک Stage، Tick بعدی را متوقف نمی‌کند.

**دلیل:** Decoupling + Failure Isolation. اگر Broker API در Stage 1 Down باشد، Stage 2 با Cache Tick قبل اجرا می‌شود (یا Skip می‌کند). Stage 3 و 4 مستقل اجرا می‌شوند.

### D15 — `Signal Channel` یک `BoundedChannel` با `FullMode.Wait` است

**تصمیم:** `SignalChannel` با Capacity از Config (`SignalChannelCapacity`، default 1000) و `FullMode.Wait`.

**دلیل:** اگر Producer (Stage 2) سریع‌تر از Consumer (Stage 3) باشد، Wait فشار را روی Stage 2 می‌آورد — بهتر از Drop.

**محدودیت:** اگر Stage 3 در یک Tick بیش از ۲۰۰ Signal پردازش کند، `ExecutionStage.MaxSignalsPerTick = 200` محدود می‌کند و بقیه در Tick بعدی. این برای جلوگیری از یک Tick طولانی است.

### D16 — `RunExecutionContextCache` فقط با `Remove` پاک می‌شود

**تصمیم:** `IRunExecutionContextCache.Remove(runId)` صریحاً از `TradingRunManager.StopAsync` و از `PositionManager.FaultRunAsync` صدا زده می‌شود. هیچ TTL یا Expiry خودکار وجود ندارد.

**دلیل:** یک Run فعال، تا وقتی کاربر Stop نکند، باید در Cache بماند. اگر TTL داشت، بعد از Expiry، Run دوباره از DB لود می‌شد — که رفتارش متفاوت است (مثلاً Config cache miss).

**محدودیت:** اگر `TradingRunManager.StopAsync` یا `FaultRunAsync` برای یک Run از دست برود (Bug)، Cache برای همیشه آلوده می‌ماند. `TradingRunService.UpdateAsync` دیگر این Risk را ندارد (C3 — dead code حذف شد).

### D17 — `UserDataContext` در هر Tick از Cache می‌آید

**تصمیم:** `IUserContextCache.GetOrLoadAsync(userId)` TTL=5min دارد. Stage 2 و Stage 3 و Stage 4 در هر Tick این را صدا می‌زنند.

**دلیل:** `IUserDataContextProvider.BuildAsync` سنگین است (چند Query به Identity + Authorization). Cache ضروری است.

**محدودیت:** اگر کاربر Permissionهایش را تغییر دهد (مثلاً Role اضافه/حذف)، تا ۵ دقیقه طول می‌کشد تا در Runtime اعمال شود. برای Phase 1 قابل قبول.

---

## Broker Abstraction

### D18 — Capability Segregation روی `IBroker` (نه Fat Interface)

**تصمیم:** به جای یک `IBrokerClient` با ۳۰ Method، هفت Interface کوچک:
- `IBrokerSessionManager`
- `IBrokerTradingCommands`
- `IBrokerAccountReader`
- `IBrokerMarketDataReader`
- `IBrokerOrderHistoryReader`
- `IBrokerAnalysisReader`
- `IBrokerWatchlistReader`
- `IBrokerFinanceCommands`

هر کدام `IBroker` را Extend می‌کند (که فقط `BrokerType` دارد).

**دلیل:** در Broker Adapter (`EasyTrader`)، پیاده‌سازی مشترک (`EasyTraderApiClient`) را می‌شود به هر Capability تزریق کرد، بدون پیاده‌سازی Methodهای غیرمرتبط. Mock کردن در Test ساده‌تر است.

### D19 — `IBrokerClientFactory.Get<T>(brokerType)` با Keyed Services

**تصمیم:** Broker Capabilityها با `AddKeyedScoped<T>(BrokerType.EasyTrader)` ثبت می‌شوند. `BrokerClientFactory` با `_sp.GetRequiredKeyedService<T>(brokerType)` Resolve می‌کند.

**دلیل:** بدون Keyed Services، برای هر Broker باید یک Factory جدا یا یک Switch روی Type می‌داشتیم. Keyed Services این را در DI حل می‌کند.

**محدودیت:** اگر Broker جدید اضافه شود (مثلاً Agah)، فقط `AgahBrokerRegistration.AddAgahBroker()` صدا زده می‌شود — بدون تغییر در `BrokerClientFactory`.

### D20 — `BrokerSessionCache` In-Memory است (نه Redis)

**تصمیم:** `InMemoryBrokerSessionCache` (Singleton، ConcurrentDictionary) با Lock per-account.

**دلیل:** Phase 1 Single-Instance است. Redis برای Phase 2 (Multi-Instance).

**Phase 2 trigger:** اگر Deployment Multi-Instance شد، `IBrokerSessionCache` را با `RedisBrokerSessionCache` جایگزین می‌کنیم — Interface همان است. Lock per-account باید با Redis Distributed Lock پیاده شود.

---

## Backtest vs Live vs Paper

### D21 — Backtest در یک Scope مستقل از Runtime اجرا می‌شود

**تصمیم:** `BacktestWorker` و `BacktestEngine` از `IServiceScopeFactory.CreateScope()` برای هر Run استفاده می‌کنند. این Scope از Runtime Worker جدا است.

**دلیل:** Backtest یک Run طولانی (دقیقه تا ساعت) است. اجرا در همان Scope با Runtime Worker، Cacheهای Runtime را آلوده می‌کند.

**محدودیت:** `BacktestEngine` نمی‌تواند از `IRunExecutionContextCache` استفاده کند (Singleton در Scope Runtime). Config را مستقیماً از `BacktestRun` (Snapshot) می‌خواند.

### D22 — Backtest Config Snapshot می‌شود، نه Mirror

**تصمیم:** `BacktestRun` تمام Config را در خود کپی می‌کند (`StrategyConfigJson`، `CapitalAllocationMode`، `Symbols`، ...). اگر `TradingRun` بعداً تغییر کند، `BacktestRun` تغییر نمی‌کند.

**دلیل:** Re-run یک Backtest باید همان نتیجه را بدهد. اگر Config از `TradingRun` زنده خوانده می‌شد، Re-run غیرقابل اطمینان بود.

**محدودیت:** `BacktestRun` داده‌ی تکراری دارد (`SourceRunId` هم ذخیره می‌شود، ولی برای Reference است). دو Run که از یک Source ساخته شده‌اند، هیچ Share ندارند — فضای ذخیره‌سازی بیشتر به قیمت Reproducibility.

### D23 — Paper Trading یک `IBrokerOrderDispatcher` است، نه Mode جدا

**تصمیم:** `PaperBrokerOrderDispatcher` همان Interface `IBrokerOrderDispatcher` را پیاده می‌کند. `OrderDispatcherResolver.Resolve(intent.IsPaper)` تصمیم می‌گیرد.

**دلیل:** در Live و Paper، Pipeline یکسان است (Stage 3 → Dispatcher → Stage 4). فقط مرحله‌ی "ارسال به Broker" متفاوت است. اگر Paper Mode جدا بود، تمام Stageها باید Fork می‌شدند.

**محدودیت:** `PaperBrokerOrderDispatcher` Fill را **فوری** در `intent.Price` اعمال می‌کند. این در سناریوهای واقعی متفاوت است (Slippage، Partial Fill، Rejection). برای Paper Trading در Phase 1 قابل قبول است.

---

## Data Integrity

### D24 — `Reconciliation` فقط Log می‌کند (Phase A1)، Auto-Fix در Phase A1+

**تصمیم:** `ReconciliationService.ReconcileAccountAsync` Positionهای DB و Broker را مقایسه می‌کند و برای هر Mismatch یک `TradingEvent` (Severity=Warning/Error) صادر می‌کند. هیچ Modification در Position انجام نمی‌دهد.

**دلیل:** Auto-Fix ریسک دارد — اگر Bug در Reconciliation باشد، Position را خراب‌تر می‌کند. اول مشاهده، بعد تصمیم.

**محدودیت:** Bonus Share و Stock Right در Reconciliation نادیده گرفته می‌شوند (چون در `PortfolioPerformanceDto` فیلد جدا هستند و مقدار Broker بالاتر از Position DB است). این Mismatch کاذب تولید می‌کند — Phase A1+ رفع می‌شود.

### D25 — `Outbox Pattern` روی `SaveChangesAsync` (از Core)

**تصمیم:** Domain Eventها (`SignalGeneratedEvent`، `PositionChangedEvent`، ...) در `EfUnitOfWork.SaveChangesAsync` به `OutboxMessages` table منتقل می‌شوند. پردازش Async.

**دلیل:** Realtime Event Broadcasting (مثلاً Signal→Signal Channel) Sync است، ولی Persistent Events (Audit/ML) باید Atomic با Entity Write باشند.

**محدودیت:** Outbox Dispatch (پردازش Outbox) در `Core` Module است، نه `Trader`. Trader مسئول Dispatch نیست.

---

## Future-Readiness (Phase B/C/D)

### D26 — `IStrategy.ExtraCandleRequirements` امکان Sub-Candle را می‌دهد

**تصمیم:** Strategy می‌تواند `CandleRequirement[]` برگرداند — مثلاً `{ TimeFrame: M15, Count: 5 }`. `StrategyInput.ExtraTimeFrames` یک Dictionary از TimeFrame → Candleها است.

**دلیل:** RSI/MACD فقط به Primary TF نیاز دارند. اما Strategyهای آینده (OrderBook، Multi-TF) ممکن است نیاز به Candleهای Sub داشته باشند. این Extension Point بدون تغییر Interface اضافه شد (A2).

**محدودیت:** `ExtraCandleRequirements` فقط می‌تواند `TimeFrame`های کوچک‌تر از Primary باشد. اگر بزرگ‌تر بود، Skip می‌شود. برای Higher Time Frame، Strategy باید از Primary TF استفاده کند.

### D27 — `StrategyEnsemble` (Phase E) بدون تغییر Interface کار می‌کند

**تصمیم:** `IStrategy.Evaluate` یک `StrategyEvaluationResult` با `IReadOnlyList<StrategySignal>` برمی‌گرداند. یک `StrategyEnsemble` می‌تواند چند Strategy را اجرا کند و Signalها را با Aggregation به یک Signal تبدیل کند — و همچنان `IStrategy` بماند.

**دلیل:** Interface برای Ensemble آماده است. `StrategyFactory` هر `IStrategyRegistration` را Resolve می‌کند — Ensemble هم یک Registration است.

### D28 — AI/ML Data Pipeline روی `StrategyEvaluation` سوار می‌شود

**تصمیم:** هر Evaluate یک `StrategyEvaluation` با `FeaturesJson`، `PriceAtEval`، `Action`، `Strength`، `MarketSnapshotId` ذخیره می‌کند. بعد از Close Position، `OutcomeJson` پر می‌شود.

**دلیل:** Training Dataset آینده = `StrategyEvaluation` + `MarketSnapshotRecord` + Candle History. بدون Schema تغییر، ML می‌تواند Featureها را Reconstruct کند.

**محدودیت:** `FeaturesJson` یک Blob است. اگر Feature Vector از ۵۰ فیلد بیشتر شود، Query مستقیم روی JSON کند می‌شود. `Partial Index` روی `Action <> 0` برای Non-Holdها هست، ولی برای Feature-specific Queries باید Denormalize شود.

### D29 — Capital Allocation per (Run, Account)، نه per Run

**تصمیم:** `TradingRunAccount.CapitalAllocation: AccountCapitalAllocation` (Owned Type). دو حالت:
- `Fixed(Money)`: مقدار ثابت > 0
- `Dynamic(PercentOfBuyPower)`: درصد در (0, 100]
- Invariant: دقیقاً یکی از دو فیلد Set

**دلیل:**
1. اصل «پول هر Account برای خودش»
2. امکان ترکیب در یک Run: Account A Fixed 50M، Account B Dynamic 100%
3. هم‌راستا با per-account Cash Cache (D33)، DailyLoss (D8 اصلاح‌شده)، CircuitBreaker

**Trade-off:** Migration از Run-level به per-account، داده را Copy می‌کند. Runهایی که Fixed با ۲ Account داشتند، بعد از Migration هر Account جدا 100M دارد — همان رفتار واقعی قبلی، فقط شفاف.

**پیامد:** Run-level `CapitalAllocation` از `TradingRun` حذف شد. `BacktestRun` سرمایه‌ی خودش را دارد (Simulation یک Pool واحد است).

---

### D30 — Cash مشترک است بین Runها روی یک Account

**تصمیم:** `IAccountCashCache` یک `BuyPowerT0` per Account نگه می‌دارد — نه per (Run, Account).

**پیامد:**
- Run R1 (Dynamic 50% از A) + R2 (Dynamic 100% از A)
- اگر R1 اول 60M از 200M موجود A مصرف کند، R2 در Order بعدی 140M می‌بیند × 100% = 140M
- ضرر روزانه در R1 روی Cash همان Account اثر می‌گذارد، پس DailyLoss R2 هم آن را می‌بیند (ضرر واقعی است، نه اجتناب‌پذیر)

**Phase B trigger:** اگر Loss per-run جدا لازم شد (مثلاً R1 می‌خواهد بداند خودش چقدر ضرر داده)، باید `CashLedger` per (Run, Account) بسازیم. Design بزرگ‌تر — فعلاً Skip.

---

### D31 — Risk Guard Pipeline: Fail-Safe = Block

**تصمیم:** `IRiskGuardPipeline` Guards را به ترتیب اجرا می‌کند. اگر Guard خطا داد (Exception)، نتیجه **Block** است — نه Allow.

**دلیل:** اصل «در شک، اجازه نده». اگر Broker Down است یا Tracker Bug دارد، Block امن‌تر از Allow است — هزینه‌ی Skip یک Order بسیار کمتر از هزینه‌ی اجرای یک Order در وضعیت نامطمئن است.

**پیامد:** اگر یک Guard به‌طور مزمن Bug بدهد، Run کاملاً متوقف می‌شود (TradingEvent Critical) — که Signal واضح برای رفع فوری است.

---

### D32 — Kill Switch: Global + Per-Account (Run = Stop موجود)

**تصمیم:** سه سطح Emergency Stop:
- **Global** — همه‌ی Runها و Accountها. `POST /api/Trader/Risk/Kill-Switch/Activate {scope:0}`
- **Per-Account** — فقط آن Account. `{scope:1, accountId}`
- **Per-Run** — `TradingRunController.Stop` موجود (بدون Endpoint جدید)

**دلیل:** Run-level Stop از قبل وجود دارد — استفاده‌ی مجدد، نه Endpoint جدید. Global و Account-level نیاز به Endpoint جدید دارند چون قبلاً نبودند.

**Action:** همه = Block Order جدید (نه Close، نه Fault). Positionهای باز با SL/TP محافظت می‌شوند. `Deactivate` برای بازیابی.

**یک limitation مستند:** در PR 14-17، `KillSwitchActivated`/`KillSwitchDeactivated` Events در Enum تعریف شدند ولی emit نمی‌شوند (TradingEvent جدول خالی برای این Types). Hotfix در PR بعدی: emit از Command Handlers.

---

### D33 — Cash Cache Invalidate-Driven (نه TTL-First)

**تصمیم:** `IAccountCashCache`:
- `RefreshAsync`: همیشه از Broker می‌خواند (~1s)
- `GetOrRefreshAsync`: از Cache اگر RefreshedAt < MaxAge (30 دقیقه)، وگرنه Refresh
- `Invalidate`: در `ExecutionCoordinator` بعد از Dispatch موفق، و در `OrderLifecycleStage` بعد از هر Fill

**دلیل:**
- قبل (`IAccountBalanceCache` با TTL 5s): هر tick پس از TTL، Broker Call → WAF risk + کند
- حالا: Refresh فقط هنگام تغییر واقعی Cash (Dispatch / Fill)
- MaxAge (30 min) Fail-Safe است اگر Invalidate از دست رفت (Bug)

**پیامد:** اگر کاربر Manual Trade در Broker انجام دهد، تا MaxAge (30 دقیقه) در Cache دیده نمی‌شود. در Phase B: WebSocket Broker Events یا `POST /Risk/Refresh-Cash/{accountId}` Endpoint برای دستی.

### D34 — PriceSource به عنوان Entity، نه فقط enum

**تصمیم:** `PriceSource` یک Entity است — هر Source یک instance پیکربندی‌شده با `Code` یکتا (مثلاً `tgju-usd`, `tgju-gold`, `telegram-channelX`), `Type` (enum محدود)، `ConfigJson` (provider-specific)، `FromAsset`/`ToAsset` پیش‌فرض.

**دلیل:**
- TGJU چندین indicator دارد (`price_dollar_rl`، `price_eur`، `gold_gram`، ...). هر کدام یک Source Instance است، نه یک enum value.
- افزودن Source جدید (Indicator جدید یا Provider جدید) → یک رکورد در DB، **صفر تغییر کد**.
- `ConfigJson` برای Provider-specific settings (نام indicator، کانال تلگرام، ...) — بدون schema change.

**Seed اولیه:** `ModuleInitializer` یک `tgju-usd` Sample را Seed می‌کند (فقط در DB خالی).

**Phase 2 trigger:** اگر تعداد Sourceها به ۵۰۰+ رسید و ConfigJson Query لازم شد، ستون‌های کلیدی را Denormalize می‌کنیم.

---

### D35 — PriceCollectionJob به عنوان Entity شبیه TradingRun

**تصمیم:** `PriceCollectionJob` یک Entity مستقل با Lifecycle شبیه `TradingRun` است:
- State: Created / Ready / Running / Stopped / Faulted
- Start / Stop از API
- `OwnerPersonId`/`OwnerUserId` (Multi-user آینده)
- Stats: `LastRunAt`, `LastRunInserted`, `TotalInserted`

**دلیل:** دقیقاً همان مسئله‌ی TradingRun — کاربر می‌خواهد «یک Job بسازم، Start/Stop کنم، ببینم چه شد». مقایسه:

┌──────────────────────┬─────────────────────┐
│ TradingRun │ PriceCollectionJob │
├──────────────────────┼─────────────────────┤
│ Strategy + Symbols │ Source + Pair │
│ Accounts │ (نمی‌خواهد) │
│ ExecutionSettings │ Schedule │
│ Start/Stop │ Start/Stop │
│ Recurring تا Stop │ Recurring + Backfill│
└──────────────────────┴─────────────────────┘

text

**چرا Entity جدا و نه "Source + Schedule" روی یک رکورد:** یک Source می‌تواند چند Job داشته باشد — مثلاً `tgju-usd` با «روزانه» و «هفتگی Backfill». Job جداست که هر کدام Lifecycle مستقل داشته باشد.

---

### D36 — CurrencyRate rename نشود (Phase C)

**تصمیم:** جدول `CurrencyRates` Rename نمی‌شود به `AssetPrices`، حتی با وجود `GOLD/USD` که «ارز نیست».

**دلیل:**
- ۱۰+ فایل به `CurrencyRate` وابسته‌اند: `DbCurrencyRateProvider`، `CurrencyCandleTransformer`، `LatestCurrencyRateSpec`، `CurrencyRateSpecs`, `ICurrencyRateProvider`, `CurrencyRateCache`، ...
- Semantic «Rate = چند ToAsset برای ۱ FromAsset» برای `GOLD/USD`، `COPPER/IRR` هم کاملاً کار می‌کند.
- Rename = ۳۰+ فایل + Migration + Risk نامه — بدون منفعت فنی.

**Phase C trigger:** اگر واقعاً مدل «Asset Price» لازم شد (مثلاً Multi-hop Transform: GOLD→USD→IRR)، آن‌وقت Rename + Refactor.

---

### D37 — CurrencyCode whitelist گسترش با کالاها

**تصمیم:** `CurrencyCode.Supported` حالا شامل Commodityها هم هست:
IRR, USD, EUR, GBP, AED, TRY, CNY, JPY, CHF,
GOLD, SILVER, COPPER, PLATINUM, OIL

text

**دلیل:** `CurrencyCandleTransformer` در هر tick با `CurrencyCode.NormalizeOrNull(TargetCurrency)` چک می‌کند. اگر GOLD در Whitelist نباشد، `TradingRun` با `TargetCurrency="GOLD"` ایجاد نمی‌شود (که غلط است — کاربر می‌خواهد قیمت سهم را به گرم طلا ببیند).

**Trade-off:** Semantics «Currency» برای GOLD دقیق نیست. ولی `CurrencyCode` یک Helper نازک است، نه یک VO با Invariantهای دقیق. Rename → Phase C.

---

### D38 — PriceSourceType = enum + Provider pattern

**تصمیم:** `PriceSourceType` یک enum محدود است (Tgju، TelegramChannel، CustomApi، ...)، و هر Type یک `IPriceSourceProvider` Implementation دارد. `IPriceSourceProviderResolver` مشابه `IMarketDataProviderResolver` آن را Resolve می‌کند.

**دلیل:** دقیقاً همان الگوی Broker Capability Segregation (D18):
- `TgjuPriceSourceProvider` فقط `ITgjuClient` را می‌شناسد
- `TelegramPriceSourceProvider` (Phase B2) فقط `ITelegramClient` را
- `PriceCollectionService` **هیچ switch روی Type ندارد** — Provider Resolver تصمیم می‌گیرد
- افزودن Provider جدید = پیاده‌سازی جدید + ثبت در DI، بدون تغییر Service

**Phase B1:** فقط `TgjuPriceSourceProvider`. `TelegramChannel` enum value وجود دارد (reserved) ولی Provider ندارد — اگر کسی `Type=TelegramChannel` بسازد، `Resolver` خطا می‌دهد.

---

### D39 — Scheduler Module برای هر دو Recurring و Backfill

**تصمیم:** Collection Jobs از `ISchedulerService` پلتفرم استفاده می‌کنند — نه BackgroundService جدید.

- **Recurring Job:** `ScheduleRecurringAsync(payload, pattern, startAt, tz)` — الگوی `ModuleInitializer` موجود
- **Backfill Job:** `ScheduleAsync(payload, fireAt: now, queue)` — یک‌بار اجرا، تمام

**دلیل:**
- Scheduler پلتفرم از قبل Retry، Persistence، TimeZone، QueryRecurring را دارد
- BackgroundService دوم = duplicate infrastructure
- Job Handler (`CollectPriceJobHandler`) همان الگوی `RetentionCleanupHandler` را دنبال می‌کند

**Stop:** `ISchedulerService.<Unschedule>(job.SchedulerRecurringId)`. اگر API اسمش متفاوت است (Cancel/Pause)، از همان استفاده می‌کنیم — به کد Scheduler وابسته است.

---

### D40 — Multi-source برای یک Pair: Transformer از Most Recent استفاده می‌کند

**تصمیم:** اگر برای یک `(From, To)` چند Source باشد (مثلاً USD/IRR از TGJU روزانه + از Telegram دقیقه‌ای)، `DbCurrencyRateProvider.GetRateAsync` نرخ را بر اساس `EffectiveAt DESC LIMIT 1` می‌خواند — **بدون فیلتر Source**.

**دلیل:**
- کاربر می‌خواهد «آخرین نرخ» را داشته باشد، نه «نرخ از Source مشخص»
- Canonical `LatestCurrencyRateSpec` از قبل همین است — بدون تغییر
- فیلتر Source در Backtest Config → Phase C (اگر کسی خواست Simulation با «نرخ از TGJU فقط» اجرا شود)

**Implied behavior:** اگر Telegram دقیقه‌ای نرخ بدهد و TGJU روزانه، `CurrencyCandleTransformer` در هر Candle از Telegram استفاده می‌کند — که دقیق‌تر است. این **تصادفی مطلوب** است.

---

### D41 — Sanity: Skip + Log Warning (نه Reject)

**تصمیم:** قبل از Persist، `PriceCollectionService` چک می‌کند:
- `rate > 0` — Skip اگر صفر یا منفی
- `rate` در بازه‌ی `[MinRate, MaxRate]` config — Skip اگر خارج
- جهش روزانه > `MaxDailyChangePercent` (default 20%) نسبت به نرخ آخر — **Skip + Warning، نه Reject**

**دلیل:**
- TGJU داده‌ی مشکل‌دار دارد (typo، اعداد صفر، field swap)
- ولی جهش واقعی > 20% هم ممکن است (خبر سیاسی، تحریم)
- Skip + Warning: کاربر در Events می‌بیند، خودش تصمیم می‌گیرد (اگر واقعی بود، دستی اضافه کند)

**Trade-off:** اگر TGJU یک روز ۳۰% جهش کند (واقعی)، آن روز Import نمی‌شود و کاربر باید دستی اضافه کند. Acceptable — چون Rare Event است و Audit Trail روشن است.

---

### D42 — Collection Idempotent با Unique Index

**تصمیم:** `CurrencyRate` از قبل `UX_CurrencyRate_Pair_EffectiveAt` دارد — Re-import همان روز، **همان EffectiveAt را Duplicate نمی‌کند**، بلکه Skip.

**Implied:** `ImportResult.Inserted` + `ImportResult.Skipped` هر دو در Response.

**دلیل:**
- Backfillهای جزئی (crash وسط) → Retry امن
- چند Source که همان `(From, To, EffectiveAt)` را بدهند → اول Persist می‌شود، دوم Skip (نه override)
- `Source` column فقط در **اولین** Persist ست می‌شود؛ نمی‌شود بعداً Override کرد. اگر لازم شد، Phase C: `UpsertRateCommand`.

### D43 — SchedulerRecurringId روی PriceCollectionJob

**تصمیم:** `PriceCollectionJob.SchedulerRecurringId` (Guid?, nullable) — در `Start` (Recurring) از `ScheduleRecurringAsync` پر می‌شود؛ در `Stop` با `CancelRecurringAsync(id)` صفر می‌شود.

**دلیل:** `QueryRecurringAsync` فیلتر Name ندارد. ذخیره ID بهترین راه برای Stop دقیق.

**Fallback (B2):** اگر DB از دست رفت، `QueryRecurringAsync(JobType="trader.price-collect")` + Parse PayloadJson + match با JobId — Recovery روی `ModuleInitializer`.

---

### D44 — Handler Resilience: Polly 3× → Throw → Scheduler Fail

**تصمیم:** `CollectPriceJobHandler.ExecuteAsync`:
1. Polly Retry 3× (1s، 3s، 9s backoff) روی `ITgjuClient` call
2. Transient error (5xx، timeout، network) → after 3 tries → **Throw**
3. Structural error (parse، schema، config invalid) → بدون Retry، فوری Throw

**پیامد:** `PriceCollectionJob.LastError` پر می‌شود. بعد از 10 consecutive failure، Scheduler خودش Recurring را Pause می‌کند؛ Handler تشخیص می‌دهد و `Job.MarkFaulted()` هم می‌زند.

**دلیل:** Silent skip در Daily Job = 24h تأخیر — غیرقابل قبول. Pause @ 10 = Signal واضح برای کاربر.

---

### D45 — Handler Dispatch بر اساس Mode، Idempotent Guard

**تصمیم:** `ExecuteAsync` ابتدا Job را reload می‌کند:
```csharp
var job = await _uow.PriceCollectionJobs.GetByIdAsync(payload.JobId);
if (job is null || job.State != PriceCollectionJobState.Running) return;

if (job.Mode == Recurring) await _collectionService.CollectOnceAsync(job.Id, ct);
else                       await _collectionService.CollectBackfillAsync(job.Id, ct);
دلیل (Idempotent): اگر Job در DB Stopped/Faulted است (کاربر Stop کرده)، Handler پیام Scheduler را نادیده می‌گیرد. Race-safe: کاربر Stop می‌زند، Scheduler job cancel می‌کند، ولی اگر در همین بین tick اجرا شود، guard آن را no-op می‌کند.

D46 — Start/Stop Flow + State Mirror
تصمیم — Start (Recurring):

text
1. Validate: Source exists، Active، FromAsset/ToAsset set
2. Parse RecurrencePatternJson
3. SchedulerRecurringId = ScheduleRecurringAsync(
       name: $"price-collect-{job.Id}",
       pattern, startAt: now+1min, maxFailures: 10, tz: Asia/Tehran)
4. Job.MarkRunningRecurring(SchedulerRecurringId, StartedAt: now)
5. SaveChanges
Stop (Recurring):

text
1. CancelRecurringAsync(job.SchedulerRecurringId)
2. Job.MarkStopped(StoppedAt: now, SchedulerRecurringId: null)
3. SaveChanges
Start (Backfill): ScheduleAsync(payload, fireAt: now, queue: "scheduler") → Job.MarkRunningBackfill(SchedulerJobId, StartedAt).

State Mirror:

هر Handler success → RecordRun(inserted, skipped) → LastRunAt, LastRunInserted, LastRunSkipped, TotalInserted += N

Backfill success → MarkCompleted() → State=Completed، BackfillProgressPercent=100

Recurring success → Job در Running می‌ماند

Handler throw + Scheduler paused → MarkFaulted()

D47 — AddScheduledJobHandler Pattern
تصمیم: services.AddScheduledJobHandler<CollectPricePayload, CollectPriceJobHandler>(); در Trader_AddInfrastructure.

پیامد: CollectPricePayload باید در Trader.Application باشد (که JobTypeRegistry.ScanAllAssemblies پیدا کند). [ScheduledJob("trader.price-collect")] روی Record.

همین الگو برای CollectCandlePayload (trader.historical-collect) هم اعمال شد (D49).

D48 — Scheduler Extension: Minutely / Hourly / Cron
تصمیم: Scheduler.Domain.RecurrenceType گسترش یافت:

Minutely = 3 (append)

Hourly = 4 (append)

Cron = 5 (append)

Factories جدید در RecurrencePattern:

Minutely(int interval)

Hourly(int interval)

Cron(string expression)

فیلد جدید: CronExpression (nullable، فقط برای Type=Cron).

GetNextOccurrence:

Sub-Daily (Minutely/Hourly): interval-based، DST-free

Cron: via Cronos 0.8.4، DST-aware

دلیل: Sub-Daily native در Scheduler، نه Workaround در Trader. Scenarios: هر ۵ دقیقه Telegram (B2)، هر ۳۰ دقیقه USD Live، هر ساعت Cache Refresh، هر ۵ دقیقه Session Refresh. هر Module آینده از این مزیت بهره‌مند می‌شود.

Backward Compat:

enum values 0/1/2 (Daily/Weekly/Monthly) بدون تغییر

ردیف‌های موجود {"type":0|1|2} در DB بدون Migration Parse می‌شوند

Constructor [JsonConstructor] با cronExpression: string? = null optional

Cron Library: Cronos 0.8.4 (MIT، used by Hangfire itself، DST-aware).

Superseded: پیشنهاد قبلی D48 («Sub-Daily در B2 via BackgroundService جدا») — Void. Sub-Daily native در Scheduler.

Scheduler.Tests: 28 تست (Minutely/Hourly/Cron + Daily/Weekly/Monthly regression + JSON compat).

### D49 — Historical Data Collection: هم‌ساختار با PriceCollection (Mode / State / Scheduler)

**تصمیم:** `HistoricalDataCollectionJob` از الگوی `PriceCollectionJob` تبعیت می‌کند:
- دو Mode: `Backfill` (یک‌بار، DateFrom..DateTo) و `Recurring` (Scheduler، از max(OpenTime) هر (Isin, TF) تا now)
- State Machine مشترک: `Created / Running / Stopped / Completed / Faulted`
- Scheduler Integration: `ScheduleAsync` برای Backfill، `ScheduleRecurringAsync` برای Recurring
- Handler جدا: `CollectCandleJobHandler` با idempotent guard (State≠Running → return)، Mode dispatch، MarkFaulted+rethrow
- Boundary per (Isin, TimeFrame) در Recurring — نه global
- `batch.Count < BatchSize` **نباید** trigger break باشد (API ممکن است batch میانی کوتاه برگرداند)؛ به‌جایش `previousOldest` guard برای no-progress → break

**دلیل:** حفظ پارادایم یکسان در Trader برای هر Collection Job (Learning curve، UX consistency، Testing pattern).

**پیامد:** `HistoricalDataCollectionWorker` و `HistoricalWorkerHealthCheck` حذف شدند — Scheduler جایگزین Worker pattern برای polling شد. `HistoricalDataCollectionJob.Status` (قدیمی) → `State` (جدید) با Migration data conversion.

---

### D50 — Collection Job boilerplate: تکرار عمدی تا سومین use-case (Rule of Three)

**تصمیم:** با وجود دو Collection Job موجود (`PriceCollectionJob`, `HistoricalDataCollectionJob`) که ~۷۰٪ boilerplate مشترک دارند (state machine، Start/Stop، Handler، Frontend table)، abstraction نمی‌کنیم تا **سومین** Collection Job ساخته شود.

**دلیل (Rule of Three):**
- با ۲ نمونه، نمی‌توان محدوده‌ی abstraction را دقیق تعریف کرد. Third use-case معمولاً variation جدیدی نشان می‌دهد (مثلاً boundary semantics متفاوت، mode اضافه، tracking متفاوت)
- هزینه‌ی refactor premature > هزینه‌ی refactor بعدی (بعدی = ۱ migration + تغییر دو entity؛ premature = ۲ migration + تثبیت B1 که همین امروز stable شد)
- Paper Trading Week (milestone بعدی) از B1 stable بیشترین سود را می‌برد — DRY نباید در این مقطع وقت بگیرد

**Trigger برای abstraction:** ساخت سومین Collection Job (مثلاً `FundamentalCollectionJob`, `NewsCollectionJob`) با رفتار مشترک اثبات‌شده.

**پس از trigger، سه سطح تصمیم:**
- **L1 (Frontend):** `GenericScheduledJobPage<TConfig>` — ریسک صفر (بدون EF/Migration)، ارزش فوری حتی با ۲ نمونه
- **L2 (Backend):** Base entity + helper service — ریسک متوسط (migration polymorphic schema: TPT vs TPH)، بعد از trigger
- **L3 (Full generic framework):** ریسک بالا، over-engineering، اجتناب تا ۴+ use-case

**Frontend L1 آزاد است حتی الان، ولی Backend L2/L3 تا trigger ممنوع.**

---

### D51 — Scheduler مرز پلتفرمی است، نه Trader

**تصمیم:** `Scheduler.Domain` / `Scheduler.Application` / `Scheduler.Infrastructure` یک **ماژول مستقل از Trader** است که در NexusPlatform در اختیار همه ماژول‌ها (Trader، HR، Finance، Identity، ...) و در آینده در پروژه‌های دیگر استفاده می‌شود. طراحی Scheduler باید **فراتر از نیازهای Trader** باشد:
- `IScheduledJobHandler<TPayload>` برای هر ماژول باز است — Trader فقط یکی از consumers است
- `RecurrencePattern` (Minutely/Hourly/Daily/Weekly/Monthly/Cron) و `Scheduler` lifecycle (Recurring/Paused/Completed/Cancelled، Idempotent dispatch، Startup recovery) باید در سطح platform کامل باشد
- اگر روزی abstraction سطح D50 (L2/L3) به trigger رسید، **باید در `Scheduler` پیاده شود، نه در `Trader`**. پیشنهاد: یک Sub-module جدید `Scheduler.Collection` که `IScheduledCollectionJob` + base entity + helper service فراهم کند. Trader (و ماژول‌های آینده) از آن consume می‌کنند
- Trader-level boilerplate امروز (`PriceCollectionService.StartJobAsync`، `CollectCandleJobHandler`، ...) به‌عنوان **Precedent برای Scheduler design** مستند است — وقتی abstraction وقتش رسید، design از همین دو نمونه استخراج می‌شود ولی در Scheduler مستقر می‌شود

**دلیل این محدودیت:**
- Scheduler مثل `Cache`، `Event`، `Audit` است — Cross-cutting، not domain-specific
- اگر abstraction در Trader بماند، ماژول‌های دیگر باید دوباره boilerplate بسازند یا به Trader وابسته شوند (نقض module boundaries)
- Scheduler باید idiom استاندارد پلتفرم باشد، نه Trader-owned

**پیامد فوری:** هیچ تغییری در Trader لازم نیست. این ADR صرفاً سند مرز و تصمیم آینده است.
### Phase B1 (PriceCollection / TGJU) — 🚧 DESIGN LOCKED, CODE PENDING

**Approved: D34-D42 + D43-D48**

**Scope:**
- `PriceSource` Entity + `PriceCollectionJob` Entity (D34, D35)
- `TgjuPriceSourceProvider` (B1 = only provider)
- Scheduler Module (Recurring: Minutely/Hourly/Daily/Weekly/Cron، D48)
- USD/IRR only (B2 = more assets)
- Endpoints: PriceSource CRUD + Job CRUD + Start/Stop + Status
- 25+ tests
تغییر ۳ — Section 14 (Recent Work):

markdown

### D52 — Order Preset Strategy Resolution: Pure domain logic, I/O در Service

**تصمیم:** `InstrumentOrderPreset` فقط "چه استراتژی و چه مقدار ثابت" را نگه می‌دارد. `ResolveAsync` (I/O برای live API) در `InstrumentOrderPresetService` است. Enum‌های `OrderPriceStrategy` / `OrderQuantityStrategy` در Domain هستند، منطق fallback (Closing×Range) در Service.

**دلیل:**
- Domain باید Pure باشد — فراخوانی broker API نباید در Entity باشد
- Resolve خروجی جدید (ResolvedOrderPresetView) برمی‌گرداند بدون تغییر state → می‌شود چند بار با Refresh فراخوانی کرد
- Preset state (`PriceStrategy`, `FixedPrice`) از resolve result مستقل است — شکست در resolve، preset را خراب نمی‌کند

**Fallback semantics (D52.1):**
- `HighAllowed`/`LowAllowed`: از live API → اگر null و Range set → `Closing × (1 ± Range%)` + warning → اگر هیچ‌کدام → **throw** با پیام واضح
- `LastTraded`/`Closing`/`FirstTraded`: از live API → اگر null → throw (هیچ fallback ندارند، چون ارزش ذاتاً متفاوت است)
- `QuantityStrategy`: `MaxOrderQuantity`/`MinOrderQuantity` از `Instrument` در همان transaction خوانده می‌شوند
- Sanity: اگر قیمت resolve شده خارج از range باشد، در `Warning` گزارش می‌شود (block نمی‌کند — Broker تصمیم می‌گیرد)

**Trade-off:** ترتیب استراتژی `Fixed` اول، سپس live، سپس fallback. اگر کاربر بخواهد `Live:HighAllowed` را حتی وقتی `Closing` هم null است (نباید اتفاق بیفتد چون Closing از session قبلی باقی می‌ماند) → خطا می‌گیرد. **این درست است** — سکوت خطرناک‌تر است.

---

### D53 — Range Up/Down جدا + nullable = unlimited

**تصمیم:** `Instrument.DailyPriceRangePercent` (یکی) → `DailyPriceRangeUpPercent` + `DailyPriceRangeDownPercent` (دو nullable).

**دلیل:**
- بازار واقعی نامتقارن است (+۵٪/-۲٪ در برخی نمادها)
- برخی نمادها هیچ محدودیتی ندارند (تازه‌عرضه, ETF با اهرم) — `null` = unlimited
- nullable با `HasValue` منطق fallback را در Resolve ساده می‌کند (بدون magic number 0)

**Migration:** بدون داده‌ی قبلی در DB (frontend اعمال نشده بود) — column drop/create.

**پیامد:** `ResolveAsync` logic و `Sanity` check باید `HasValue` را respect کنند — پیاده شد.

---

### D54 — Command Wrapping در Controller: pattern برای unwrap

**تصمیم (اصلاحیه):** Commandهایی که Model در خود wrap می‌کنند (مثل `CreateOrderPresetCommand(Model)`) نباید مستقیم با `[FromBody] Command` bind شوند. Controller باید Model را از body بگیرد و Command را در خود بسازد — مثل `Update` endpoint که از قبل همین کار را می‌کرد.

**دلیل:**
- فرانت با بدنه‌ی flat (`{name, priceStrategy, ...}`) می‌فرستد، ولی ASP.NET انتظار `{"model": {...}}` دارد
- خروجی: `The Model field is required` — پیام گمراه‌کننده
- تفاوت بین Create (wrap در body) و Update (unwrap در controller) inconsistent بود

**الگو:**
```csharp
[HttpPost("Create")]
public async Task<IActionResult> Create([FromBody] CreateOrderPresetModel model)
{
    var command = new CreateOrderPresetCommand(model);
    ...
}

Trigger for future: هر وقت Command یک Model property دارد → Controller unwrap می‌کند. یا در بدنه {"model":{...}} (درست ولی نه convention فرانت). این تصمیم: unwrap در Controller.

text

---
---

### D55 — Backtest/Live: Shared Domain، Separate Orchestration

**تصمیم:** Backtest و Live دو engine جدا هستند که در **Domain** مشترک‌اند (Strategy، FillMath، OrderSizeCalculator، StopLossConfig، TakeProfitConfig، ICandleTransformer) ولی orchestration، position tracking، و risk guards جدا هستند.

**Drift policy:**
- هر تغییر در Domain logic خودکار به Backtest می‌رسد (مطلوب)
- هر تغییر در orchestration یا risk config **باید دستی به Backtest منتقل شود** (پذیرفته)
- **Backtest Parity Checklist** در `docs/BACKTEST-PARITY.md` هر PR بک‌تست را چک می‌کند

**Critical drift fixes (این PR):**
1. `BacktestEngine.ResolveStopLoss`/`ResolveTakeProfit` حالا از `StopLossConfig.FromPercentage().ResolveForEntry()` / `TakeProfitConfig` استفاده می‌کنند — تغییر در VO خودکار propagate
2. `BacktestEngine.HandleSignal` حالا `MaxPositionValue` از `TradingRiskOptions.MaxPositionSize` را به `CalculateBuy` پاس می‌دهد — همان که `OrderIntentBuilder` (Live) می‌دهد

**Trade-off پذیرفته‌شده (Live-only، در Backtest اجرا نمی‌شود):**
- `KillSwitchGuard`, `DailyLossLimitGuard`, `CircuitBreakerGuard`, `RateLimiterGuard` — چون Backtest مفهوم «روز» و «consecutive loss» realtime ندارد (کندل‌ها سریع‌اند)
- Order lifecycle (Channel + Stage 4 polling)
- Broker Rejections / Partial fills

**Phase E trigger:** اگر AI/ML real money شد → Full refactor با `IClock` abstraction + `IPositionTracker` interface + `BacktestBrokerDispatcher`.

---

### D56 — SL/TP on Add to Position: Live unchanged، Backtest aligned

**تصمیم:** `Position.ApplyBuyFill` (Live) SL/TP را تغییر نمی‌دهد — intent اول set می‌کند، Add fill از intent خودش (که شاید SL/TP نداشته باشد) تبعیت می‌کند. **Backtest هم همان رفتار را دنبال می‌کند** (BC4 fix از ۲۰۲۶/۰۸/۲۰ revert شد).

**دلیل:** user intent = `StopLossPercentage=5` یعنی «۵٪ زیر اولین entry». Add کردن، intent را نقض نمی‌کند. اگر کاربر می‌خواست SL با avg حرکت کند، feature جدید است (Trailing SL، E.2) که باید صریحاً در **هر دو** Live و Backtest پیاده شود.

**پیامد:** Backtest و Live در رفتار SL/TP روی Add همگام‌اند.

---

### D57 — Backtest `TargetCurrency`: فقط Strategy Input، نه Execution

**تصمیم:** `BacktestRun.TargetCurrency` فقط بر ورودی Strategy اثر می‌گذارد (RSI/MACD روی کندل‌های USD-adjusted برای کاهش اثر تورم). اجرای سفارش، fill، SL/TP resolution، Cash، Position، PnL همه در **IRR** باقی می‌مانند.

**دلیل:** بازار سرمایه ایران همه‌ی قیمت‌گذاری‌ها را در ریال انجام می‌دهد. `TargetCurrency` ابزار تحلیلی است، نه تغییر واحد معامله. Live از ابتدا این‌طور بود (`OrderIntentBuilder` از `snapshotStore` IRR می‌خواند) — Backtest باید هم‌راستا شود.

**Implementation (Two-Stream):**
- `BacktestCandleEvent` دو candle دارد: `Candle` (IRR) + `StrategyCandle` (transformed، nullable)
- `state.LastPrices`, `ExecuteFill`, `_sltpResolver.Resolve`, `HandleSignal` (sizing) — همه از `Candle` (IRR)
- `runningCandles` (ورودی Strategy) از `StrategyCandle ?? Candle`
- `SubCandles` (فقط برای SL/TP resolution) — بدون transform، IRR
- Final close از `primaryCandlesIrr`

**پیامد:** گزارش نهایی (Entry/Exit price، PnL، Final Equity) همه IRR — همان که کاربر انتظار دارد. `TargetCurrency` صرفاً در `StrategyConfig` اثر دارد.

**تجربه‌ی واقعی:** قبل از این fix، Backtest با `TargetCurrency=USD` و سرمایه ۱۰۰M IRR، همه‌ی محاسبات را در USD انجام می‌داد → Entry/Exit Price در حدود ۰.۰۰۰۴ و PnL ≈ ۰.



### D58 — Backtest Position Quantity: One-Shot Mutation + Integrity Check

**تصمیم:** در `ExecuteFill` (buy-add path)، هر fill **دقیقاً یک بار** روی `Position.Quantity`, `WeightedAvgEntryPrice`, `TotalBuyCommission` effect می‌گذارد. یک integrity check در انتهای `ExecuteFill` این invariant را تضمین می‌کند: `Position.Quantity == sum(EntryFills.Quantity)` (وقتی Position باز است).

**Context (این باگ را کشف کرد):** بعد از BC4 fix و revert، سه خط duplicate در `if (state.Positions.TryGetValue(...))` باقی ماند:
```csharp
existingPos.Quantity += qty;              // ← خط ۱
existingPos.WeightedAvgEntryPrice = newAvg;
existingPos.TotalBuyCommission += commission;
// ... کامنت BC4 ...
existingPos.Quantity += qty;              // ← خط تکراری
existingPos.WeightedAvgEntryPrice = newAvg;
existingPos.TotalBuyCommission += commission;

پیامد: Buy دوم (261 + 185 → Quantity=631 نه 446)، Sell از pos.Quantity استفاده می‌کرد → ۱۸۵ سهم phantom proceeds به state.Cash اضافه می‌شد → بازده ۷۸۱٪ با ProfitFactor ۰.۲۸ (تناقض ریاضی).

Fix:

حذف سه خط تکراری

Integrity check:

csharp
if (state.Positions.TryGetValue(isin, out var checkPos))
{
    var expectedQty = checkPos.EntryFills.Sum(f => f.Quantity);
    if (checkPos.Quantity != expectedQty)
        throw new InvalidOperationException(
            $"Position integrity violated for {isin}: " +
            $"Quantity={checkPos.Quantity}, SumOfFills={expectedQty}");
}
Regression evidence: با حذف duplicateها، بازده از ۷۸۱٪ به −۳۶٪ رسید (ProfitFactor ۰.۴۵، Sharpe −۴.۵۷، MaxDD ۳۷.۵۵٪) — منطبق با WinRate ۴۹.۷۶٪ و استراتژی RSI M1 با MinSignalStrength=0.1.

D59 — Runtime Correctness: Paper/Live Segregation، Throttle Singleton، No-Swallow Persist
تصمیم — سه اصلاح:

B1. PositionCloser باید Paper/Live را رعایت کند

PositionCloser.CloseInternalAsync قبلاً IBrokerOrderDispatcher را inject می‌کرد (همیشه Live = BrokerOrderDispatcher) و OrderIntent.Create را بدون isPaper صدا می‌زد (default=false). نتیجه: هر Stop با ClosePositions روی Paper Run، سفارش Sell واقعی به Broker می‌فرستاد.

Fix:

IOrderDispatcherResolver inject شود (به‌جای IBrokerOrderDispatcher)

dispatcherResolver.Resolve(pos.IsPaper) per-position

OrderIntent.Create(..., isPaper: pos.IsPaper)

Session acquisition فقط برای Live (Paper dispatcher session را ignore می‌کند)

B2. OrderLifecycleStage throttle باید Singleton باشد

_lastRunAt یک instance field روی AddScoped بود → instance در هر tick عوض → reset به MinValue → throttle همیشه pass. Stage 4 در هر tick اجرا می‌شد، نه هر OrderLifecycleInterval.

Fix: IRuntimeThrottleState (Application) + RuntimeThrottleState (Infrastructure، Singleton) با TryClaimOrderLifecycle(TimeSpan) atomic (Interlocked.CompareExchange).

B3. ExecutionCoordinator نباید SaveChanges failure را swallow کند

قبلاً:

csharp
try { await _uow.SaveChangesAsync(ct); }
catch (Exception ex) { _logger.LogError(ex, ...); }   // ← no rethrow
Signal.MarkConsumed() در memory ConsumedAt را ست می‌کرد، ولی DB unchanged. Signal از Channel خارج شده → startup recovery آن را unconsumed می‌بیند → Re-push → duplicate dispatch.

Fix: throw; بعد از log — Stage 3 catch می‌کند، Signal در tick بعدی retry می‌شود، recovery با OrderIntentsBySignalId check کار خودش را می‌کند.

D60 — TGJU Recurring: Last-Page Fetch (API oldest-first)
تصمیم: IPriceSourceProvider.FetchAsync پارامتر maxPages: int را به fetchAllPages: bool تغییر داد. برای Recurring (fetchAllPages: false): probe recordsTotal (صفحه ۱ رکورد ۱) + fetch آخرین صفحه (start = recordsTotal - PageSize).

دلیل: TGJU API با وجود order_dir=asc در URL، در عمل ascending (oldest-first) برمی‌گرداند. Recurring که maxPages=1 می‌داد، فقط صفحه ۱ = قدیمی‌ترین (۲۰۱۳-۲۰۱۵) را می‌دید. بعد از حذف ۳ رکورد آخر از DB و اجرای Recurring، sinceEffectiveAt = ۴ روز پیش بود، و همه‌ی صفحه ۱ قدیمی‌تر → filter = ۰ → ۰ درج، ۰ skip، لاگ در سطح Info.

چرا probe + last-page به‌جای order_dir=desc: پارامتر API قابل اعتماد نیست (تجربی نشان داد ignore می‌شود). start = total - PageSize کاملاً مستقل از sort order است. هزینه: ۱ API call اضافی در هر tick — برای Recurring روزانه ~۲۴ call/day اضافه، قابل قبول.

Backfill بدون تغییر — از start=0 تا انتها fetch می‌کند (fetchAllPages: true).

تجربه‌ی واقعی: قبل از این fix، Backfill ۳ رکورد insert کرد ولی Recurring ۰؛ ریشه در همین دوگانگی page-range بود نه در دوگانگی کد.

D61 — Currency Rate CheckRange: Server-Side Validation
تصمیم: Frontend برای validation «نرخ ارز در بازه‌ی Backtest موجود است؟» از endpoint جدید GET /api/Trader/CurrencyRate/CheckRange?fromCurrency&toCurrency&from&to استفاده می‌کند که توسط ICurrencyRateService.CheckRangeExistsAsync سرو می‌شود (direct pair → reverse pair → bool).

دلیل:

GetList clamp داشت (Math.Clamp(limit, 1, 500) → raise به 10_000) — کاربر limit=50000 می‌فرستاد ولی ۵۰۰ رکورد می‌گرفت

Filter سمت Frontend (effectiveAt <= dateFrom) روی ۵۰۰ رکورد اخیر (۲۰۲۵-۲۰۲۶) → برای ۲۰۲۴ هیچ رکورد → false «missing» حتی با ۴۰۰۰ رکورد موجود از ۲۰۱۱

راه تمیز: server-side existence check در SQL، بدون انتقال داده

Handler → Service → UoW (convention): CheckRangeExistsAsync در CurrencyRateService (Infrastructure) پیاده شد — نه مستقیم _uow در Handler (نقض D-Conventions که Handler → Service).

Frontend usage:

hasBaseline = CheckRange(cur, "IRR", "1900-01-01", dateFrom) → ok

else hasAny = CheckRange(cur, "IRR", "1900-01-01", dateTo) → late-start (warning، submit مجاز) یا missing (block)
---
## ۲. `docs/HANDOFF.md` — Header (جایگزین)

```markdown
> **آخرین به‌روزرسانی:** ۱۴۰۴/۰۹/۲۲ — Phase C-lite.1 + C-lite.2 + Preset Strategy
> **وضعیت:** Phase A ✅ | Phase D ✅ | Scheduler ✅ | Phase B1 ✅ | C-lite.1/2 ✅ | 258 Tests ✅ | CI ✅
> **گام بعدی:** C-lite.3 (Dashboard) → Paper Trading Week

## 14. RECENT COMPLETED WORK

1. **Scheduler Extension** — `RecurrenceType.Minutely=3, Hourly=4, Cron=5` + Factories + `GetNextOccurrence` handling. Cronos 0.8.4. Backward compat (enum append). 28 tests در `Scheduler.Tests`. ⭐ New project.
2. **Docs Update (B1)** — `TRADING-DECISIONS.md` D34-D48 (PriceSource، Job، Cash، Scheduler، Cron)، `Backend-Architecture.md` Appendix (Scheduler capabilities)، `HANDOFF.md` Phase B1 status.
3. **PR 21** — Risk Dashboard UI (`/trader/risk`). Kill Switch Card + CircuitBreaker Card + Auto-refresh 15s.
4. **Risk Seed** — `trader.risk` Resource + Permission + Menu. `RiskController` به `trader.risk` منتقل شد.
5. **PR 20** — Kill Switch Events Hotfix. `ActivateKillSwitchCommandHandler`/`DeactivateKillSwitchCommandHandler` حالا `KillSwitchActivated`/`KillSwitchDeactivated` emit می‌کنند + Flush. +۵ تست.

**Next:** PR B1 — TGJU Import (design locked D34-D48).
---

## خلاصه D34-D42

| ID | موضوع | Type |
|---|---|---|
| D34 | PriceSource Entity (نه enum) | Phase B1 |
| D35 | PriceCollectionJob Entity (شبیه TradingRun) | Phase B1 |
| D36 | CurrencyRate rename نمی‌شود | Design |
| D37 | CurrencyCode Extended (GOLD، ...) | Phase B1 |
| D38 | PriceSourceType enum + Provider Pattern | Phase B1 |
| D39 | Scheduler Module برای Recurring + Backfill | Phase B1 |
| D40 | Most Recent Rate (Source-agnostic) | Design |
| D41 | Sanity = Skip + Warning (نه Reject) | Phase B1 |
| D42 | Collection Idempotent (UX already) | Design |
---

## Known Debt (اولویت Phase B)

| # | Debt | Impact | Phase |
|---|---|---|---|
| 1 | `ExecutionLogService.Skip/Take` در Memory (نه SQL) | با ۵۰,۰۰۰+ Log کند | B |
| 2 | `UseQuerySplittingBehavior` Commented | Cartesian در Run with N Symbol × M Account | B |
| 3 | Multi-Account Routing Policy ندارد (Fan-Out فقط) | برای Hedge / Risk Balancing نیاز | C |
| 4 | `ITraderUnitOfWork` یک Registry است، نه UoW واقعی | Naming گمراه‌کننده | B |
| 5 | Bonus / Stock Right در Reconciliation نادیده | Mismatch کاذب | A1+ |
| 6 | `TraderSymbol` Legacy (Refactor به `Instrument`) | Confusion، Duplicate Concept | E |
| 7 | `SchedulePlan` / `ScheduledOrder` Legacy | Overlap با `TradingRun` | C |
| 8 | `PaperTrading` Fill فوری در `intent.Price` | Realism کم | C |
| 9 | `TradingSchedule` Fallback به `TSE-REGULAR` | اگر نشد، همیشه باز (Behavior غیرمنتظره) | B |
| 10 | `CandleRecord.TimeFrame` Enum → int Cast در بعضی Queryها | Type Safety کم | Done (N5) |

---

## منابع مرتبط

- `docs/HANDOFF.md` — Status فعلی، Phase Status، Recent Work
- `docs/TRADING-STORY.md` — Narrative Pipeline (برای Context کیفی)
- Code Review Series — C1-C4، M1-M10، N1-N10 (در commit history قابل ردیابی با `Refs: full-review-2026-11`)