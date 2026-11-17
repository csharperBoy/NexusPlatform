# ماژول Scheduler (زمان‌بندی کارها)

> **در یک جمله:** Scheduler به ماژول‌های دیگر می‌گوید «این کار را در این لحظه (یا با این الگوی تکرار) اجرا کن»؛ اجرای واقعی روی **Hangfire** سوار است، اما رکورد کار، وضعیت، تکرار و بازیابی همه در دیتابیس خود Scheduler نگه داشته می‌شود.

| | |
|---|---|
| **پروژه‌ها** | `Scheduler.Domain` / `Scheduler.Application` / `Scheduler.Infrastructure` / `Scheduler.Presentation` + `Scheduler.Tests` |
| **قرارداد عمومی** | `ISchedulerService` (در `Scheduler.Application.Abstractions` — این‌بار سر جای خودش است)، به‌همراه `IScheduledJobPayload`، `IScheduledJobHandler<T>`، `[ScheduledJob]` و `AddScheduledJobHandler<T,THandler>()` |
| **وابستگی به ماژول‌های دیگر** | Hangfire (SQL Server storage)، Navigation (ثبت منو)، Authorization و Identity (ثبت منبع/مجوز)، Core |
| **چه کسانی از آن استفاده می‌کنند** | فقط ماژول `Trader` (چهار payload: `CollectPricePayload`، `CollectCandlePayload`، `RetentionCleanupPayload`، `PlanExecutionPayload`) و `Trader.WebApi` که ماژول را فعال می‌کند |
| **جایگاه پیشنهادی در مدل لایه‌ای** | لایه ۱ (سرویس‌دهنده، ولی تنها ماژولی که واقعاً یک API عمومی تمیز دارد) |

---

## ۱. این ماژول به چه کار می‌آید؟ (به زبان ساده)

دو نوع نیاز زمان‌بندی وجود دارد و Scheduler هر دو را پوشش می‌دهد:

1. **یک‌باره (One-off):** «این کار را ساعت ۱۴:۳۰ انجام بده.»
2. **تکرارشونده (Recurring):** «هر روز ۹ صبح»، «هر ۵ دقیقه»، «اول هر ماه»، «`0 9 * * 1-5`».

مدل ذهنی Scheduler این است: **تعریف کار و اجرای کار از هم جدا هستند.** ماژول مصرف‌کننده فقط یک payload می‌سازد (یک رکورد ساده) و می‌گوید کِی اجرا شود. Scheduler یک ردیف `ScheduledJob` می‌نویسد، یک Job تأخیری در Hangfire ثبت می‌کند که به یک دیسپچر ثابت اشاره می‌کند، و در لحظهٔ موعد، payload را Deserialize می‌کند و `IScheduledJobHandler<TPayload>` مربوطه را صدا می‌زند.

نکتهٔ مهم: Scheduler **نمی‌داند** کار چه می‌کند. `Scheduler.Application` هیچ رفرنسی به Trader ندارد؛ ارتباط از طریق `IScheduledJobPayload` (پارامتر نوع) برقرار می‌شود. این تنها ماژولی از این پنج ماژول است که قرارداد عمومی‌اش درست، در لایهٔ خودش، تعریف شده.

## ۲. چه چیزهایی داخلش هست؟

### دامنه

- **`ScheduledJob`** — رکورد یک اجرا: `JobType` (کلید payload)، `HangfireJobId`، `PayloadJson`، `FireAt` (UTC)، `PreFireBuffer`، `State` (`Pending`/`Executing`/`Succeeded`/`Failed`/`Cancelled`)، `StartedAt`/`CompletedAt`/`Error` و `RecurringScheduleId` (اگر از یک تکرار آمده باشد). متدهای گذار: `MarkExecuting`/`MarkSucceeded`/`MarkFailed`/`MarkCancelled`.
- **`RecurringSchedule`** — تعریف تکرار: `Name`، `JobType`، `PayloadJson`، `RecurrencePatternJson`، `TimeZoneId`، `StartAt`/`EndAt`، `MaxOccurrences`، `MaxFailures` (پیش‌فرض ۱۰)، `State` (`Active`/`Paused`/`Completed`/`Cancelled`)، شمارنده‌ها و `NextRunAt`.
- **`RecurrencePattern`** — یک Value Object خالص (بدون I/O) با انواع `Minutely`، `Hourly`، `Daily`، `Weekly`، `Monthly` و `Cron` (با `Cronos`). محاسبهٔ رخداد بعدی DST‑آگاه است و برای `Monthly` مقدار `-1` یعنی «آخرین روز ماه». `Interval` باید > 0 باشد و Cron فقط وقتی مجاز است که `Type == Cron`.

### اجرا

- **`HangfireSchedulerService`** — پیاده‌سازی `ISchedulerService`. در `ScheduleAsync` ابتدا رکورد دامنه را ذخیره می‌کند (با `HangfireJobId = "PENDING"`)، بعد `IBackgroundJobClient.Schedule` را روی `HangfireJobDispatcher` صدا می‌زند و شناسهٔ Hangfire را برمی‌گرداند. این کلاس **`internal`** است (`class HangfireSchedulerService` بدون `public`)؛ از بیرون فقط با `ISchedulerService` می‌بینی‌اش.
- **`HangfireJobDispatcher`** — نقطهٔ ورود همهٔ اجراها. `[Queue("scheduler")]`، `[AutomaticRetry(Attempts = 0)]`. کارها: Scope می‌سازد، رکورد را می‌خواند، **Idempotency** (اگر `Succeeded` یا `Cancelled` است برمی‌گردد)، `MarkExecuting`، Deserialize، اجرای `IScheduledJobHandler<T>` با Reflection، `MarkSucceeded`/`MarkFailed`، و در پایان Reschedule در صورت تکرارشونده بودن.
- **`PreciseDelay`** — چون Hangfire با دقت ثانیه کار می‌کند، Job با `PreFireBuffer` (پیش‌فرض ۱۵ ثانیه) **زودتر** Trigger می‌شود و بعد داخل دیسپچر با `Task.Delay` + `SpinWait` تا `FireAt` واقعی صبر می‌کند (~۱–۲ms).
- **`JobTypeRegistry`** — نگاشت `Key → Type` با اسکن خودکار همهٔ Assemblyهای `AppDomain` (فقط Typeهای public که `IScheduledJobPayload` دارند و `[ScheduledJob]` دارند) و امکان ثبت دستی با `AddScheduledJobHandler`.
- **`RecurringScheduleRescheduler`** — بعد از هر اجرا رخداد بعدی را می‌سازد («Self-Rescheduling»). با `CurrentScheduledJobId` از دوباره‌کاری محافظت می‌کند؛ `MaxOccurrences`، `EndAt` و `MaxFailures` را رعایت می‌کند و در سقف خطا، `State = Paused` می‌شود.
- **`RecurringScheduleRecovery`** + **`RecurringScheduleStartupService`** — بعد از `ApplicationStarted` (با ۳ ثانیه تأخیر) همهٔ `RecurringSchedule`ها را می‌گردد: اگر `Active` است ولی Occurrence معلقی ندارد، رخداد بعدی را می‌سازد؛ اگر `Paused`/`Cancelled`/`Completed` است ولی Occurrence معلق دارد، آن را Orphan می‌شناسد و Cancel می‌کند.
- **`SchedulerUnitOfWork`** — `ISchedulerUnitOfWork<SchedulerDbContext>` که علاوه بر `IUnitOfWork` دو Repository آماده (`ScheduledJobs`، `RecurringSchedules`) می‌دهد.

### سطح API و Seed

- **`SchedulerController`** — مسیر `api/Scheduler/[controller]`: `GET {id}`، `POST Query`، `DELETE {id}`، `POST Recurring/Create`، `GET Recurring/{id}`، `POST Recurring/Query`، `POST Recurring/{id}/Pause`، `Resume`، `DELETE Recurring/{id}`. همه از MediatR رد می‌شوند.
- **Seed** — منبع `scheduler` و `scheduler.job` با مجوز Full برای نقش ادمین + منوی «مدیریت برنامه‌های زمان‌بندی‌شده» و زیرمنوی «برنامه‌های زمان بندی شده».
- **Migration** — اسکیمای `scheduler`، تاریخچهٔ `__SchedulerMigrationsHistory`، و علاوه بر آن Hangfire هم با `SchemaName = "scheduler"` و `PrepareSchemaIfNecessary = true` جدول‌های خودش را در همان اسکیما می‌سازد.

## ۳. چه چیزهایی داخلش **نیست**

- **Hangfire Dashboard ندارد.** هیچ `UseHangfireDashboard` در مخزن نیست؛ برای دیدن صف باید مستقیم به جدول‌های Hangfire نگاه کنی.
- **`RecurringJob` آمادهٔ Hangfire استفاده نمی‌کند.** تکرار با زنجیرهٔ Self-Rescheduling پیاده شده (یک Job تأخیری در هر رخداد)، نه با `RecurringJob.AddOrUpdate`.
- **Retry خودکار ندارد.** `[AutomaticRetry(Attempts = 0)]` روی دیسپچر است؛ تلاش دوباره فقط از طریق `MaxFailures`/Pause منطق می‌شود، نه Retry فوری.
- **API ویرایش payload ندارد.** فقط Create/Cancel/Pause/Resume؛ تغییر payload یعنی Cancel و ساخت مجدد.
- **Health Check عملاً خالی است.** `Scheduler_AddHealthChecks` هیچ چکی اضافه نمی‌کند (بدنه‌اش فقط کامنت است).
- **UI ندارد.** فقط API.

## ۴. یک نگاه فنی کوتاه

- گزینه‌ها در بخش `Scheduler` بایند می‌شوند: `DefaultQueue = "scheduler"`، `DefaultPreFireBuffer = 00:00:15`، `SchedulePollIntervalSeconds = 5`، `WorkerCount = 50`. فقط `Trader.WebApi/appsettings.json` این بخش را دارد.
- `SchedulerDbContext : Base_DbContext` با اسکیمای `scheduler`، `DbSet<ScheduledJob>` و `DbSet<RecurringSchedule>`، پیکربندی `ScheduledJobConfiguration` (ایندکس یکتا روی `HangfireJobId`، ایندکس روی `FireAt` و `State`، ذخیرهٔ `PreFireBuffer` به‌صورت Ticks) و Outbox.
- ثبت با کنوانسیون پلتفرم: `Scheduler_AddApplication` (MediatR)، `Scheduler_AddInfrastructure` (DbContext + `AddHangfire` + `AddHangfireServer` + Registry Singleton + UoW + دیسپچر + Rescheduler/Recovery + `AddHostedService<RecurringScheduleStartupService>`)، `Scheduler_UseInfrastructure` (Migration).
- منطقهٔ زمانی پیش‌فرض: `Iran Standard Time` روی Windows و `Asia/Tehran` روی بقیه — یک انتخاب هاردکد که در `ScheduleRecurringAsync` و `CreateRecurringScheduleCommandHandler` تکرار شده.
- `Name` یک `RecurringSchedule` باید یکتا باشد؛ تکرار نام، `InvalidOperationException` می‌دهد.

## ۵. وضعیت واقعی امروز (صادقانه)

- **کنترلر هیچ `[Authorize]` یا `[AuthorizeResource]` ندارد.** با اینکه Seed منبع `scheduler.job` و مجوز Full را ثبت می‌کند، هیچ Endpointی آن را اعمال نمی‌کند. عملاً هر کسی که به شبکه دسترسی داشته باشد می‌تواند Job بسازد یا لغو کند. این جدی‌ترین ریسک این ماژول است.
- **Health Check خالی است** ولی `Scheduler_AddHealthChecks` در Loader صدا زده می‌شود؛ پس `/health` هیچ خبری از Scheduler ندارد.
- **`RecurringScheduleRecovery` وابستگی بی‌استفاده دارد:** `ISchedulerService _scheduler` تزریق و ذخیره می‌شود ولی هیچ‌جا صدا زده نمی‌شود (در لاگ کد هم توضیح داده شده که عمداً مستقیم Occurrence می‌سازد). فقط یک تزریق مرده است.
- **`HangfireSchedulerService` internal است** — از بیرون قابل ارث‌بری/تست مستقیم نیست؛ تست فقط از طریق `ISchedulerService` ممکن است.
- **`CreateRecurringScheduleCommandHandler` با Reflection کار می‌کند:** `MakeGenericMethod(payloadType)` روی `ISchedulerService.ScheduleRecurringAsync`. کار می‌کند ولی اگر امضای متد عوض شود، خطا در زمان اجراست نه کامپایل.
- **دقت بالا با Busy-Wait:** `PreciseDelay` در ~۱۵ms آخر با `SpinWait` می‌چرخد. با `WorkerCount = 50` و چند Job هم‌زمان، این می‌تواند CPU بخورد. کد صادقانه گفته «دقت ~۱–۲ms»، ولی هزینه‌اش مصرف CPU در Worker است.
- **نام فایل `Scheduler.Domain/Entities/ScheduledJob.cs.cs`** دو پسوند دارد (اشتباه تایپی)؛ روی Build اثری ندارد ولی پیدا کردنش با جست‌وجو را سخت می‌کند.
- **تست‌ها فقط دامنه را پوشش می‌دهند:** `Scheduler.Tests/Domain/ValueObjects/RecurrencePatternTests.cs` (۳۴۶ خط، Minutely/Hourly/Daily/Weekly/Monthly/Cron). هیچ تستی برای Dispatcher، Rescheduler، Recovery یا Service وجود ندارد — یعنی حساسترین بخش (زنجیرهٔ تکرار و بازیابی) تست ندارد.
- **فعال بودن ماژول محدود است:** فقط `Trader.WebApi` آن را در `Modules:Enabled` دارد. در `AkSteel.WebApi` و `PhoneBook.WebApi` اصلاً بارگذاری نمی‌شود.
- **جمع‌بندی:** این ماژول از نظر طراحی بالغ‌ترین ماژول این گروه است (قرارداد در جای درست، Value Object تست‌شده، بازیابی و Idempotency فکر‌شده)؛ ضعف اصلی‌اش امنیت Endpointها و نبود تست برای لایهٔ اجراست.
