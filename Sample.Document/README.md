# ماژول Sample (نمونهٔ مرجع / مستندسازی با کد)

> **در یک جمله:** Sample یک ماژول «کاربردی» نیست؛ **قالب مرجع** است. هر چیزی که یک ماژول درست باید داشته باشد — Command، Handler، Validator، Query، Domain Event، Specification، Seed، DbContext، تست — یک نسخهٔ حداقلی و خوانا در Sample دارد تا وقتی ماژول جدید می‌سازی، به‌جای حدس زدن، از این الگو کپی کنی.

| | |
|---|---|
| **پروژه‌ها** | `Sample.Domain` / `Sample.Application` / `Sample.Infrastructure` / `Sample.Presentation` / `Sample.Shared` / `Sample.Test` (کامل‌ترین مجموعهٔ پروژه‌ها در پلتفرم) |
| **قرارداد عمومی** | ندارد — و این عمدی است؛ Sample قرار نیست کسی به آن وابسته شود |
| **وابستگی به ماژول‌های دیگر** | Core (Application / Domain / Infrastructure / Shared) |
| **چه کسانی از آن استفاده می‌کنند** | هیچ ماژول اجرایی. در هیچ `Modules:Enabled`ای نیست و در هیچ هاستی بارگذاری نمی‌شود؛ مصرف‌کنندهٔ آن «توسعه‌دهنده» است |
| **جایگاه پیشنهادی در مدل لایه‌ای** | نمونهٔ آموزشی (لایه ۳) — نه لایهٔ اجرایی |

---

## ۱. این ماژول به چه کار می‌آید؟ (به زبان ساده)

هر معماری‌ای یک «فایل نمونه» لازم دارد و Sample همان است: **documentation-by-code**. اگر بخواهی بفهمی در این پلتفرم:

- Command چطور تعریف می‌شود؟ → `SampleApiCommand`
- Handler چطور سرویس را صدا می‌زند و `Result<T>` برمی‌گرداند؟ → `SampleApiCommandHandler`
- Validator کجاست و چه شکلی است؟ → `SampleApiCommandValidator`
- Query با Specification چطور نوشته می‌شود؟ → `GetSamplesByPropertyQuery` + `SampleQueryService`
- Domain Event چطور Raise می‌شود و کجا Handle می‌شود؟ → `MarkSample` → `SampleActionEvent` → `SampleEventHandler`
- Seed، DbContext، Configuration، Outbox، Health Check، Migration و تست چه شکلی‌اند؟ → همه در `Sample.Infrastructure` و `Sample.Test`

پاسخ همه‌اش در Sample است، با کامنت‌های فارسی مفصل بالای هر کلاس که «چرا» را توضیح می‌دهند.

## ۲. چه چیزهایی داخلش هست؟

### دامنه (`Sample.Domain`)

- **`SampleEntity`** — `BaseEntity` + `IAuditableEntity` + `IOwnerableEntity` + `IAggregateRoot`؛ یک فیلد ساده `property1`، یک Value Object (`PhoneNumber? ValueObjectsSample`) و یک رفتار دامنه‌ای `MarkSample(string)` که `AddDomainEvent(new SampleActionEvent(value))` می‌زند.
- **`SampleActionEvent`** — پیاده‌سازی `IDomainEvent` با `ActionProperty1` و `OccurredOn = DateTime.UtcNow`. کامنت کلاس صادقانه می‌گوید نسخهٔ قبلی `ITimeProvider` را تزریق نمی‌کرد و `NullReferenceException` می‌داد.
- **`SampleGetSpec`** — `BaseSpecification<SampleEntity>` با فیلتر `property1`، `ApplyOrderBy`، `ApplyThenOrderBy(Id)` و `ApplyPaging` (نمونهٔ کامل Specification).
- **`Sample_AddDomain`** — عمداً خالی است (با توضیح چرا).

### کاربرد (`Sample.Application`)

- **`SampleApiCommand(string property1, string property2) : IRequest<Result<SampleApiResponse>>`**
- **`SampleApiCommandHandler`** — سرویس را صدا می‌زند، خطا را به `Result.Fail` تبدیل می‌کند و در موفقیت `SampleApiResponse("عملیات موفق بود")` می‌دهد. بخش بررسی مجوز (`IPermissionChecker`) **کامنت** شده.
- **`SampleApiCommandValidator`** — دو قاعده: `NotEmpty` و `MaximumLength(200)`.
- **`GetSamplesByPropertyQuery(property1, Page, PageSize)`** + Handler → `ISampleQueryService.GetBySpecAsync`.
- **`ISampleService` / `ISampleQueryService`** — جداسازی Write از Read (CQRS در سطح سرویس).
- **`SampleEventHandler : DomainEventHandler<SampleActionEvent>`** — نمونهٔ ارث‌بری از Base Class رویداد + پیچیدن کار در سیاست `DefaultRetry` پالی.

### زیرساخت (`Sample.Infrastructure`)

- **`SampleDbContext`** — اسکیمای `sample`، `DbSet<SampleEntity> Sample`، `DbSet<OutboxMessage>`، `SampleEntityConfiguration` و `OutboxMessageConfiguration("sample")`.
- **`SampleService`** — `IRepository` + `IUnitOfWork` + `ICachePublicService`؛ بعد از Write، `RemoveByPatternAsync("sample:list:{property1}")`.
- **`SampleQueryService`** — خواندن با `ISpecificationRepository`، صفحه‌بندی، و نمونهٔ Cache-Aside با کلید `sample:list:{property1}` و TTL پنج دقیقه.
- **`SampleSeedData`** — نمونهٔ Seed ایدمپوتنت (`ExistsAsync` روی `SeededValue1`، بعد `AddRangeAsync` + `SaveChangesAsync`).
- **`ModuleInitializer`** — الگوی HostedService برای Seed.
- **Migration و Health Check** — اسکیمای `sample`، `__SampleMigrationsHistory`، چک `SampleDatabase`.
- **`Sample_UseInfrastructure`** — اجرای Migration با `IMigrationManager`.

### ارائه و اشتراک

- **`SampleController`** — `POST api/sample/sample/SampleApi`، بدون هیچ `[Authorize]`؛ نمونهٔ «Controller نازک» که فقط `IMediator.Send` می‌کند.
- **`Sample.Shared/Events/SampleCreatedIntegrationEvent`** — نمونهٔ Integration Event در لایهٔ Shared.

### تست

- **`Sample.Test`** — دو تست با Moq روی `SampleService` (افزودن + ذخیره؛ افزودن + ذخیره + پاک‌کردن کش) و یک تست روی `SampleQueryService`. نمونهٔ این‌که چطور `IRepository` و `IUnitOfWork` را Mock کنی.

## ۳. چه چیزهایی داخلش **نیست**

- **UI، صفحه یا منو ندارد.** هیچ `MenuDto`، منبع یا مجوزی Seed نمی‌کند.
- **مجوز ندارد.** هیچ `[AuthorizeResource]`ای در کنترلر نیست؛ چک مجوز در Handler هم کامنت شده. این «نمونهٔ حداقلی» است، نه نمونهٔ امن.
- **منطق کسب‌وکار واقعی ندارد.** `property1`/`property2` عمداً بی‌معنا هستند.
- **در هیچ هاستی اجرا نمی‌شود.** در هیچ `appsettings*.json`‌ای `"Sample"` در `Modules:Enabled` نیست، پس `ModuleInitializer` و Seed آن هم هرگز اجرا نمی‌شوند.

## ۴. یک نگاه فنی کوتاه

- ثبت با کنوانسیون کامل: `Sample_AddApplication` (MediatR)، `Sample_AddInfrastructure` (DbContext + `IUnitOfWork` + `ISpecificationRepository` + `AddHostedService<ModuleInitializer>` + Outbox processor)، `Sample_AddDomain` (خالی)، `Sample_AddPresentation` (کنترلر + MediatR)، `Sample_AddHealthChecks`، `Sample_UseInfrastructure` (Migration).
- `SampleActionEvent` از طریق Outbox منتشر می‌شود و `SampleEventHandler` آن را می‌گیرد (چون `IDomainEvent : INotification` است و `MediatorEventBus` با `IMediator.Publish` کار می‌کند).
- `SampleDbContext` دو سازنده دارد: یکی با `IServiceProvider` (برای DI) و یکی فقط `DbContextOptions` (برای Design-time / `SampleDbContextFactory`).

## ۵. وضعیت واقعی امروز (صادقانه)

Sample به‌عنوان «سند» عالی است، ولی **اگر امروز آن را در `Modules:Enabled` بگذاری، کار نمی‌کند**:

- **سرویس‌ها در DI ثبت نمی‌شوند.** `ISampleService` و `ISampleQueryService` در هیچ‌جای مخزن `AddScoped` نشده‌اند (نه در `Sample_AddInfrastructure`، نه در `Sample_AddApplication`، نه در `Sample_AddPresentation`). نتیجه: اولین درخواست به `POST api/sample/sample/SampleApi` با `InvalidOperationException: Unable to resolve service for type 'ISampleService'` می‌شکند. این تنها جای نمونه است که «ناقص» است — بقیهٔ اجزا کامل‌اند.
- **`IRepository<,,>` از طریق Core به‌صورت Open Generic ثبت شده** (`Core.Infrastructure/DependencyInjection/ServiceCollectionExtensions.cs:129`)، پس `ModuleInitializer` و `SampleService` برای گرفتن Repository مشکلی ندارند؛ فقط ثبت صریح `IRepository` در `Sample_AddInfrastructure` وجود ندارد. اما `ISpecificationRepository<,>` به‌صورت Open Generic ثبت نشده (کامنت است) و Sample خودش نسخهٔ بستهٔ `ISpecificationRepository<SampleEntity, Guid>` را ثبت می‌کند — که درست است، ولی الگویی است که هر ماژول باید تکرار کند.
- **Validator اجرا نمی‌شود.** `ValidationBehavior` وجود دارد و `IEnumerable<IValidator<TRequest>>` می‌گیرد، اما در کل مخزن فقط `Authorization` است که `AddValidatorsFromAssembly` را صدا می‌زند. پس `SampleApiCommandValidator` ثبت نمی‌شود و هیچ اعتبارسنجی‌ای روی `SampleApiCommand` انجام نمی‌شود. این یک درس مهم برای کسی است که از Sample کپی می‌کند: **Validator نوشتن کافی نیست، ثبتش هم لازم است.**
- **`SampleCreatedIntegrationEvent` هیچ‌جا منتشر نمی‌شود.** فقط تعریف شده و در کامنت‌های Core به‌عنوان مثال آمده. یعنی «انتشار Integration Event» در این قالب فقط ایده است، نه کد اجراشدنی.
- **بدون مجوز و بدون منو** است؛ برای کپی‌کردن به‌عنوان شروع یک ماژول واقعی، باید هر دو را خودت اضافه کنی.
- **جمع‌بندی:** Sample را به‌عنوان «چه شکلی باید باشد» بخوان، نه «چه چیزی کار می‌کند». ارزش اصلی‌اش در ساختار و کامنت‌هاست؛ دو سه ثبت DI و یک `AddValidatorsFromAssembly` کم دارد تا کامل شود.
