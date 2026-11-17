# ماژول Notification (اطلاع‌رسانی / ایمیل و Realtime)

> **در یک جمله:** Notification ماژول «واکنش» است؛ خودش هیچ کاری را شروع نمی‌کند، فقط گوش می‌دهد به رویدادهایی که ماژول‌های دیگر تولید می‌کنند و در پاسخ، یا ایمیل می‌فرستد یا یک پیام Realtime به کاربر می‌رساند.

| | |
|---|---|
| **پروژه‌ها** | `Notification.Domain` (خالی) / `Notification.Application` / `Notification.Infrastructure` / `Notification.Presentation` + `Notification.Test` |
| **قرارداد عمومی** | ندارد. نه `INotificationPublicService` وجود دارد و نه چیزی شبیه آن؛ نقطهٔ تعامل، رویداد MediatR است |
| **وابستگی به ماژول‌های دیگر** | `Identity.Shared` (`UserRegisteredEvent`)، Event (غیرمستقیم: پردازشگر Outbox رویداد را منتشر می‌کند)، Core و Cach |
| **چه کسانی از آن استفاده می‌کنند** | عملاً هیچ ماژولی مستقیم صدایش نمی‌زند؛ فقط `TraderServer.WebApi` آن را فعال می‌کند و `NotificationHub` را Map می‌کند |
| **جایگاه پیشنهادی در مدل لایه‌ای** | لایه ۱ (مصرف‌کنندهٔ رویداد، بدون API عمومی) |

---

## ۱. این ماژول به چه درد می‌خورد؟ (به زبان ساده)

بقیهٔ ماژول‌ها یک کارشان را انجام می‌دهند و تمام. Identity کاربر را ثبت می‌کند و به «بعد از ثبت کاربر چه شود؟» کاری ندارد. اگر قرار بود Identity خودش ایمیل خوش‌آمد بفرستد و پیام Realtime هم بفرستد، تا ابد به SMTP و SignalR وابسته می‌ماند.

Notification همین بار را برمی‌دارد. تنها چیزی که می‌داند این است: «اگر رویداد X منتشر شد، این کار را بکن». الان دقیقاً یک رویداد را می‌شناسد: `UserRegisteredEvent` که در `Identity.Shared/Events/UserRegisteredEvent.cs` تعریف شده. دو Handler مستقل برای همان یک رویداد دارد:

- `WelcomeEmailEventHandler` → یک ایمیل «خوش آمدی» می‌فرستد.
- `WelcomeInAppNotificationHandler` → یک پیام `NotificationMessage` می‌سازد و با SignalR به همان کاربر می‌فرستد.

نکتهٔ مهم این است که این دو Handler هیچ ربطی به هم ندارند؛ اگر فردا «پیامک» هم اضافه شود، فقط یک `INotificationHandler<UserRegisteredEvent>` سوم نوشته می‌شود و هیچ‌کس در Identity باخبر نمی‌شود.

## ۲. چه چیزهایی داخلش هست؟

- **`Notification.Application`** — قراردادهای خروجی: `IEmailSender` (فقط `SendEmailAsync(to, subject, body)`)، `IRealtimeNotifier` (`SendToUserAsync` و `BroadcastAsync`) و یک `IWebPushSender` که تعریف شده ولی هیچ پیاده‌سازی ندارد. مدل مشترک `NotificationMessage` با فیلدهای `UserId`، `Title`، `Body`، `IconUrl`، `LinkUrl`، `Channel` (`Email` / `WebPush` / `InApp`) و `Metadata`.
- **دو Handler** — هر دو از Polly استفاده می‌کنند: `DefaultRetry` را از `IReadOnlyPolicyRegistry<string>` می‌گیرند و ارسال را داخل آن اجرا می‌کنند.
- **`SmtpEmailSender`** — با `System.Net.Mail.SmtpClient` (نه MailKit) ایمیل می‌فرستد و دور آن سیاست `SmtpBreaker` (Circuit Breaker) پیچیده شده. تنظیمات از بخش `Smtp` در appsettings خوانده و روی `SmtpOptions` بایند می‌شود (`Host`، `Port`، `Username`، `Password`، `From`، `EnableSsl`).
- **`SmtpHealthCheck`** — با نام `SMTP` در Health Checks ثبت می‌شود. صادقانه بگویم: این Health Check یک ایمیل واقعی از `From` به `From` با موضوع `HealthCheck` می‌فرستد؛ یعنی هر بار صدا زدن `/health` یک ایمیل واقعی تولید می‌کند.
- **`SignalRRealtimeNotifier`** — روی `IHubContext<NotificationHub>` می‌نشیند و رویداد `notifications/new` را برای یک کاربر (`Clients.User(userId)`) یا همه (`Clients.All`) می‌فرستد.
- **`NotificationHub`** — یک `Hub` خالی با `[Authorize]`؛ هیچ متد سروری ندارد و فقط مقصد اتصال کلاینت است.
- **دو تست** — `WelcomeEmailEventHandlerTests` و `WelcomeInAppNotificationHandlerTests` که با Moq بررسی می‌کنند ارسال با ایمیل/آی‌دی درست انجام شده.

## ۳. چه چیزی داخلش **نیست** (و نباید انتظارش را داشت)

- **مودل دامنه ندارد.** `Notification.Domain` فقط یک `.csproj` است و صفر فایل `.cs`. هیچ موجودیتی مثل `NotificationLog` یا `NotificationTemplate` وجود ندارد.
- **دیتابیس ندارد.** نه DbContext، نه Migration، نه Outbox، نه اسکیما. پس هیچ سابقه‌ای از «چه پیامی، کِی، به کی فرستاده شد» نگه نمی‌دارد.
- **تضمین تحویل ندارد.** هیچ Retry پایدار، صف یا Dead-letter ندارد. اگر ایمیل شکست بخورد، فقط در لاگ می‌ماند (و در حالت Outbox، پیام دوباره پردازش می‌شود).
- **قالب/تمپلیت ندارد.** بدنهٔ ایمیل یک رشتهٔ ثابت فارسی داخل کد است.
- **تنظیمات کاربر ندارد.** کسی نمی‌تواند بگوید «ایمیل نگیر». `Metadata` و `Channel` تعریف شده‌اند ولی هیچ تنظیم ترجیحی خوانده نمی‌شود.
- **`IWebPushSender` پیاده‌سازی و ثبت ندارد.** یعنی Web Push امروز یک placeholder است.

## ۴. یک نگاه فنی کوتاه

- ثبت در DI با کنوانسیون پلتفرم: `Notification_AddApplication` (فقط MediatR)، `Notification_AddInfrastructure` (`IEmailSender` → `SmtpEmailSender` به‌صورت Scoped + `Configure<SmtpOptions>`)، `Notification_AddPresentation` (`AddSignalR` + `IRealtimeNotifier`).
- `Notification_UseInfrastructure` **هیچ کاری نمی‌کند** و فقط `app` را برمی‌گرداند؛ چون ماژول Migration ندارد این طبیعی است.
- `UseNotificationPresentation` هم خالی است. اتصال Hub بیرون از ماژول انجام می‌شود: تنها جایی که `MapHub<NotificationHub>("/hubs/notifications")` فعال است، `TraderServer.WebApi/Program.cs` است (در سه WebApi دیگر کامنت شده).
- ماژول فقط در `TraderServer.WebApi/appsettings.json` فعال است (`Modules:Enabled` با `Order: 77`). در بقیهٔ هاست‌ها اصلاً بارگذاری نمی‌شود.
- مسیر عملی یک رویداد: موجودیت `AddDomainEvent` می‌زند → `EfUnitOfWork` رویدادها را جمع می‌کند → `OutboxMessage` در دیتابیس همان ماژول نوشته می‌شود → `OutboxProcessor` (ماژول Event) پیام را برمی‌دارد و نوع را از `AssemblyQualifiedName` (یا در نبود آن، اسکن `TypeName` در اسمبلی‌های لودشده) پیدا می‌کند → `MediatorEventBus.PublishAsync` → `IMediator.Publish` → Handlerهای Notification.

## ۵. وضعیت واقعی امروز (صادقانه)

- **بخش `Smtp` در هیچ appsettings‌ای وجود ندارد.** با یک جست‌وجوی ساده در همهٔ فایل‌های `appsettings*.json` مخزن، هیچ کلید `Smtp` پیدا نمی‌شود. نتیجه: `SmtpOptions` با مقادیر پیش‌فرض (`Host=""`، `Port=0`، `From=""`) بایند می‌شود و `SmtpEmailSender` در عمل شکست می‌خورد. یعنی «ارسال ایمیل» امروز کد نوشته‌شده دارد ولی پیکربندی‌شده نیست.
- **هیچ‌جای Notification از `ModuleHelper.IsActive` استفاده نمی‌کند** (برخلاف Audit و Scheduler که Seed خود را با آن گارد می‌کنند)؛ پس فعال/غیرفعال بودنش فقط به `Modules:Enabled` همان هاست بستگی دارد.
- **هیچ Seed منو/منبع/مجوزی ندارد** و `ModuleInitializer` هم ندارد؛ در نتیجه در پنل هیچ صفحه‌ای برای Notification ساخته نمی‌شود (که با ماهیت «بدون UI» آن سازگار است).
- **دو رفرنس مرده در `Notification.Application.csproj`:** به `Notification.Domain` (که خالی است) و به `Sample.Shared` (که هیچ‌جا استفاده نمی‌شود). هر دو فقط زمان Build را طولانی می‌کنند.
- **Handlerها از `DomainEventHandler<T>` ارث نمی‌برند**، بلکه مستقیم `INotificationHandler<T>` هستند. یعنی لاگ و مدیریت خطای استاندارد پایه را ندارند و هر کدام باید خودشان Polly را دستی بگیرند.
- **Health Check خطرناک است:** چون واقعاً `SendMailAsync` می‌زند، در محیطی که SMTP تنظیم نشده باشد `Unhealthy` می‌شود و اگر تنظیم شده باشد، هر فراخوانی `/health` یک ایمیل واقعی مصرف می‌کند.
- **`IRealtimeNotifier` به‌صورت Scoped ثبت شده ولی `IHubContext` Singleton است** — این ترکیب کار می‌کند ولی در Background Jobها اگر از Scope درست استفاده نشود، ممکن است به مشکل بخوری.
- جمع‌بندی: Notification از نظر «شکل معماری» الگوی درستی است (واکنش به رویداد، بدون دامنه و بدون دیتابیس)، ولی از نظر «کارکرد واقعی» امروز در حد یک اسکلت پیکربندی‌نشده است.
