# استفاده از ماژول Scheduler در ماژول‌های دیگر

این فایل فقط یک سؤال را جواب می‌دهد: **اگر ماژول من کاری دارد که باید در آینده یا به‌صورت تکرارشونده اجرا شود، چطور آن را به Scheduler بدهم؟**

## ۱. خلاصهٔ در یک نگاه

| کاری که می‌خواهی | از چه چیزی استفاده کن |
|---|---|
| اجرا در یک لحظهٔ مشخص | `ISchedulerService.ScheduleAsync<TPayload>(...)` |
| اجرای تکرارشونده (روزانه/هفتگی/ماهانه/کران) | `ISchedulerService.ScheduleRecurringAsync<TPayload>(...)` |
| لغو یک Job یک‌باره | `ISchedulerService.CancelAsync(jobId)` |
| لغو/Pause/Resume یک تکرار | `CancelRecurringAsync` / `PauseRecurringAsync` / `ResumeRecurringAsync` |
| دیدن وضعیت | `GetAsync` / `QueryAsync` / `GetRecurringAsync` / `QueryRecurringAsync` |
| اجرای واقعی کار | یک کلاس `IScheduledJobHandler<TPayload>` بنویس و با `AddScheduledJobHandler<TPayload, THandler>()` ثبت کن |

## ۲. قرارداد عمومی

```csharp
// مسیر: Scheduler.Application/Abstractions/ISchedulerService.cs
public interface ISchedulerService
{
    Task<Guid> ScheduleAsync<TPayload>(
        TPayload payload, DateTimeOffset fireAt, string? queue = null,
        TimeSpan? preFireBuffer = null, CancellationToken ct = default)
        where TPayload : IScheduledJobPayload;

    Task<bool> CancelAsync(Guid jobId, CancellationToken ct = default);
    Task<ScheduledJobInfo?> GetAsync(Guid jobId, CancellationToken ct = default);
    Task<IReadOnlyList<ScheduledJobInfo>> QueryAsync(SchedulerQuery query, CancellationToken ct = default);

    Task<Guid> ScheduleRecurringAsync<TPayload>(
        string name, TPayload payload, RecurrencePattern pattern,
        DateTimeOffset? startAt = null, DateTimeOffset? endAt = null,
        int? maxOccurrences = null, int? maxFailures = 10,
        TimeZoneInfo? timeZone = null, string? queue = null,
        CancellationToken ct = default)
        where TPayload : IScheduledJobPayload;

    Task<RecurringScheduleInfo?> GetRecurringAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<RecurringScheduleInfo>> QueryRecurringAsync(RecurringSchedulerQuery query, CancellationToken ct = default);
    Task<bool> PauseRecurringAsync(Guid id, CancellationToken ct = default);
    Task<bool> ResumeRecurringAsync(Guid id, CancellationToken ct = default);
    Task<bool> CancelRecurringAsync(Guid id, CancellationToken ct = default);
}
```

سه قطعهٔ دیگر که مصرف‌کننده لازم دارد:

```csharp
public interface IScheduledJobPayload { }                       // marker

public interface IScheduledJobHandler<in TPayload>              // کاری که اجرا می‌شود
    where TPayload : IScheduledJobPayload
{
    Task ExecuteAsync(TPayload payload, CancellationToken ct);
}

[ScheduledJob("my-module.my-job")]                              // کلید یکتای payload
```

## ۳. مسیر توصیه‌شده (نمونهٔ واقعی، مطابق الگوی Trader)

### گام ۱ — رفرنس بده

در `.csproj` ماژول خودت:

```xml
<ProjectReference Include="..\Scheduler.Application\Scheduler.Application.csproj" />
```

اگر به `RecurrencePattern`، `ScheduledJobState` یا `RecurringScheduleState` نیاز داری، `Scheduler.Domain` را هم رفرنس بده. به `Scheduler.Infrastructure` **رفرنس نده**.

### گام ۲ — payload بساز (در لایهٔ Application خودت)

```csharp
// مسیر: MyModule.Application/Scheduler/SendDailyDigestPayload.cs
using Scheduler.Application.Abstractions;
using Scheduler.Application.Attributes;

[ScheduledJob(Key)]
public sealed record SendDailyDigestPayload(
    [property: System.Text.Json.Serialization.JsonPropertyName("digestId")] Guid DigestId
) : IScheduledJobPayload
{
    public const string Key = "mymodule.daily-digest";
}
```

> کلید باید در کل برنامه یکتا باشد. `JobTypeRegistry` اگر دو Type با یک کلید ببیند، `InvalidOperationException` می‌دهد.

### گام ۳ — Handler بنویس (در لایهٔ Infrastructure خودت)

```csharp
// مسیر: MyModule.Infrastructure/Scheduler/SendDailyDigestHandler.cs
public sealed class SendDailyDigestHandler : IScheduledJobHandler<SendDailyDigestPayload>
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<SendDailyDigestHandler> _logger;

    public SendDailyDigestHandler(IServiceScopeFactory scopeFactory,
        ILogger<SendDailyDigestHandler> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    public async Task ExecuteAsync(SendDailyDigestPayload payload, CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var uow = scope.ServiceProvider.GetRequiredService<IMyModuleUnitOfWork<MyModuleDbContext>>();
        var job = await uow.DigestJobs.GetByIdAsync(payload.DigestId);

        // ⚠️ Idempotency: دیسپچر ممکن است بعد از یک خطای موقت دوباره اجرا کند
        if (job is null || job.State != DigestJobState.Running) return;

        try
        {
            await scope.ServiceProvider.GetRequiredService<IDigestService>()
                       .BuildAndSendAsync(job.Id, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Digest {Id} failed", job.Id);
            throw;   // → Scheduler رکورد را Failed می‌کند و در تکرارشونده، MaxFailures را می‌شمارد
        }
    }
}
```

### گام ۴ — در DI ثبت کن (در `MyModule_AddInfrastructure`)

```csharp
services.AddScheduledJobHandler<SendDailyDigestPayload, SendDailyDigestHandler>();
```

این متد **دو** کار می‌کند: `IScheduledJobHandler<TPayload>` را Scoped ثبت می‌کند و یک `ScheduledJobPayloadRegistration` (Singleton) می‌گذارد تا `JobTypeRegistry` آن را ببیند. اگر این خط را نگذاری، ممکن است Registry نوع را از اسکن اسمبلی پیدا کند (اگر اسمبلی تا آن لحظه لود شده باشد) — ولی به آن تکیه نکن.

### گام ۵ — زمان‌بندی کن

```csharp
// یک‌باره
var jobId = await _scheduler.ScheduleAsync(
    payload: new SendDailyDigestPayload(digestId),
    fireAt: DateTimeOffset.UtcNow.AddMinutes(5),
    queue: "scheduler");

// تکرارشونده: هر روز ۹ صبح به وقت تهران
var pattern = RecurrencePattern.Daily(interval: 1);
var recurringId = await _scheduler.ScheduleRecurringAsync(
    name: $"digest-{digestId}",                 // باید یکتا باشد
    payload: new SendDailyDigestPayload(digestId),
    pattern: pattern,
    startAt: DateTimeOffset.UtcNow,
    maxFailures: 10,
    timeZone: TimeZoneInfo.FindSystemTimeZoneById("Asia/Tehran"));
```

ساخت الگوهای دیگر:

```csharp
RecurrencePattern.Minutely(5);                       // هر ۵ دقیقه
RecurrencePattern.Hourly(2);                         // هر ۲ ساعت
RecurrencePattern.Weekly(DayOfWeek.Saturday, DayOfWeek.Monday);
RecurrencePattern.Monthly(1, 1, 15);                 // روز ۱ و ۱۵ هر ماه
RecurrencePattern.Monthly(1, -1);                    // آخرین روز ماه
RecurrencePattern.Cron("0 9 * * 1-5");               // روزهای کاری ۹:۰۰
```

### گام ۶ — رکورد دامنهٔ خودت را آپدیت نگه دار

الگوی Trader این است: پیش از هر اجرا `MarkRunning...` و بعد از `Cancel` هم `Stopped`. Scheduler خودش هم `ScheduledJob`/`RecurringSchedule` را نگه می‌دارد، ولی **وضعیت کسب‌وکار تو** (مثلاً «این جمع‌آوری قیمت متوقف شد») جای دیگری است و باید خودت همگام کنی.

## ۴. چه چیزی عمومی **نیست**

- `HangfireJobDispatcher`، `JobTypeRegistry`، `PreciseDelay`، `SchedulerJson`، `RecurringScheduleRescheduler/Recovery`، `SchedulerUnitOfWork` → زیرساخت داخلی.
- `ISchedulerUnitOfWork<DbContext>` و `IRepository<SchedulerDbContext, ScheduledJob, Guid>` → نباید از بیرون استفاده شوند.
- `HangfireSchedulerService` → `internal`.
- `SchedulerDbContext` → برای Migration و Debug، نه برای کوئری ماژول‌های دیگر. اگر می‌خواهی Jobها را ببینی، `ISchedulerService.QueryAsync` بده.

## ۵. قواعد

- **`queue: "scheduler"` را همیشه بده.** سرور Hangfire فقط صف `scheduler` (از `SchedulerOptions.DefaultQueue`) را گوش می‌دهد؛ Job در صف دیگر هرگز اجرا نمی‌شود.
- **Payload را کوچک و JSON-friendly نگه دار.** فقط شناسه‌ها؛ داده را داخل Handler از دیتابیس خودت بخوان. (سریالایز با camelCase و `WhenWritingNull` انجام می‌شود.)
- **Handler را Idempotent بنویس.** دیسپچر از اجرای مجدد یک Job `Succeeded`/`Cancelled` جلوگیری می‌کند، ولی اجرای نیمه‌تمام (کرش وسط کار) دوباره اجرا می‌شود.
- **داخل Handler از `IServiceScopeFactory` استفاده کن، نه تزریق مستقیم DbContext.** دیسپچر خودش Scope می‌سازد، ولی Handler طولانی‌عمر است و DbContext مشترک خطرناک است. (نمونهٔ `CollectPriceJobHandler` دقیقاً همین کار را می‌کند.)
- **`name` تکرارشونده را یکتا و قابل‌حدس بگذار** (`price-collect-{jobId}`). تکرار نام = استثنا.
- **`maxFailures` را جدی بگیر.** وقتی `FailureCount >= MaxFailures` برسد، `State = Paused` می‌شود و رخداد بعدی ساخته نمی‌شود؛ باید دستی `Resume` کنی.
- **به دقیق‌بودن `FireAt` در حد ثانیه تکیه نکن؛** دقت میلی‌ثانیه‌ای با `PreFireBuffer` + Busy-Wait به دست می‌آید و هزینهٔ CPU دارد. اگر دقت میلی‌ثانیه لازم نداری، `preFireBuffer` بزرگ‌تر بده تا Spin کمتر شود.

## ۶. چک‌لیست وقتی می‌خواهی یک Job جدید اضافه کنی

1. `Scheduler` در `Modules:Enabled` هاست هست؟ (الان فقط `Trader.WebApi`.)
2. payload `IScheduledJobPayload` است و `[ScheduledJob("key-یکتا")]` دارد؟
3. `AddScheduledJobHandler<TPayload, THandler>()` را در `{Module}_AddInfrastructure` صدا زدی؟
4. `THandler` در همان اسمبلی است که Registry اسکن می‌کند؟ (Type باید `public` باشد — Registry فقط `IsPublic`/`IsNestedPublic` را می‌پذیرد.)
5. `queue` را `"scheduler"` گذاشتی؟
6. Handler را با حالت «دو بار اجرا» تست کردی (Idempotency)؟
7. رکورد دامنهٔ خودت را با Scheduler همگام می‌کنی (شروع/پایان/لغو)؟
8. تست: `Scheduler.Tests` را نگاه کن (فقط `RecurrencePattern` پوشش دارد) و برای مسیر خودت تست جدا بنویس.
