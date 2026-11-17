# استفاده از ماژول Notification در ماژول‌های دیگر

این فایل فقط یک سؤال را جواب می‌دهد: **اگر ماژول من کاری کرد که باید به کاربر خبر داده شود، چطور این خبر را به Notification بدهم؟**

## ۱. خلاصهٔ در یک نگاه

| کاری که می‌خواهی | از چه چیزی استفاده کن |
|---|---|
| «بعد از این اتفاق، به کاربر خبر بده» | یک `IDomainEvent` منتشر کن و در `Notification.Application` یک `INotificationHandler<TEvent>` بنویس |
| فرستادن ایمیل مستقیم از ماژول خودت | `IEmailSender.SendEmailAsync(to, subject, body)` از `Notification.Application.Interfaces` |
| فرستادن پیام Realtime به یک کاربر | `IRealtimeNotifier.SendToUserAsync(userId, message, ct)` |
| پخش پیام برای همه | `IRealtimeNotifier.BroadcastAsync(message, ct)` |
| «چرا هیچ ایمیلی نمی‌آید؟» | بخش `Smtp` را در appsettings هاست تنظیم کن (امروز در هیچ appsettings‌ای نیست) |

## ۲. قرارداد عمومی (که در واقع قرارداد نیست)

Notification **هیچ سرویس عمومی ندارد**. تنها چیزهایی که از بیرون قابل استفاده‌اند، این‌ها هستند:

```csharp
// مسیر: Notification.Application/Interfaces/IEmailSender.cs
public interface IEmailSender
{
    Task SendEmailAsync(string to, string subject, string body);
}

// مسیر: Notification.Application/Interfaces/IRealtimeNotifier.cs
public interface IRealtimeNotifier
{
    Task SendToUserAsync(string userId, NotificationMessage message, CancellationToken ct = default);
    Task BroadcastAsync(NotificationMessage message, CancellationToken ct = default);
}

// مسیر: Notification.Application/Models/NotificationMessage.cs
public class NotificationMessage
{
    public string UserId { get; set; }
    public string Title { get; set; }
    public string Body { get; set; }
    public string? IconUrl { get; set; }
    public string? LinkUrl { get; set; }
    public NotificationChannel Channel { get; set; } = NotificationChannel.InApp;
    public IDictionary<string, string>? Metadata { get; set; }
}
```

> ⚠️ **نکتهٔ ساختاری:** `IEmailSender` و `IRealtimeNotifier` در لایهٔ Application خودِ Notification هستند، نه در `Core.Application`. پس هر ماژولی که مستقیم از آن‌ها استفاده کند، به `Notification.Application` رفرنس می‌دهد — یعنی به یک ماژول «لایه ۱» که هیچ‌کس نباید به آن وابسته باشد. مسیر درست، رویداد است.

## ۳. مسیر توصیه‌شده: رویداد بده، نه وابستگی

### گام ۱ — یک رویداد دامنه بساز

رویداد باید `IDomainEvent` باشد (که خودش `INotification` است) و جایی باشد که ماژول‌های دیگر بتوانند ببینندش:

```csharp
// مسیر پیشنهادی: {Module}.Shared/Events/EmploymentSuspendedEvent.cs
public class EmploymentSuspendedEvent : IDomainEvent
{
    public Guid EmploymentId { get; }
    public string EmploymentCode { get; }
    public string Reason { get; }
    public DateTime OccurredOn { get; }

    public EmploymentSuspendedEvent(Guid employmentId, string employmentCode, string reason)
    {
        EmploymentId = employmentId;
        EmploymentCode = employmentCode;
        Reason = reason;
        OccurredOn = DateTime.UtcNow;
    }
}
```

### گام ۲ — موجودیت آن را در رفتار دامنه‌ای Raise کند

```csharp
public void Suspend(string reason)
{
    State = EmploymentState.Suspended;
    AddDomainEvent(new EmploymentSuspendedEvent(Id, EmploymentCode, reason));
}
```

از این نقطه به بعد، `EfUnitOfWork` رویداد را جمع می‌کند و در `OutboxMessages` اسکیمای همان ماژول می‌نویسد؛ `OutboxProcessor` ماژول Event آن را برمی‌دارد و با MediatR منتشر می‌کند. **هیچ کد اضافه‌ای در ماژول تو لازم نیست.**

### گام ۳ — در Notification یک Handler بنویس

```csharp
// مسیر: Notification.Application/EventHandlers/EmploymentSuspendedNotificationHandler.cs
public class EmploymentSuspendedNotificationHandler
    : INotificationHandler<EmploymentSuspendedEvent>
{
    private readonly IEmailSender _emailSender;
    private readonly IRealtimeNotifier _notifier;
    private readonly IReadOnlyPolicyRegistry<string> _policies;
    private readonly ILogger<EmploymentSuspendedNotificationHandler> _logger;

    public EmploymentSuspendedNotificationHandler(
        IEmailSender emailSender,
        IRealtimeNotifier notifier,
        IReadOnlyPolicyRegistry<string> policies,
        ILogger<EmploymentSuspendedNotificationHandler> logger)
    {
        _emailSender = emailSender;
        _notifier = notifier;
        _policies = policies;
        _logger = logger;
    }

    public async Task Handle(EmploymentSuspendedEvent e, CancellationToken ct)
    {
        var policy = _policies.Get<IAsyncPolicy>("DefaultRetry");

        await policy.ExecuteAsync(async token =>
        {
            var message = new NotificationMessage
            {
                UserId = e.EmploymentId.ToString(),   // ⚠️ این‌جا باید UserId واقعی باشد
                Title = "تعلیق نیرو",
                Body = $"نیرو با کد {e.EmploymentCode} تعلیق شد. دلیل: {e.Reason}",
                Channel = NotificationChannel.InApp
            };

            await _notifier.SendToUserAsync(message.UserId, message, token);
            _logger.LogInformation("Suspension notice sent for {Code}", e.EmploymentCode);
        }, ct);
    }
}
```

**ثبت در DI لازم نیست.** `Notification_AddApplication` همهٔ `INotificationHandler`های اسمبلی `Notification.Application` را با `RegisterServicesFromAssembly` پیدا می‌کند.

### گام ۴ — هاست را تنظیم کن

سه پیش‌نیاز که اگر یکی نباشد هیچ اتفاقی نمی‌افتد:

1. `Modules:Enabled` شامل `Notification` باشد (الان فقط `TraderServer.WebApi` این‌طور است).
2. ماژول `Event` (پردازشگر Outbox) هم فعال باشد، وگرنه پیام در جدول `OutboxMessages` می‌ماند.
3. برای ایمیل: بخش `Smtp` در appsettings. برای Realtime: `app.MapHub<NotificationHub>("/hubs/notifications")` — که در TraderServer.WebApi انجام شده.

## ۴. قواعد

- **Notification را «صدا» نزن، به آن «خبر بده».** اگر مستقیماً `IEmailSender` را تزریق می‌کنی، داری ماژول خودت را به SMTP گره می‌زنی. رویداد بده.
- **Handler را Idempotent بنویس.** Outbox می‌تواند پیام را دوباره بدهد؛ دو بار ایمیل خوش‌آمد فرستادن یک باگ است. یک `Metadata["eventId"]` بگذار و اگر جایی ثبت می‌کنی، تکرار را رد کن.
- **داخل Handler خطا را قورت نده، ولی همیشه throw هم نکن.** اگر throw کنی، Outbox پیام را Failed می‌کند و دوباره امتحان می‌کند؛ برای خطای موقت خوب است، برای خطای دائمی (آدرس ایمیل بد) فقط لاگ و برگشت بهتر است.
- **بدنهٔ پیام را از دامنه جدا نگه دار.** Notification نباید `Employment` را کوئری کند؛ هر چه لازم دارد باید داخل رویداد باشد.
- **بدون Notification هم باید کار کند.** اگر ماژولی به Notification وابسته باشد، در هاست‌هایی که Notification فعال نیست (مثل `AkSteel.WebApi`) آن ماژول می‌شکند. پس هیچ کدی نباید فرض کند Notification هست.
- **از `IWebPushSender` استفاده نکن.** پیاده‌سازی و ثبت ندارد.

## ۵. چک‌لیست وقتی می‌خواهی یک نوتیفیکیشن جدید اضافه کنی

1. آیا واقعاً «اطلاع‌رسانی» است، یا می‌توان آن را در همان ماژول با `ILogger` نوشت؟ اگر خبر به کاربر انسانی می‌رسد، Notification درست است.
2. رویداد را در `{Module}.Shared` گذاشتی یا فقط داخل دامنهٔ خودت است؟ اگر ماژول دیگری باید بشنود، Shared.
3. هندلر را با `grep` پیدا کن: `grep -r "INotificationHandler" Notification.Application`.
4. تست: مسیر موفق + خطای SMTP. نمونهٔ موجود: `Notification.Test/WelcomeEmailEventHandlerTests.cs`.
5. تصمیم بگیر: اگر ارسال خبر «حیاتی» است (مثل رمز موقت)، Notification امروز ضمانت تحویل ندارد؛ آن را در ماژول اصلی و به‌صورت تراکنشی انجام بده.
