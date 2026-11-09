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