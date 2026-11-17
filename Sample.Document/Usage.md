# استفاده از ماژول Sample (به‌عنوان قالب، نه وابستگی)

این فایل جواب یک سؤال است: **چطور از Sample استفاده کنم؟** و جواب کوتاه: **از آن کپی کن، به آن رفرنس نده.**

## ۱. خلاصهٔ در یک نگاه

| کاری که می‌خواهی | کار درست |
|---|---|
| یک ماژول جدید بسازی | پروژه‌های Sample را کپی کن، نام‌ها را عوض کن، و پنج جای DI/Seed را که پایین لیست شده کامل کن |
| بفهمی Command/Query/Validator/Specification چه شکلی است | همان فایل‌های Sample را بخوان؛ هر کلاس کامنت فارسی «چرا» دارد |
| در کد خودت از Sample استفاده کنی | ❌ نکن. هیچ سرویس عمومی ندارد و در هیچ هاستی بارگذاری نمی‌شود |
| تست سرویس با Moq بنویسی | `Sample.Test/Services/SampleServiceTests.cs` را الگو بگیر |

## ۲. قرارداد عمومی

**هیچ.** Sample نه `I*PublicService` دارد و نه در DI هیچ هاستی ثبت می‌شود. دو اینترفیس آن داخلی‌اند:

```csharp
// مسیر: Sample.Application/Interfaces/ISampleService.cs
public interface ISampleService
{
    Task<Result<SampleApiResponse>> SampleApiMethodAsync(SampleApiRequest request);
    Task<Result<SampleApiResponse>> SampleApiMethodWithCacheAsync(SampleApiRequest request);
}

// مسیر: Sample.Application/Interfaces/ISampleQueryService.cs
public interface ISampleQueryService
{
    Task<Result<IReadOnlyList<SampleEntity>>> GetBySpecAsync(string property1, int page = 1, int pageSize = 10);
    Task<Result<IReadOnlyList<SampleEntity>>> GetCachedSamplesAsync(string property1);
}
```

> ⚠️ اگر می‌خواهی Sample را واقعاً اجرا کنی (مثلاً برای یادگیری)، بدان که **`ISampleService` و `ISampleQueryService` در DI ثبت نشده‌اند**. باید خودت در `Sample_AddInfrastructure` این دو خط را اضافه کنی:
>
> ```csharp
> services.AddScoped<ISampleService, SampleService>();
> services.AddScoped<ISampleQueryService, SampleQueryService>();
> ```

## ۳. الگوی درست استفاده: کپی برای ساخت ماژول جدید

### گام ۱ — ساختار را کپی کن

```
{New}.Domain        → موجودیت + Domain Event + Specification
{New}.Application   → Command/Query + Handler + Validator + Interface + EventHandler
{New}.Infrastructure→ DbContext + Configuration + Seed + Service + DI + ModuleInitializer
{New}.Presentation  → Controller + DI
{New}.Shared        → Integration Event (اگر ماژول دیگری باید بشنود)
{New}.Test          → تست سرویس‌ها با Moq
```

### گام ۲ — نام‌ها را عوض کن (این‌هایی که فراموش می‌شوند)

- نام متدهای DI: `New_AddApplication` / `New_AddInfrastructure` / `New_AddDomain` / `New_AddPresentation` / `New_AddHealthChecks` / `New_UseInfrastructure` — **اسم ماژول در `Modules:Enabled` باید دقیقاً با پیشوند این متدها یکی باشد**، چون Loader با `Assembly.Load("{Name}.Infrastructure")` و رفلکشن کار می‌کند.
- `HasDefaultSchema("sample")` → اسکیمای خودت؛ `MigrationsHistoryTable("__SampleMigrationsHistory","sample")` → نام خودت.
- نام Health Check `SampleDatabase` → نام خودت.
- `DbSet<SampleEntity> Sample` و `SampleEntityConfiguration` → موجودیت خودت.

### گام ۳ — این پنج چیز را از خودت اضافه کن (Sample ندارد!)

1. **ثبت سرویس‌ها** (`AddScoped<IMyService, MyService>()`) — همان‌جا در `{New}_AddInfrastructure`.
2. **ثبت Validatorها:**
   ```csharp
   services.AddValidatorsFromAssembly(typeof(MyCommandValidator).Assembly);
   ```
   بدون این خط، `ValidationBehavior` (که در Core ثبت شده) هیچ Validatorای پیدا نمی‌کند و اعتبارسنجی‌ات اجرا نمی‌شود. تنها ماژولی که این خط را دارد `Authorization` است.
3. **Seed منو:** یک `List<MenuDto>` بساز و به `IMenuPublicService.SyncModuleMenusAsync` بده (راهنما: `Navigation.Document/Usage.md`).
4. **Seed منبع/مجوز:** `IResourcePublicService.SyncModuleResourcesAsync` + `IPermissionPublicService.SeedRolePermissionsAsync` (الگو: `AuditSeedData` / `SchedulerSeedData`).
5. **مجوز روی Endpoint:**
   ```csharp
   [HttpPost("SampleApi")]
   [AuthorizeResource("mysample.sample", "Create")]
   public async Task<IActionResult> SampleApi([FromBody] SampleApiCommand command) { ... }
   ```

### گام ۴ — تست را از دست نده

```csharp
// الگو: Sample.Test/Services/SampleServiceTests.cs
_repositoryMock = new Mock<IRepository<MyDbContext, MyEntity, Guid>>();
_uowMock        = new Mock<IUnitOfWork<MyDbContext>>();
_cacheMock      = new Mock<ICachePublicService>();
_loggerMock     = new Mock<ILogger<MyService>>();

_service = new MyService(_repositoryMock.Object, _uowMock.Object,
                         _loggerMock.Object, _userContextMock.Object, _cacheMock.Object);

await _service.MyMethodAsync(request);

_repositoryMock.Verify(r => r.AddAsync(It.IsAny<MyEntity>()), Times.Once);
_uowMock.Verify(u => u.SaveChangesAsync(default), Times.Once);
```

## ۴. چه چیزی عمومی **نیست**

- `SampleDbContext`، `SampleEntity`، `SampleGetSpec`، `SampleEntityConfiguration`، `SampleSeedData` → داخلی.
- `SampleService` / `SampleQueryService` → `public` هستند ولی در DI ثبت نشده‌اند؛ یعنی عملاً قابل استفاده نیستند.
- `SampleCreatedIntegrationEvent` → تعریف شده ولی هیچ‌جا منتشر نمی‌شود. اگر الگوی Integration Event می‌خواهی، از ماژول‌های واقعی (`Identity.Shared`، `HR.Shared`) الگو بگیر.

## ۵. قواعد

- **Sample را در `Modules:Enabled` نگذار.** اگر بگذاری، `ModuleInitializer` آن سعی می‌کند `IRepository` بگیرد (که از Open Generic در Core می‌آید و کار می‌کند) ولی به‌محض اولین درخواست به `POST api/sample/sample/SampleApi`، DI برای `ISampleService` خطا می‌دهد.
- **`Result<T>` را نگه دار.** همهٔ Handlerها `Result`/`Result<T>` برمی‌گردانند؛ Exception را به لایهٔ بالا پرت نکن (الگوی `SampleApiCommandHandler`).
- **Domain Event را در رفتار موجودیت Raise کن، نه در سرویس.** (`MarkSample` → `AddDomainEvent`.) این تنها راهی است که Outbox و UnitOfWork رویداد را ببینند.
- **کش را بعد از Write باطل کن، نه قبلش.** الگوی `SampleApiMethodWithCacheAsync` + `RemoveByPatternAsync`.
- **`ApplyPaging` در Specification و `Skip/Take` در سرویس را هم‌زمان نکن.** در Sample هر دو وجود دارد (`SampleGetSpec` صفحه‌بندی می‌کند و `GetBySpecAsync` دوباره `Skip/Take` می‌زند) — این یک اشتباه آموزشی است؛ یکی را انتخاب کن.
- **کامنت‌های فارسی Sample را نگه دار.** آن‌ها بخشی از ارزش این قالب هستند.

## ۶. چک‌لیست ساخت ماژول جدید از روی Sample

1. شش پروژه را کپی و نام‌گذاری کردی (`{New}.Domain` … `{New}.Test`)؟
2. `.csproj`ها به Core رفرنس می‌دهند و بین خودشان زنجیرهٔ درست دارند؟
3. `{New}_Add*`ها با نام ماژول در `Modules:Enabled` هم‌نام‌اند؟
4. اسکیما و `MigrationsHistoryTable` را عوض کردی؟
5. سرویس‌ها را `AddScoped` کردی؟
6. `AddValidatorsFromAssembly` را اضافه کردی؟
7. Seed منو + منبع + مجوز را نوشتی؟
8. `[AuthorizeResource]` روی Endpointها گذاشتی؟
9. `{New}_AddHealthChecks` را با `AddDbContextCheck<{New}DbContext>("{New}Database")` پر کردی؟
10. تست سرویس (حداقل یک مسیر موفق و یک مسیر خطا) نوشتی؟
