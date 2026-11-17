# استفاده از Core در ماژول‌ها

این فایل راهنمای «چطور یک ماژول بسازیم که با Core درست کار کند» است. اگر Core تجزیه شود، همین فایل عملاً تبدیل به نقشهٔ آن تجزیه می‌شود.

## ۱. چه پروژه‌ای را از کجا رفرنس بدهم؟

| لایهٔ ماژول | رفرنس‌های لازم | نکته |
|---|---|---|
| `X.Domain` | `Core.Domain`، (`Core.Shared` اگر DTO/Enum مشترک لازم داری) | Domain نباید به Application یا EF وابسته شود |
| `X.Application` | `X.Domain`، `Core.Application`، (`Core.Shared`) | فقط قرارداد؛ نه EF، نه DbContext |
| `X.Infrastructure` | `X.Application`، `X.Domain`، `Core.Infrastructure`، `Core.Shared` | تنها جایی که EF و پیاده‌سازی سرویس‌ها مجاز است |
| `X.Presentation` | `X.Application`، (`Core.Presentation` برای `BaseController` و `AuthorizeResource`) | مستقیم به Infrastructure رفرنس نده |

## ۲. موجودیت را از کجا شروع کنم؟

- کلاس پایه: `Core.Domain.Common.EntityProperties.BaseEntity` (شناسه + Domain Events). اگر موجودیتت Audit می‌خواهد، `IAuditableEntity` را هم پیاده کن تا `Base_DbContext` خودش `CreatedAt/ModifiedAt` را پر کند.
- برای مالکیت داده (Row-Level Security) `IOwnerableEntity` را پیاده کن و `SetOwners(...)` را در سازنده یا متدهای دامنه پر کن.
- برای حذف منطقی `ISoftRemovable`، برای ساختار درختی `IHierarchicalStructureEntity<T, Guid>`.
- اگر می‌خواهی Authorization روی این موجودیت کار کند: `[SecuredResource("module.entity")]` بگذار؛ همین کلید است که در Seed ماژول باید به‌عنوان `ResourceKey` ثبت شود.

⚠️ کلاس‌های `AuditableEntity` و `DataScopedEntity` در `Core.Domain` **کامنت شده‌اند**. از آنها ارث‌بری نکن؛ الگوی فعلی «BaseEntity + اینترفیس» است.

## ۳. Repository و UnitOfWork

```csharp
// در Application فقط قرارداد را می‌شناسی
public class SuspendEmploymentHandler : IRequestHandler<SuspendEmploymentCommand, Result>
{
    private readonly IRepository<HRDbContext, Employment, Guid> _repo;
    private readonly IUnitOfWork<HRDbContext> _uow;

    public SuspendEmploymentHandler(
        IRepository<HRDbContext, Employment, Guid> repo,
        IUnitOfWork<HRDbContext> uow)
    {
        _repo = repo; _uow = uow;
    }

    public async Task<Result> Handle(SuspendEmploymentCommand request, CancellationToken ct)
    {
        var employment = await _repo.GetByIdAsync(request.Id, e => e.Post);
        if (employment is null) return Result.Fail("Employment not found");

        employment.Suspend(request.Reason);
        await _uow.SaveChangesAsync(ct);   // Outbox هم همین‌جا ذخیره می‌شود
        return Result.Ok();
    }
}
```

قواعد:
- `IRepository` را باید در DI خودِ ماژول ثبت کنی؛ Core فقط نوع باز `(typeof(IRepository<,,>), typeof(EfRepository<,,>))` را ثبت می‌کند و برای `IUnitOfWork<TContext>` هم باید `EfUnitOfWork<XDbContext>` را خودت ثبت کنی (مثل `Audit.Infrastructure`).
- برای کوئری‌های پیچیده، `BaseSpecification<T>` بساز و `ISpecificationRepository<TEntity,TKey>` را تزریق کن. `FindBySpecAsync` هم‌زمان `Items` و `TotalCount` می‌دهد (مناسب Paging).
- از `AsQueryable()`/`AsNoTrackingQueryable()` در Application فقط وقتی استفاده کن که واقعاً به ترکیب آزاد Query نیاز داری؛ وگرنه Specification کد را تمیزتر نگه می‌دارد.

## ۴. DbContext و Migration

```csharp
public class HRDbContext : Base_DbContext
{
    public HRDbContext(DbContextOptions<HRDbContext> options, IServiceProvider sp) : base(options, sp) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("hr");                       // هر ماژول اسکیمای خودش
        modelBuilder.ApplyConfiguration(new EmploymentConfiguration());
        modelBuilder.ApplyConfiguration(new OutboxMessageConfiguration("hr"));
    }
}
```

- Configurationها را از `Core.Infrastructure.Database.Configurations.BaseConfiguration<T>` ارث بده تا ایندکس‌ها و ستون‌های Audit استاندارد خودکار ساخته شوند.
- جدول تاریخچهٔ Migration را در DI جدا کن: `b.MigrationsHistoryTable("__HRMigrationsHistory", "hr")`.
- Migration را در `{Module}_UseInfrastructure` با `IMigrationManager.MigrateAsync<HRDbContext>()` اجرا کن (الگوی Audit/Base).

## ۵. کاربر جاری، مجوز و دادهٔ Scoped

- **کاربر جاری:** `UserDataContext` (Scoped) — `UserId`، `UserName`، `PartyId`، `PostIds`، `RoleIds`، `Permissions`. این شیء با `SetUserDataBehavior` پر می‌شود؛ در Background Service خودت باید `IUserDataContextProvider.SetUserData(userId, ct)` را صدا بزنی.
- **`ICurrentUserService` را تزریق نکن** — در DI ثبت نشده و بلااستفاده است.
- **چک مجوز در کنترلر:** `[AuthorizeResource("hr.employment", "Edit")]` روی اکشن. فیلتر خودش `Unauthorized`/`Forbid` می‌دهد.
- **چک مجوز در Application:** `IPermissionChecker.HasPermissionAsync("hr.employment.edit")`.
- **صفحه‌بندی و خروجی استاندارد:** `BaseController.HandleResult(...)` و `HandleBatchResult(...)`.

## ۶. ثبت ماژول (قرارداد نام‌گذاری — مهم‌ترین بخش)

لودر Core اسمبلی‌ها را با `Assembly.Load("{Module}.Infrastructure")` پیدا می‌کند و متدهای static زیر را صدا می‌زند. اگر نام یا امضای یکی از این‌ها غلط باشد، ماژول **بی‌صدا** لود نمی‌شود:

```csharp
public static IServiceCollection HR_AddDomain(this IServiceCollection s, IConfiguration c);
public static IServiceCollection HR_AddApplication(this IServiceCollection s, IConfiguration c);
public static IServiceCollection HR_AddInfrastructure(this IServiceCollection s, IConfiguration c);
public static IServiceCollection HR_AddPresentation(this IServiceCollection s, IConfiguration c);
public static IServiceCollection HR_AddHealthChecks(this IServiceCollection s, IConfiguration c);
public static Task<IApplicationBuilder> HR_UseInfrastructure(this IApplicationBuilder app);
```

- اولین پارامتر باید دقیقاً `IServiceCollection` (یا `IApplicationBuilder` برای `Use`) باشد.
- نام متد از `Modules:Enabled[].Name` ساخته می‌شود؛ پس در `appsettings.json` باید `HR` بنویسی نه `Hr`.
- برای ماژول افزونه (مثل `HR.IrisaSync.Extention`) قرارداد این است: `IrisaSync_AddDependency` و `IrisaSync_UseDependency` در اسمبلی `HR.IrisaSync.Extention`، و تعریف آن زیر `Extentions` همان ماژول در `Modules:Enabled`.

## ۷. قواعد و ممنوعیت‌ها

- **DbContext ماژول دیگر را تزریق نکن.** فقط از قرارداد عمومی آن ماژول استفاده کن (حتی اگر هنوز آن قرارداد در Core تعریف شده باشد).
- **به `Core.Infrastructure` از لایهٔ Application رفرنس نده.** اگر مجبور شدی، یعنی یک انتزاع در جای اشتباه است.
- **`Result`/`Result<T>` را برای خطاهای قابل‌انتظار استفاده کن**، Exception را برای خطاهای غیرمنتظره.
- **در `{Module}_AddApplication` MediatR را دوباره ثبت نکن**؛ Core با `RegisterServicesFromAssemblies(AppDomain.CurrentDomain.GetAssemblies())` همهٔ هندلرهای اسمبلی‌های لودشده را ثبت می‌کند. (بعضی ماژول‌ها مثل `Audit` باز هم `AddMediatR` صدا می‌زنند — این تکراری است و بهتر است حذف شود.)
- **در `{Module}_AddInfrastructure` بیرون از `BuildServiceProvider` سرویس resolve نکن** — الگوی موجود در Audit دقیقاً همین است و باعث ساخت یک ServiceProvider دوم می‌شود؛ برای کد جدید از `services.AddHostedService` یا factory استفاده کن.

## ۸. چک‌لیست افزودن یک ماژول جدید

1. چهار پروژه `X.Domain/Application/Infrastructure/Presentation` با همان نام‌گذاری بساز.
2. رفرنس‌ها را طبق جدول بخش ۱ ببند.
3. `XDbContext` را از `Base_DbContext` بساز + اسکیمای اختصاصی.
4. چهار متد `X_AddDomain/Application/Infrastructure/Presentation` + `X_AddHealthChecks` + `X_UseInfrastructure` را بنویس.
5. در `appsettings.json` ماژول و ترتیبش را به `Modules:Enabled` اضافه کن.
6. اگر منبع/مجوز/منو داری: در `ModuleInitializer` با `IResourcePublicService` / `IPermissionPublicService` / `IMenuPublicService` Seed کن.
7. یک تست ساده بگذار که ثابت کند DI بالا می‌آید و Null Service جای واقعی را نگرفته.
