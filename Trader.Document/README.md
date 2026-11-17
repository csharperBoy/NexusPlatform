# ماژول Trader (معامله‌گری الگوریتمی)

> **در یک جمله:** Trader «موتور معامله‌گری» پلتفرم است؛ نمادهای بازار سرمایهٔ ایران (سهام و آپشن) را پایش می‌کند، استراتژی‌ها را روی کندل‌های قیمتی اجرا می‌کند، از دل آن‌ها سیگنال می‌سازد، و اگر ریسک اجازه بدهد سفارش را از طریق کارگزاری ثبت و تا لحظهٔ پر شدن پیگیری می‌کند.

| | |
|---|---|
| **پروژه‌ها** | `Trader.Domain` / `Trader.Application` / `Trader.Infrastructure` / `Trader.Presentation` + `Trader.ExternalApiClient` |
| **قرارداد عمومی** | به بیرون قرارداد صادر نمی‌کند؛ ورودی عمومی‌اش `Trader.Application` است: `I*CommandService` / `I*QueryService` و Command/Queryهای MediatR |
| **وابستگی به ماژول‌های دیگر** | `Scheduler` (اجرای زمان‌بندی‌شده)، `Authorization` / `Identity` / `Navigation` (فقط هنگام Seed)، `Core.Infrastructure` (DbContext، Repository، UoW، Outbox) |
| **چه کسانی از آن استفاده می‌کنند** | فقط `Trader.WebApi` به‌عنوان هاست و اپ FrontEnd تریدر؛ هیچ ماژولی به Trader رفرنس نمی‌دهد |
| **جایگاه پیشنهادی در مدل لایه‌ای** | لایهٔ آخر (مصرف‌کنندهٔ سرویس‌های پلتفرم) |

---

## ۱. این ماژول به چه درد می‌خورد؟ (به زبان ساده)

فرض کن می‌خواهی هر روز روی سه سهم و یک آپشن یک استراتژی مشخص را اجرا کنی: اگر RSI از ۳۰ رد شد بخر، اگر به ۷۰ رسید بفروش، حد ضرر ۲.۵٪. دستی یعنی دائم صفحهٔ بازار را نگاه کنی و در پنل کارگزاری سفارش بگذاری. Trader همین حلقه را خودکار می‌کند — به یک شرط: **همه‌چیز باید از قبل تعریف شده باشد**.

واحد کار، `TradingRun` است (در UI «پلن»): یک استراتژی + یک `PrimaryTimeFrame` + چند نماد با سهم سرمایه + یک یا چند حساب کارگزاری با سهم سرمایهٔ خودشان + حد سود/ضرر + `ExecutionMode` (واقعی یا شبیه‌سازی). تا پلن ساخته نشده هیچ اتفاقی نمی‌افتد؛ به‌محض `Start`، پلن وارد حافظهٔ Runtime می‌شود و موتور شروع به چرخیدن می‌کند.

هر «تیک» چهار مرحله دارد. **مرحلهٔ ۱** (`MarketDataIngestionStage`) اتحاد نمادهای پلن‌های فعال را برمی‌دارد، یک درخواست دسته‌ای به کارگزاری می‌زند، `MarketSnapshotRecord` را ذخیره و تصویر لحظه‌ای را در یک Store درون‌حافظه‌ای نگه می‌دارد. **مرحلهٔ ۲** (`StrategyEvaluationStage`) برای هر (پلن، نماد) کندل‌ها را می‌خواند — و اگر پلن ارز تحلیل مثل `USD` داشته باشد، کندل ریالی را با نرخ ارز تبدیل می‌کند — استراتژی را صدا می‌زند، `StrategyEvaluation` و در صورت وجود `Signal` را ذخیره می‌کند و سیگنال را به یک `Channel` می‌فرستد. **مرحلهٔ ۳** (`ExecutionStage`) سیگنال‌ها را تخلیه می‌کند: برای هر حساب `OrderIntent` می‌سازد (سمت، قیمت، تعداد)، از Pipeline ریسک عبور می‌دهد، Session کارگزاری را می‌گیرد و سفارش را می‌فرستد. **مرحلهٔ ۴** (`OrderLifecycleStage`) سفارش‌های باز را می‌خواند، `OrderTrade` را ثبت و روی `Position` اعمال می‌کند (میانگین قیمت ورود، سود/زیان تحقق‌یافته، بسته شدن جزئی یا کامل).

مسیر داده: `market data → strategy → signal → execution → order tracking`. این چهار مرحله Event-Driven نیستند؛ همه در یک حلقهٔ `PeriodicTimer` و پشت سر هم در `TradingRuntimeWorker` اجرا می‌شوند. کنارش یک **زیرسیستم بک‌تست** مستقل هست: `BacktestRun` یک Snapshot کامل از تنظیمات نگه می‌دارد تا بازتولیدپذیر باشد، `BacktestWorker` هر ۱۰ ثانیه `Pending`ها را برمی‌دارد، `BacktestEngine` + `BacktestSimulator` کندل‌ها را بازپخش می‌کنند، SL/TP با Sub-Candle حل می‌شود و خروجی به `BacktestTrade`، `BacktestDailyEquity` و متریک‌ها (بازده، حداکثر افت، Sharpe، WinRate، ProfitFactor) تبدیل می‌شود. یک بهینه‌ساز ژنتیک هم روی همین موتور سوار است. در حاشیه، نرخ ارز (اسکرپ TGJU) برای تحلیل دلاری، جمع‌آوری کندل تاریخی با Scheduler، مغایرت‌گیری با کارگزاری و دفتر رویداد فارسی (`TradingEvent`) هم کار می‌کنند.

## ۲. چه چیزهایی داخلش هست؟

**مدل دامنه (۳۵ موجودیت، ۳۸ Enum، ۱۱ Value Object).** قلب مدل `TradingRun` است با فرزندان `TradingRunSymbol` و `TradingRunAccount` و ماشین حالت `Created → Ready → Running → Stopped/Faulted`. بعد از آن: `Instrument` (سهام/آپشن با `OptionContract`، کمیسیون، بازهٔ نوسان روزانه)، `TraderAccount`، `Signal`، `OrderIntent` (تصمیم اجرا، نه سفارش)، `BrokerOrder` + `OrderTrade` (آنچه واقعاً اتفاق افتاده)، `Position`، `StrategyEvaluation`، `MarketSnapshotRecord`، `CandleRecord`، خانوادهٔ `BacktestRun`، `RiskKillSwitch` و `TradingEvent`.

**لایهٔ Application.** ۲۵ اینترفیس سرویس در ریشهٔ `Abstractions` (اکثراً Command/Query Service) و بعد گروه‌های `Runtime` (چهار Stage، `ISignalChannel`، `ITradingRunManager`)، `Execution`، `Risk`، `Brokers` (هفت Capability کلیدخورده با `BrokerType`)، `Backtesting`، `MarketData`، `Strategies`، `Positions`، `PriceCollection` و `Health`. روی این‌ها حدود ۴۰ Command و ۳۰ Query با MediatR نشسته که تقریباً همه پوستهٔ نازکی روی سرویس‌ها هستند و خطا را به `Result`/`Result<T>` تبدیل می‌کنند. چهار اندیکاتور (RSI، EMA، MACD، ATR)، دو استراتژی واقعی (`rsi` و `mtf-composite`) و یک Tuner ژنتیک هم اینجاست.

**لایهٔ Infrastructure.** چهار Stage (بین ۶۳ تا ۵۲۲ خط)، `PositionManager`/`PositionCloser`، شش Guard ریسک، توزیع‌کنندهٔ سفارش Live و Paper، مغایرت‌گیری، Providerهای داده‌بازار، جمع‌آوری قیمت (TGJU) و کندل تاریخی، داشبورد، نگهداشت رویدادها، لاگر بافرشدهٔ `TradingEventLogger`، و کارگزاری `EasyTrader` به‌صورت هفت Capability کلیدخورده با `BrokerType`.

**هاست‌ها و Workerها.** پنج `IHostedService`: `TradingRuntimeWorker` (حلقهٔ اصلی + Recovery هنگام بالا آمدن + Shutdown تمیز)، `BacktestWorker`، `StrategyTuningWorker`، `ClockSyncBackgroundService` (هم‌زمان‌سازی ساعت کارگزاری در ۰۸:۴۴:۵۶ و ۱۱:۴۴:۵۶ تهران) و `ModuleInitializer` (Seed منابع/مجوزها/منوها + Job روزانهٔ پاک‌سازی). چهار Job Handler هم از طریق Scheduler اجرا می‌شوند: `CollectPriceJobHandler`، `CollectCandleJobHandler`، `RetentionCleanupHandler`، `PlanExecutionJobHandler`.

**API.** ۲۵ کنترلر زیر `api/Trader/[controller]` با حدود ۱۳۰ Endpoint؛ همه به‌جز `DiagnosticController` با `[AuthorizeResource("trader.…", "View|Create|Edit|Delete")]` محافظت شده‌اند و `/health/trader` با پنج زیرمسیر بدون احراز هویت برای Probeها باز است.

## ۳. چه چیزی داخلش **نیست** (و نباید انتظارش را داشت)

- سرویس‌های هستهٔ پلتفرم (احراز هویت، مجوز، ناوبری، کش، موتور Scheduler، پردازشگر Outbox) اینجا نیستند؛ Trader فقط مصرف‌کنندهٔ آن‌هاست.
- کد UI داخل ماژول نیست. اپ تریدر یک SPA جداست (`FrontEnd/apps/Trader` و `FrontEnd/modules/Trader`) که فقط از این API استفاده می‌کند.
- کلاینت HTTP کارگزاری در پروژهٔ جداگانهٔ `Trader.ExternalApiClient` است، نه در Infrastructure؛ پس «افزودن یک کارگزاری» دو نقطهٔ تغییر دارد نه یکی.
- موتور قیمت‌گذاری آپشن نیست. `OptionContract` فقط متادیتاست (نوع، Strike، انقضا، دارایی پایه)؛ Black-Scholes، یونانی‌ها یا استراتژی مخصوص آپشن وجود ندارد.
- حسابداری مالی و دفتر کل نیست. `Position` و اسنپ‌شات‌های سرمایه تصویر لحظه‌ای تصمیم معاملاتی‌اند؛ `Reconciliation` هم با کارگزاری مقایسه می‌کند نه با دفتر مالی پلتفرم.
- گزارش‌گیری تحلیلی/BI و انبار داده ندارد؛ تنها نمای تحلیلی `IDashboardQueryService` است. هیچ `PublicService` هم برای ماژول‌های دیگر ندارد؛ Trader امروز «برگ» است.

## ۴. یک نگاه فنی کوتاه

- **DDD + CQRS.** موجودیت‌ها سازندهٔ کارخانه‌ای و گذارهای کنترل‌شده دارند (`TradingRun.Start/Stop/MarkFaulted`، `Position.ApplyBuyFill/ApplySellFill/ApplyBuyCancel`، `BacktestRun.MarkCompleted/MarkFailed`) و Propertyها `private set` هستند. هفت Domain Event تعریف شده و `EfUnitOfWork` هنگام `SaveChangesAsync` آن‌ها را به Outbox می‌ریزد.
- **دیتابیس.** یک `TraderDbContext` از `Base_DbContext`، `HasDefaultSchema("trader")`، جدول تاریخچهٔ `__TraderMigrationsHistory`، ۳۵ `DbSet`، ۳۵ Configuration و ۲۵ Migration. الگوی دسترسی `IRepository` / `ISpecificationRepository` / `IUnitOfWork` است؛ ولی `ITraderUnitOfWork<TraderDbContext>` عملاً یک Facade غول است که سازنده‌اش حدود ۶۰ پارامتر دارد.
- **Runtime.** یک `PeriodicTimer` با `HeartbeatInterval` (پیش‌فرض ۱۰ ثانیه) که Stageها را به ترتیب صدا می‌زند؛ خطای یک Stage تیک را نمی‌کشد. `SignalChannel` یک Channel محدود و Singleton بین مرحلهٔ ۲ و ۳ است و `ExecutionStage` در هر تیک حداکثر ۲۰۰ سیگنال برمی‌دارد. قبل از اولین تیک، `RuntimeRecoveryService` پلن‌های `Running` را دوباره در حافظه می‌نشاند.
- **کلاینت کارگزاری.** `EasyTraderApiClient` (حدود ۱۰۹۰ خط) ورود OIDC با PKCE را با اسکرپ HTML صفحهٔ لاگین انجام می‌دهد (`__RequestVerificationToken` را از HTML می‌خواند)، دادهٔ بازار را با GraphQL دسته‌ای می‌گیرد و گزارش سفارش‌ها را با فیلترهای Sieve می‌خواند. Session با `ISecretProtector` رمز و در `TraderAccount.EncryptedSession` ذخیره می‌شود، در Cache درون‌حافظه‌ای می‌ماند و گرفتنش با `SemaphoreSlim` برای هر حساب سریال می‌شود.
- **Resilience.** Timeout کوتاه روی HTTP، `PooledConnectionLifetime` بیست‌ثانیه‌ای برای فرار از Connection مرده، و یک Retry دستی با Connection تازه. در سطح دیتابیس، `TradingEventLogger` و `BacktestEventLogger` از Policyهای نام‌دار پلتفرم (`DbRetry` سپس `DefaultRetry` و در نهایت `NoOp`) استفاده می‌کنند. هیچ Policy برای HTTP تعریف نشده است.
- **Caching.** همه‌چیز درون‌حافظه‌ای و Singleton است: `IMarketSnapshotStore`، `ICandleCache`، `ITradingScheduleCache`، `IAccountCashCache`، `IRateLimiterTracker`، `IRiskStateTracker`، `IBrokerSessionCache`، `IRunExecutionContextCache`. `ICachePublicService` پلتفرم در Trader اصلاً استفاده نشده؛ یعنی افقی‌سازی امروز شکسته می‌شود و خود کد با `// TODO: When moving to multi-instance, replace with RedisBrokerSessionCache` به آن اعتراف کرده است.
- **ریسک.** Pipeline با ترتیب KillSwitch → DailyLossLimit → CircuitBreaker → RateLimiter، به‌علاوهٔ سقف اندازهٔ موقعیت که داخل `OrderIntentBuilder` چک می‌شود. Kill Switch در دیتابیس پایدار است و از API قابل تغییر است.
- **Scheduler.** چهار Payload، چهار Job Handler و یک `RecurringSchedule` روزانهٔ ۲۳:۳۰ تهران. Cron داخلی برای معامله‌گری وجود ندارد؛ حلقهٔ معاملات همان Heartbeat است و چک «بازار باز است؟» با `IMarketSessionProvider` انجام می‌شود.

## ۵. وضعیت واقعی امروز (صادقانه)

- **Outbox فقط پر می‌شود، خالی نمی‌شود.** `TraderDbContext` جدول Outbox را می‌سازد و `EfUnitOfWork` پنج نوع Domain Event را در آن می‌نویسد، اما برخلاف تقریباً همهٔ ماژول‌های دیگر (و برخلاف `TraderServer` که همین کار را می‌کرد) هیچ‌جا `registration.AddOutboxProcessor<TraderDbContext>(services)` صدا زده نشده است.
- **دو نسل هم‌زمان.** زیرسیستم قدیمی «سفارش زمان‌بندی‌شده» (`TraderSymbol`، `SchedulePlan`، `ScheduledOrder`، `ExecutionLog`، `ServerClockService`، `PlanExecutor` با ۵۰۰ خط) هنوز کامل ثبت، مسیر‌دهی و Seed شده و کنار موتور جدید زنده است. خود کد در `TraderDbContext` نوشته: `// Legacy — Refactor to Instrument pending`.
- **یک متد مرده در قرارداد.** `IExecutionCoordinator.CloseRunPositionsAsync` پیاده‌سازی ندارد و فقط لاگ می‌کند «not yet implemented»؛ بستن واقعی موقعیت‌ها در `TradingRunManager.StopAsync` با `IPositionCloser` انجام می‌شود. دو مسیر برای یک کار، یکی بی‌اثر.
- **باگ بایندینگ تنظیمات.** `PriceCollectionOptions.SectionName` برابر `"Trader:PriceCollection"` است، ولی در `appsettings.json` این بخش زیر `Trader:Risk:PriceCollection` نشسته؛ پس آن چهار مقدار بایند نمی‌شوند و Defaultهای کد (مثلاً `DefaultMinRate = 100000`) بی‌صدا برنده می‌شوند.
- **ثبت‌های تکراری در DI.** `Configure<PriceCollectionOptions>` دو بار پشت‌سرهم، `AddHttpClient("EasyTrader")` دو بار با Timeout متفاوت (۱۰ ثانیه در `Trader.ExternalApiClient` و ۳۰ ثانیه در `AddEasyTraderBroker`) و `AddSingleton<EasyTraderApiClient>()` هم دو بار. همچنین `Trader_AddPresentation` خودش `AddControllers()` و `AddMediatR()` را صدا می‌زند، درحالی‌که `Trader_AddApplication` هم MediatR را ثبت کرده است.
- **کنترل دسترسی فقط در لبه است.** هیچ موجودیتی در `Trader.Domain` صفت `[SecuredResource]` ندارد؛ بنابراین `ApplyFilter` و `CheckPermissionAsync` در Repository برای Trader عملاً No-op است. مجوز فقط با `[AuthorizeResource]` روی Endpoint چک می‌شود و مالکیت را ماژول Authorization با `RowLevelSecurityProcessor` مهر می‌زند. همچنین پنج منبع Seed شده (`trader.backtest`، `trader.currencyrate`، `trader.tradingevent`، `trader.scheduledorder`، `trader.tuning`) هیچ‌جا اعمال نمی‌شوند: Endpointهای Backtest از `trader.tradingrun` و CurrencyRate از `trader.instrument` استفاده می‌کنند، و `DiagnosticController` هیچ محافظی ندارد.
- **مالکیت به ماژول دیگر وابسته است.** `TradingRunService.CreateAsync` مقدار `ownerPersonId: Guid.Empty` می‌فرستد و هیچ‌جای Trader `SetUserOwner` را صدا نمی‌زند؛ پس اگر کد در Scope بدون `UserDataContext` (Worker، Seed، تست) اجرا شود، `StartAsync` با خطای «has no OwnerUserId» شکست می‌خورد.
- **سمت خواندن، Repository را دور می‌زند.** شش سرویس Query مستقیماً `TraderDbContext` می‌گیرند و `PriceCollectionService` برای نوشتن دسته‌ای از `_uow.Context.CurrencyRates` استفاده می‌کند — یعنی مسیر مهر مالکیت و چک مجوز را رد می‌کند.
- **پوشش تست نابرابر.** ۲۶۲ تست (۲۵۵ `[Fact]` و ۷ `[Theory]`) در ۳۷ فایل. خوب پوشش داده‌شده: ریاضیات Fill، Value Objectها، اندیکاتورها، استراتژی‌ها، Guardهای ریسک، حجم سفارش، جمع‌آوری قیمت و کندل. بدون هیچ تست: `BacktestEngine`/`BacktestSimulator`، `ExecutionCoordinator`، `OrderLifecycleStage`، `MarketDataIngestionStage`، `RuntimeRecoveryService`، `ReconciliationService`، `TradingRunService`، `BrokerSessionProvider` و Capabilityهای EasyTrader؛ یعنی خطرناک‌ترین بخش‌ها کمترین پوشش را دارند.
- **بدهی‌های کوچک ولی واقعی.** پوشه‌های `TraderServer.*` (۳۷ فایل، نسخهٔ قبلی همین ماژول) در `NexusPlatform.sln` نیستند و بار مرده‌اند؛ `Trader.Domain` به `Trader.ExternalApiClient` رفرنس می‌دهد و این وابستگی فقط برای نگاشت `OrderStateKind` است (وارونگی لایه)؛ `Program.cs` هاست هنوز «AkSteel Management application» را لاگ می‌کند و `OrderLifecycleStage` یک `using Microsoft.AspNetCore.Components;` بی‌ربط دارد.
