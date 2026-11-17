# ماژول Event (رویداد دامنه و Outbox)

> **در یک جمله:** Event موتور انتشار قابل‌اعتماد رویدادهای دامنه است؛ رویداد را در همان تراکنش دیتابیس داخل جدول Outbox می‌نویسد و یک سرویس پس‌زمینه بعداً آن را برمی‌دارد و به MediatR می‌دهد تا هندلرها اجرا شوند.

| | |
|---|---|
| **پروژه‌ها** | `Event.Domain` و `Event.Application` (هر دو بدون کد) / `Event.Infrastructure` (تمام منطق) / `Event.Presentation` (بدون کد) — به‌همراه `Event.Test` |
| **قرارداد عمومی** | `IEventBus`، `IOutboxService<TDbContext>` و `IOutboxProcessorRegistration` در `Core.Application/Abstractions/Events/`؛ موجودیت `OutboxMessage` در `Core.Domain/Common/OutboxMessage.cs` |
| **وابستگی به ماژول‌های دیگر** | ندارد؛ ولی تقریباً همهٔ ماژول‌ها به آن وابسته‌اند |
| **چه کسانی از آن استفاده می‌کنند** | Audit، Base، Identity، Authorization، Navigation (دو کپی)، Contact، People، PhoneBook، Sample، HR، User، TraderServer (ثبت پردازشگر) و Scheduler / Trader / HR.IrisaSync (فقط نوشتن در Outbox) |
| **جایگاه پیشنهادی در مدل لایه‌ای** | لایه ۰ (سرویس فنی مشترک) |

---

## ۱. این ماژول به چه درد می‌خورد؟ (به زبان ساده)

وقتی یک Command رکوردی را تغییر می‌دهد، چند ماژول دیگر باید باخبر شوند: کش باطل شود، Audit ثبت کند، Notification ایمیل بفرستد. صدا زدن مستقیم این کارها تراکنش را طولانی می‌کند و شکست ارسال ایمیل، تغییر اصلی را زیر سؤال می‌برد. Event این را با الگوی Outbox حل می‌کند.

**نیمهٔ نوشتن:** موجودیت رویدادش را با `AddDomainEvent(...)` نگه می‌دارد. `EfUnitOfWork<TContext>.SaveChangesAsync` در `Core.Infrastructure/Repositories/EfUnitOfWork.cs` قبل از `SaveChanges` رویدادها را جمع می‌کند و به `IOutboxService<TContext>.AddEventsAsync` می‌دهد؛ چون در همان `SaveChanges` اضافه می‌شوند، «تغییر داده» و «ثبت رویداد» اتمیک‌اند.

**نیمهٔ خواندن:** یک `BackgroundService` پیام‌های `Pending` را می‌خواند، نوع رویداد را از `AssemblyQualifiedName` بازسازی می‌کند، JSON را دی‌سریالایز می‌کند و با `IEventBus.PublishAsync` به MediatR می‌دهد؛ بعد وضعیت `Completed` یا `Failed` می‌شود.

نکتهٔ مهم: **Event هیچ DbContext و اسکیمای اختصاصی ندارد.** جدول Outbox داخل دیتابیس هر ماژول و با اسکیمای همان ماژول ساخته می‌شود (`audit.OutboxMessages`، `people.OutboxMessages`). این ماژول «رفتار» است، نه «داده».

## ۲. چه چیزهایی داخلش هست؟

- **`OutboxService<TDbContext>`** — افزودن رویدادها، خواندن Pending (پیش‌فرض ۱۰۰ تایی)، تغییر وضعیت به Processing/Completed/Failed و یک `CleanupProcessedMessagesAsync`.
- **`MediatorEventBus`** — `IEventBus` روی `IMediator`، با دو اورلود `PublishAsync` (یکی برای یک رویداد، یکی `params` که در حلقه منتشر می‌کند).
- **`OutboxProcessorRegistration`** — تنها کارش `AddHostedService<HybridOutboxProcessor<TDbContext>>()` است.
- **`HybridOutboxProcessor<TDbContext>`** — تنها پردازشگر ثبت‌شده؛ بر اساس `Database:Provider` انتخاب می‌کند: `sqlserver` → `SqlDependencyOutboxProcessor`، `postgresql` → هشدار و polling، هر چیز دیگر → `OutboxProcessor`.
- **پردازشگرهای دیگر** — `OutboxProcessor` (polling تطبیقی)، `SqlDependencyOutboxProcessor` (Service Broker)، `PostgresOutboxProcessor` (`LISTEN/NOTIFY`) و `TriggerBasedOutboxProcessor` (polling پنج‌ثانیه‌ای).
- **`Event.Test`** — پنج فایل: رفتار پردازشگر، انتخاب پردازشگر، `OutboxMessage`، `OutboxService` و `DomainEventHandler`.

## ۳. چه چیزی داخلش **نیست** (و نباید انتظارش را داشت)

- **هیچ Brokerی ندارد.** «انتشار» یعنی صدا زدن MediatR در همان پروسه؛ پس برای «بین ماژول‌ها در یک اپ» است، نه «بین سرویس‌ها».
- **تضمین ترتیب و تضمین یک‌بار اجرا ندارد.** هر DbContext پردازشگر خودش را دارد و اگر پروسه بین انتشار و ثبت وضعیت بمیرد، پیام دوباره منتشر می‌شود.
- **تلاش مجدد برای Failed ندارد:** نه retry خودکار، نه dead-letter، نه Endpointی برای replay.
- **زمان‌بندی پاک‌سازی ندارد.** `CleanupProcessedMessagesAsync` هیچ‌جا صدا زده نمی‌شود.
- **مدیریت نسخهٔ رویداد ندارد.** `EventVersion` هست ولی هیچ منطق upgradeی برایش نیست.
- **هیچ لایهٔ Application/Domain ندارد:** نه Command، نه Query، نه Controller، نه Validator.

## ۴. یک نگاه فنی کوتاه

- جدول `OutboxMessages` با `Core.Infrastructure/Database/Configurations/OutboxMessageConfiguration.cs` ساخته می‌شود؛ سازنده‌اش نام اسکیما را می‌گیرد و هر DbContext آن را با اسکیمای خودش apply می‌کند. ایندکس روی `(Status, OccurredOnUtc)` و `RowVersion` به‌عنوان Concurrency Token.
- `Event_AddInfrastructure` سه چیز ثبت می‌کند: `IOutboxProcessorRegistration` (Singleton)، `IEventBus` (Scoped) و `IOutboxService<>` (Transient). `Event_AddHealthChecks` و `Event_UseInfrastructure` خالی‌اند و `Event_AddApplication`/`Event_AddDomain` وجود ندارند.
- ترتیب بارگذاری مهم است: ماژول‌ها در `{Module}_AddInfrastructure` خودشان `IOutboxProcessorRegistration` را Resolve می‌کنند، پس `Event` باید `Order` کوچک‌تری داشته باشد — در همهٔ appsettingsها `Order = 15`.
- بازسازی نوع دو مرحله‌ای است: اول `Type.GetType(AssemblyQualifiedName)` و اگر نشد، جست‌وجو در اسمبلی‌های لودشده بر اساس `TypeName`.
- در مدل لایه‌ای پیشنهادی (فعلاً پیشنهاد، نه وضعیت کد) لایه ۰ شامل cache، audit، persistence و eventing است و Event یکی از ارکان همان لایه است.

## ۵. وضعیت واقعی امروز (صادقانه)

- **دو پروژه از چهار پروژه خالی‌اند.** `Event.Domain` و `Event.Application` هیچ فایل `.cs` ندارند و `Event.Presentation` فقط یک متد ثبت خالی دارد. تمام ماژول در `Event.Infrastructure` است و قراردادهایش در `Core.Application` — نه در لایهٔ Application خودش.
- **پنج پردازشگر، یک انتخاب واقعی.** فقط `HybridOutboxProcessor` ثبت می‌شود؛ `PostgresOutboxProcessor` و `TriggerBasedOutboxProcessor` هیچ‌جا ساخته نمی‌شوند (کد مرده) و `OutboxProcessor` فقط با `ActivatorUtilities` توسط Hybrid نمونه‌سازی می‌شود.
- **در عمل همیشه polling اجرا می‌شود.** کلید `Database:Provider` در هیچ appsettingsی نیست، پس `switch` همیشه به `_ => CreatePollingProcessor()` می‌رسد؛ حتی `CreatePostgresProcessor` هم `PostgresOutboxProcessor` را استفاده نمی‌کند.
- **`SqlDependencyOutboxProcessor` برای اسکیمای Audit hardcode شده است:** `SELECT [Id] FROM [Audit].[OutboxMessages] WHERE [Status] = 0`. اگر روزی `Database:Provider = sqlserver` شود، پردازشگر همهٔ ماژول‌ها جدول Audit را می‌خواند؛ ضمناً به Service Broker و مجوز `ALTER DATABASE` نیاز دارد.
- **پیام‌های Failed برای همیشه می‌مانند.** `GetPendingMessagesAsync` فقط `Status == Pending` را می‌خواند و هیچ‌چیز `Failed` را به `Pending` برنمی‌گرداند؛ `RetryCount` فقط شمارنده است و یک خطای گذرا، رویداد را همیشه زمین می‌گذارد.
- **سه ماژول می‌نویسند ولی پردازشگر ندارند.** `Scheduler`، `Trader` و `HR.IrisaSync.Extention` از `IOutboxService<...>` استفاده می‌کنند ولی `AddOutboxProcessor` را صدا نمی‌زنند؛ رویدادهایشان هیچ‌وقت خوانده نمی‌شود. `WebScrapper` و `StreamBus` هم همان خط را کامنت کرده‌اند.
- **الگوی ثبت یک ضدالگوی DI است.** هر ماژول داخل `{Module}_AddInfrastructure` یک `services.BuildServiceProvider()` موقت می‌سازد تا `IOutboxProcessorRegistration` را بگیرد؛ نتیجه: کانتینر دوم با Singletonهای تکراری، و ترتیب بارگذاری به یک قرارداد نانوشته تبدیل می‌شود.
- **سه SaveChanges جدا به ازای هر پیام** و **بدون claim یا lease:** عملیات‌ها در تراکنش مشترک با انتشار نیستند و دو نمونهٔ هم‌زمان اپ می‌توانند یک پیام را دوبار منتشر کنند.
- **`IEventHandler<TEvent>` کاملاً کامنت شده است** (`Core.Application/Abstractions/Events/IEventHandler.cs`)؛ قرارداد واقعی `INotificationHandler<TEvent>` است که با `Core.Application/Common/Events/DomainEventHandler.cs` استاندارد شده.
- **`Event_AddHealthChecks` و `Event_UseInfrastructure` خالی‌اند:** هیچ چکی برای عقب‌افتادگی Outbox یا تعداد Failed وجود ندارد؛ اگر پردازشگر بمیرد از بیرون معلوم نمی‌شود.
