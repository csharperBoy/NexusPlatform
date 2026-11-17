# استفاده از ماژول Event در ماژول‌های دیگر

این فایل فقط یک سؤال را جواب می‌دهد: **اگر من می‌خواهم رویداد منتشر کنم یا به رویداد کسی واکنش بدهم، چطور این کار را بکنم که به Event گره نخورم؟**

## ۱. خلاصهٔ در یک نگاه

| کاری که می‌خواهی | از چه چیزی استفاده کن |
|---|---|
| رویداد از موجودیت خودم تولید کنم | `AddDomainEvent(...)` روی `BaseEntity` در لایهٔ Domain |
| رویدادهای دامنه‌ام واقعاً منتشر شوند | یک‌بار `registration.AddOutboxProcessor<MyDbContext>(services)` در `{Module}_AddInfrastructure` |
| به رویداد ماژول دیگر واکنش بدهم | `INotificationHandler<TEvent>` — یا راحت‌تر، ارث‌بری از `DomainEventHandler<TEvent>` |
| مستقیم و بدون Outbox منتشر کنم | `IEventBus.PublishAsync(...)` — امروز هیچ ماژولی این کار را نمی‌کند و توصیه هم نمی‌شود |
| پردازشگر خودم را بنویسم | **نکن.** یک پردازشگر برای هر DbContext کافی است |

## ۲. قرارداد عمومی

```csharp
// مسیر: Core.Application/Abstractions/Events/IOutboxProcessorRegistration.cs
public interface IOutboxProcessorRegistration
{
    IServiceCollection AddOutboxProcessor<TDbContext>(IServiceCollection services)
        where TDbContext : DbContext;
}

// مسیر: Core.Application/Abstractions/Events/IEventBus.cs
public interface IEventBus
{
    Task PublishAsync<TEvent>(TEvent @event) where TEvent : INotification;
    Task PublishAsync<TEvent>(params TEvent[] events) where TEvent : INotification;
}

// مسیر: Core.Application/Abstractions/Events/IOutboxService.cs
public interface IOutboxService<TDbContext> where TDbContext : DbContext
{
    Task AddEventsAsync(IEnumerable<IDomainEvent> domainEvents);
    Task<IEnumerable<OutboxMessage>> GetPendingMessagesAsync(int batchSize = 100);
    Task MarkAsProcessingAsync(Guid messageId);
    Task MarkAsCompletedAsync(Guid messageId);
    Task MarkAsFailedAsync(Guid messageId, Exception ex);
    Task CleanupProcessedMessagesAsync(DateTime olderThan);
}
```

موجودیت `OutboxMessage` در `Core.Domain/Common/OutboxMessage.cs` است و نگاشتش در `Core.Infrastructure/Database/Configurations/OutboxMessageConfiguration.cs`.

> ⚠️ **نکتهٔ ساختاری:** هر سه اینترفیس در `Core.Application` تعریف شده‌اند، نه در `Event.Application`. پس مصرف‌کننده به Core وابسته می‌شود، نه به Event. `Event.Application` و `Event.Domain` هم هیچ کدی ندارند.

## ۳. مسیر توصیه‌شده

**گام ۱ — جدول Outbox را در DbContext خودت داشته باش.** اگر `OutboxMessages` را نداری، اول این را اضافه کن (وگرنه هیچ رویدادی ذخیره نمی‌شود):

```csharp
// People.Infrastructure/Data/PeopleDbContext.cs
public DbSet<OutboxMessage> OutboxMessages { get; set; } = null!;

protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    base.OnModelCreating(modelBuilder);
    modelBuilder.ApplyConfiguration(new OutboxMessageConfiguration("people"));
}
```

**گام ۲ — یک‌بار پردازشگر را برای DbContext خودت ثبت کن:**

```csharp
// People.Infrastructure/DependencyInjection/ServiceCollectionExtensions.cs
using Core.Application.Abstractions.Events;
using Microsoft.Extensions.DependencyInjection;

public static IServiceCollection People_AddInfrastructure(
    this IServiceCollection services, IConfiguration configuration)
{
    services.AddDbContext<PeopleDbContext>(/* ... */);
    services.AddScoped<IUnitOfWork<PeopleDbContext>, EfUnitOfWork<PeopleDbContext>>();

    // ↓ همان کاری که Audit/Contact/HR/People/Identity و ... می‌کنند
    var registration = services.BuildServiceProvider()
                               .GetRequiredService<IOutboxProcessorRegistration>();

    registration.AddOutboxProcessor<PeopleDbContext>(services);

    return services;
}
```

**گام ۳ — رویداد را از موجودیت تولید کن (لایهٔ Domain، بدون هیچ using از Event):**

```csharp
// People.Domain/Entities/NaturalPerson.cs
public void SetFullName(string firstName, string lastName)
{
    FullName = FullName.Create(firstName, lastName);
    AddDomainEvent(new ChangeNaturalPersonEvent(Id));   // از BaseEntity
    Touch();
}
```

**گام ۴ — هندلر را در Application بنویس (این‌جا هم فقط Core):**

```csharp
// People.Application/EventHandlers/ChangeNaturalPersonCacheInvalidator.cs
using Core.Application.Abstractions.Caching.PublicService;
using Core.Application.Common.Events;      // DomainEventHandler<TEvent>
using Microsoft.Extensions.Logging;
using People.Domain.Events;

public sealed class ChangeNaturalPersonCacheInvalidator
    : DomainEventHandler<ChangeNaturalPersonEvent>
{
    private readonly ICachePublicService _cache;

    public ChangeNaturalPersonCacheInvalidator(
        ICachePublicService cache,
        ILogger<DomainEventHandler<ChangeNaturalPersonEvent>> logger) : base(logger)
        => _cache = cache;

    protected override async Task HandleEventAsync(
        ChangeNaturalPersonEvent @event, CancellationToken cancellationToken)
    {
        await _cache.RemoveByPatternAsync($"people:person:{@event.Id}:*");
    }
}
```

هندلر را دستی ثبت نکن؛ `Core_AddInfrastructure` در `Core.Infrastructure/DependencyInjection/ServiceCollectionExtensions.cs` با `cfg.RegisterServicesFromAssemblies(AppDomain.CurrentDomain.GetAssemblies())` همهٔ هندلرهای اسمبلی‌های لودشده را ثبت می‌کند.

## ۴. قواعد

- **`Event.Infrastructure` را رفرنس نده.** `HybridOutboxProcessor`، `SqlDependencyOutboxProcessor` و بقیهٔ پردازشگرها جزو API عمومی نیستند. تنها چیزی که لازم داری `IOutboxProcessorRegistration` است.
- **پردازشگر خودت را نساز.** اگر دو پردازشگر برای یک DbContext ثبت شود، هر پیام دوبار منتشر می‌شود. `AddOutboxProcessor<TDbContext>` دقیقاً یک‌بار صدا زده شود.
- **ترتیب بارگذاری را رعایت کن.** ماژول تو `IOutboxProcessorRegistration` را داخل `{Module}_AddInfrastructure` Resolve می‌کند؛ پس `Event` باید در `Modules:Enabled` با `Order` کوچک‌تر از ماژول تو باشد. اگر نباشد، همان خط شروع اپ با `InvalidOperationException` می‌خورد.
- **فوراً منتشر نمی‌شود.** بین `SaveChangesAsync` و اجرای هندلر حداقل یک دور polling (امروز ۱۰ تا ۶۰ ثانیه) فاصله است. اگر جواب Immediate لازم داری، Outbox ابزار درستی نیست.
- **نام و شکل رویداد را عوض نکن.** در جدول Outbox، `AssemblyQualifiedName` ذخیره می‌شود؛ تغییر namespace یا نام کلاس، پیام‌های Pending قبلی را غیرقابل بازسازی می‌کند.
- **رویداد را JSON-friendly نگه دار.** فقط پراپرتی‌های ساده؛ نه Navigation، نه `DbContext`، نه delegate. رویدادهایی مثل `ChangeNaturalPersonEvent(Guid Id)` که ctorشان با نام پراپرتی می‌خورد درست کار می‌کنند.
- **هندلر باید idempotent و کم‌خطا باشد.** ممکن است یک پیام دوبار منتشر شود (مرگ پروسه یا اجرای چند نمونهٔ هم‌زمان) و پیام Failed هیچ‌وقت دوباره تلاش نمی‌شود؛ پس خطا را خودت مدیریت کن و عملیات را تکرارشدنی بنویس.
- **هندلر را سنگین نکن.** پردازشگر پیام‌ها را تک‌تک و پشت‌سرهم منتشر می‌کند؛ یک هندلر کند، صف را عقب می‌اندازد.
- **در Domain و Application از `IOutboxService` استفاده نکن.** نوشتن در Outbox کار `EfUnitOfWork` است؛ خواندن و تغییر وضعیت کار پردازش‌گر.

## ۵. چک‌لیست وقتی می‌خواهی یک رویداد جدید اضافه کنی

1. آیا این رویداد واقعاً «دامنه‌ای» است؟ اگر فقط یک فراخوانی داخلی است، همان سرویس را صدا بزن.
2. رویداد را در `{Module}.Domain/Events/` ساختی و از `IDomainEvent` ارث برده؟
3. `AddDomainEvent(...)` را در متدی گذاشتی که واقعاً وضعیت را تغییر می‌دهد (نه در سازنده، نه در Query)?
4. DbContext ماژول، `OutboxMessages` و `OutboxMessageConfiguration("{schema}")` را دارد و Migrationش اجرا شده؟
5. `AddOutboxProcessor<{Module}DbContext>` یک‌بار در `{Module}_AddInfrastructure` هست؟
6. `Event` در `Modules:Enabled` قبل از ماژول توست و هندلر در اسمبلی Application ماژول تو؟
7. تست: مسیر موفق + مسیر خطای هندلر (نباید روی ذخیرهٔ داده اثر بگذارد) + idempotency.
8. مصرف‌کننده‌ها را با `grep` پیدا کن: نام رویداد و `IOutboxService`.
