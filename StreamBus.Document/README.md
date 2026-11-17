# ماژول StreamBus (مصرف‌کنندهٔ استریم gRPC)

> **در یک جمله:** StreamBus یک پوستهٔ نازک دور استریم دوب‌طرفهٔ gRPC است: اتصال را با Retry زنده نگه می‌دارد، پیام‌های ورودی را به یک Background Worker می‌دهد و آن‌ها را به‌شکل `StreamMessageNotification<T>` داخل MediatR پخش می‌کند.

| | |
|---|---|
| **پروژه‌ها** | `StreamBus.Domain` / `StreamBus.Application` / `StreamBus.Infrastructure` (تمام منطق) / `StreamBus.Presentation` (بدون کد) — بدون پروژهٔ تست |
| **قرارداد عمومی** | `IStreamBusClient<TRequest,TResponse>` در `StreamBus.Application/Abstractions/IStreamBusClient.cs` — تنها ماژولی که قراردادش داخل خودش است، نه در Core |
| **وابستگی به ماژول‌های دیگر** | هیچ ماژول دامنه‌ای؛ فقط `Core.Application` و `Core.Infrastructure` (برای DI و لاگ) |
| **چه کسانی از آن استفاده می‌کنند** | امروز **هیچ‌کس**. فقط `Trader.WebApi` به سه پروژه‌اش رفرنس دارد ولی هیچ APIی از آن را صدا نمی‌زند |
| **جایگاه پیشنهادی در مدل لایه‌ای** | لایه ۰ (سرویس فنی) |

---

## ۱. این ماژول به چه درد می‌خورد؟ (به زبان ساده)

بخشی از سیستم‌ها به‌جای «درخواست/پاسخ»، یک اتصال دائمی باز می‌کنند و مدام داده می‌فرستند: قیمت لحظه‌ای، تیک معاملات، رویدادهای یک دستگاه. پروتکل طبیعی این کار gRPC duplex streaming است، ولی استفادهٔ مستقیم از آن سه درد دارد: مدیریت reconnect، تبدیل خطای شبکه به چیز قابل‌فهم، و تبدیل استریم به چیزی که بقیهٔ ماژول‌ها بتوانند مصرف کنند. StreamBus همین سه کار را می‌کند:

- **کلاینت خام** (`GrpcStreamBusClient`) با `AsyncDuplexStreamingCall` استریم را باز می‌کند و `ConnectAsync`، `SendAsync` و `ReceiveAllAsync` (به‌صورت `IAsyncEnumerable<TResponse>`) می‌دهد.
- **کلاینت مقاوم** (`ResilientStreamBusClient`) همان قرارداد را با Polly می‌پیچد: پنج تلاش با Exponential Backoff، و در `ReceiveAllAsync` حلقهٔ «قطع شد → دوباره وصل شو → ادامه بده».
- **مصرف‌کننده** (`StreamBusConsumerWorker`) یک `BackgroundService` است که با `AddStreamBusConsumer<TRequest,TResponse>` ثبت می‌شود: وصل می‌شود، پیام اولیه را می‌فرستد و برای هر پیام ورودی یک `StreamMessageNotification<TResponse>` می‌سازد و با `IMediator.Publish` پخش می‌کند.

ایدهٔ خوبش این است که لایهٔ Application هیچ‌وقت gRPC نمی‌بیند: هندلر تو فقط `INotificationHandler<StreamMessageNotification<Tick>>` است.

## ۲. چه چیزهایی داخلش هست؟

- **`IStreamBusClient<TRequest,TResponse>`** — قرارداد اصلی به‌همراه `IAsyncDisposable`؛ چهار عضو: `State`، `ConnectAsync(address, Method<,>)`، `SendAsync(message)` و `ReceiveAllAsync()`.
- **`GrpcStreamBusClient<,>`** — پیاده‌سازی خام روی `Grpc.Net.Client` با `MaxReceiveMessageSize = null` و `MaxSendMessageSize = null` (یعنی بی‌سقف).
- **`ResilientStreamBusClient<,>`** — پیاده‌سازی ثبت‌شده در DI؛ در سازنده یک `GrpcStreamBusClient` می‌سازد و `WaitAndRetryAsync(retryCount: 5)` را روی همهٔ خطاها اعمال می‌کند.
- **`StreamConsumerOptions<,>`** — `ServerAddress`، `GrpcMethod` و `InitialRequest`.
- **`StreamMessageNotification<TMessage>`** — یک `record` از نوع `INotification`؛ پل بین استریم و MediatR.
- **`StreamBusConsumerWorker<,>`** — Worker مصرف‌کننده، با یک Scope جدا برای هر پیام.
- **`AddStreamBusConsumer<,>(...)`** — ثبت options و Worker در DI.
- **`StreamConnectionState`** و **`StreamBusException`** — در `StreamBus.Domain` (تنها کد آن پروژه) و یک `ModuleInitializer` که فقط لاگ می‌کند.

## ۳. چه چیزی داخلش **نیست** (و نباید انتظارش را داشت)

- **هیچ قرارداد Protobufی در ریپو نیست.** هیچ فایل `.proto` و هیچ `Grpc.Tools`ی وجود ندارد؛ `Method<TRequest,TResponse>` را خود مصرف‌کننده باید بسازد.
- **سمت سرور را نمی‌سازد؛** فقط کلاینت است.
- **احراز هویت و Metadata ندارد.** `CallOptions` فقط `cancellationToken` می‌گیرد.
- **تضمین تحویل ندارد.** نه Ack، نه Offset، نه ذخیرهٔ پیام؛ بعد از هر reconnect پیام‌های فاصلهٔ قطعی می‌روند.
- **DbContext، Migration، Outbox، Controller و Endpoint ندارد** و ثبت Outbox هم کامنت شده است.
- **Health Check و متریک ندارد؛** `State` جایی گزارش نمی‌شود.
- **تست ندارد.** پروژهٔ `StreamBus.Test` وجود ندارد.
- **پردازش موازی ندارد.** پیام‌ها تک‌تک پردازش می‌شوند.

## ۴. یک نگاه فنی کوتاه

- `StreamBus_AddDomain` خالی است؛ `StreamBus_AddApplication` فقط `AddMediatR` روی اسمبلی خودش می‌زند؛ `StreamBus_AddInfrastructure` هم `IStreamBusClient<,>` را Transient روی `ResilientStreamBusClient<,>` ثبت می‌کند و `ModuleInitializer` را به‌عنوان HostedService. `StreamBus_AddPresentation` و `StreamBus_AddHealthChecks` خالی‌اند.
- `StreamBus_UseInfrastructure` یک `RunSmartMigrations` را صدا می‌زند که بدنه‌اش خالی است.
- `AddStreamBusConsumer<TRequest,TResponse>` در `StreamBus.Infrastructure/DependencyInjection/ServiceCollectionExtensions.cs` است، نه در Application؛ پس مصرف‌کننده برای ثبت ناچار است به Infrastructure رفرنس بدهد.
- Worker یک Scope برای عمر کلاینت و یک Scope برای هر پیام می‌گیرد؛ پس هندلرها می‌توانند `DbContext` و سرویس‌های Scoped تزریق کنند.
- در مدل لایه‌ای پیشنهادی (که فعلاً پیشنهاد است، نه واقعیت) لایه ۰ محل سرویس‌های فنی مثل همین StreamBus است: هیچ منطق کسب‌وکاری ندارد و فقط «حمل داده» می‌کند.

## ۵. وضعیت واقعی امروز (صادقانه)

- **صفر مصرف‌کنندهٔ واقعی دارد.** تنها رفرنس بیرونی، `Trader.WebApi.csproj` است که سه پروژهٔ StreamBus را رفرنس داده ولی هیچ‌جا `AddStreamBusConsumer` یا `IStreamBusClient` را صدا نمی‌زند. `AkSteel.WebApi` هم `StreamBus` را در `Modules:Enabled` دارد بدون رفرنس به اسمبلی‌هایش؛ نتیجه‌اش فقط چند پیام «نتونستم اسمبلی رو لود کنم» در لاگ است.
- **`GrpcStreamBusClient` ثبت نمی‌شود.** خط ثبتش کامنت است و `ResilientStreamBusClient` هم کلاینت خام را با `new` می‌سازد؛ پس نه می‌توانی پیاده‌سازی خام را جایگزین کنی، نه تستش کنی.
- **`StreamBus.Presentation` هیچ کدی ندارد.** تمام بدنه‌اش کامنت است، حتی `AddApplicationPart(typeof(StreamBusController).Assembly)` که `StreamBusController` اصلاً وجود ندارد.
- **`ModuleInitializer` کار مفیدی نمی‌کند:** فقط دو خط لاگ، و خط Seed کامنت است. `StreamBus_AddInfrastructure` هم `conn` را می‌خواند و استفاده نمی‌کند و ثبت Outbox را کامنت کرده است.
- **سیاست Retry خیلی پهن است.** `Policy.Handle<RpcException>().Or<Exception>()` هر خطایی را قابل‌تلاش‌مجدد می‌داند، حتی باگ برنامه؛ نتیجه پنج تلاش با تأخیر ۲، ۴، ۸، ۱۶ و ۳۲ ثانیه، حتی وقتی مشکل شبکه نیست.
- **`SendAsync` هم Retry می‌شود.** اگر نوشتن نیمه‌کاره شکست بخورد و دوباره تلاش شود، پیام می‌تواند دوبار برود؛ هیچ شناسهٔ پیام یا deduplicationی وجود ندارد.
- **`ReceiveAllAsync` بعد از قطعی پیام از دست می‌دهد.** حلقه enumerator را دور می‌ریزد، دوباره وصل می‌شود و از ابتدای استریم جدید می‌خواند؛ تحویل عملاً at-most-once است.
- **احراز هویت ممکن نیست.** `CallOptions` فقط `cancellationToken` دارد و راهی برای Bearer Token یا Header نیست؛ این ماژول فقط با endpoint بی‌احراز هویت کار می‌کند.
- **سقف حجم پیام برداشته شده است:** `MaxReceiveMessageSize = null` یعنی پیام غول‌آسا می‌تواند حافظه را پر کند.
- **Worker تک‌رشته‌ای است.** بین `await foreach` و `ProcessMessageAsync` موازی‌سازی نیست؛ یک هندلر کند کل استریم را نگه می‌دارد و خطای هندلر فقط لاگ و رد می‌شود.
- **دو مصرف‌کننده با یک جفت نوع نمی‌توانند کار کنند.** `StreamConsumerOptions<,>` به‌صورت Singleton ثبت می‌شود؛ دو بار `AddStreamBusConsumer<TickRequest,TickResponse>` یعنی هر دو Worker با آخرین options.
- **بدون `AppContext` switch یا تنظیم Handler:** برای آدرس `http://` (gRPC بدون TLS) هیچ تنظیمی وجود ندارد و عملاً فقط `https://` کار می‌کند.
