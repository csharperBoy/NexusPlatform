# استفاده از ماژول Authorization در ماژول‌های دیگر

این فایل فقط یک سؤال را جواب می‌دهد: **ماژول من چطور از Authorization محافظت کند و چطور منابع و مجوزهایش را ثبت کند؟**

## ۱. خلاصهٔ در یک نگاه

| کاری که می‌خواهی | از چه چیزی استفاده کن |
|---|---|
| محافظت از یک Action کنترلر | `[AuthorizeResource("module.resource", "View")]` از `Core.Presentation.Filters` |
| ثبت منابع و منوی ماژول هنگام Startup | `IResourcePublicService.SyncModuleResourcesAsync(...)` در `ModuleInitializer` خودت |
| دادن مجوز پیش‌فرض به نقش ادمین | `IPermissionPublicService.SeedRolePermissionsAsync(...)` |
| چک مجوز در کد Application | `IAuthorizationProcessor.CheckAccessAsync(resourceKey, action)` یا `UserDataContext.Permissions` |
| فیلتر سطری روی داده‌های خودم | `[SecuredResource]` + `IOwnerableEntity` روی موجودیت (فیلتر خودکار در Repository) |

## ۲. قرارداد عمومی

```csharp
// مسیر: Core.Application/Abstractions/Authorization/PublicService/IResourcePublicService.cs
Task SyncModuleResourcesAsync(List<ResourceDto> resources, CancellationToken cancellationToken = default);

// مسیر: Core.Application/Abstractions/Authorization/PublicService/IPermissionPublicService.cs
Task SeedRolePermissionsAsync(List<PermissionDto> permissions, CancellationToken cancellationToken = default);
Task<IReadOnlyList<PermissionDto>> GetUserAllPermissionsAsync(Guid userId, Guid? personId, List<Guid>? positionsId, List<Guid> roleIds);
Task<Guid> CreatePermissionAssigneeAsync(AssigneeType type, CancellationToken cancellationToken = default);
Task SaveAsync();

// مسیر: Core.Application/Abstractions/Authorization/Processor/IAuthorizationProcessor.cs
Task<bool> CheckAccessAsync(string resourceKey, string action);
```

> ⚠️ **نکتهٔ ساختاری:** هر سه اینترفیس امروز در `Core.Application` تعریف شده‌اند، نه در `Authorization.Application`. پس مصرف‌کننده به‌جای وابستگی به Authorization، به Core وابسته می‌شود. اگر Core حذف شود، این‌ها باید به `Authorization.Application` منتقل شوند تا مصرف‌کننده فقط به همان رفرنس بدهد.

> ⚠️ **دو قرارداد توخالی:** `IScopePublicService` هیچ متدی ندارد و `IPermissionRulePublicService` کامنت شده است. `IPermissionChecker` و `IAuthorizationChecker` هم در `Core.Application/Abstractions/Security` فقط فایل دارند و بدنه‌شان کامنت است — به آن‌ها تکیه نکن.

## ۳. مسیر توصیه‌شده

1. `[AuthorizeResource]` را روی Action بگذار، نه روی کلاس، تا هر Endpoint اکشن درست خودش را اعلام کند.
2. **کلید منبع را lowercase بده** و همان رشته‌ای باشد که در Seed ثبت کرده‌ای؛ تطبیق در `AuthorizationProcessor` حساس به حرف است.
3. در `ModuleInitializer` ماژولت اول `SyncModuleResourcesAsync` را صدا بزن، بعد `SeedRolePermissionsAsync`. اگر ترتیب را برعکس بزنی، `SeedRolePermissionsAsync` روی `Items.FirstOrDefault().Id` می‌شکند، چون منبع هنوز وجود ندارد.
4. کل بلوک Seed را داخل `if (ModuleHelper.IsActive(ModuleEnum.Authorization))` بگذار؛ اگر Authorization در `Modules:Enabled` نباشد، نسخهٔ Null تزریق می‌شود و فراخوانی‌ها بی‌اثرند.
5. روی هر موجودیتی که می‌خواهی Scope رویش اعمال شود، هم `[SecuredResource("module.key")]` بگذار و هم `IOwnerableEntity` را پیاده کن؛ اگر یکی را جا بگذاری، `ApplyFilter` کوئری را دست‌نخورده برمی‌گرداند.

نمونهٔ واقعی کنترلر (هم‌الگو با `HR.Presentation\Controller.EmploymentController`):

```csharp
using Core.Presentation.Controllers;   // BaseController، Mediator، HandleResult
using Core.Presentation.Filters;       // AuthorizeResourceAttribute
using HR.Application.Commands.Employment;
using HR.Application.Queries.Employment;

[ApiController]
[Route("api/HR/[controller]")]
public class EmploymentController : BaseController
{
    [HttpGet("GetList")]
    [AuthorizeResource("hr.employment", "View")]
    public async Task<IActionResult> GetList([FromQuery] GetEmploymentListQuery request)
        => HandleResult(await Mediator.Send(request));

    [HttpPut("{id:guid}")]
    [AuthorizeResource("hr.employment", "Edit")]
    public async Task<IActionResult> UpdateEmployment(Guid id, [FromBody] UpdateEmploymentCommand command)
        => HandleResult(await Mediator.Send(command with { Id = id }));

    [HttpDelete("{id:guid}")]
    [AuthorizeResource("hr.employment", "Delete")]
    public async Task<IActionResult> Delete(Guid id)
        => HandleResult(await Mediator.Send(new DeleteEmploymentCommand(id)));
}
```

اکشن‌های مجاز در این Attribute: `View`، `Create`، `Edit`، `Delete` (با `ToLower` مقایسه می‌شوند) و هر رشتهٔ ناشناخته به `CanView` برمی‌گردد. مجوز `Full` هر چهار اکشن را پوشش می‌دهد. نبودِ کاربر ⇒ ۴۰۱، نبودِ مجوز ⇒ ۴۰۳.

نمونهٔ واقعی Seed (هم‌الگو با `AuditSeedData`):

```csharp
private static List<ResourceDto> GetMyResourceDefinitions() => new()
{
    new()
    {
        Key = "mymodule", Name = "My Module", Type = ResourceType.Module,
        Category = ResourceCategory.System, Description = "My module", DisplayOrder = 3000,
        Children = new()
        {
            new()
            {
                Key = "mymodule.entity", Name = "Entities", Type = ResourceType.Data,
                Category = ResourceCategory.System, Description = "My entity", DisplayOrder = 3001
            }
        }
    }
};

public static async Task SeedForAuthorizationAsync(
    IResourcePublicService resourcePublicService,
    IPermissionPublicService permissionPublicService,
    IRolePublicService roleService,
    ILogger logger,
    CancellationToken ct = default)
{
    if (!ModuleHelper.IsActive(ModuleEnum.Authorization)) return;

    // ۱) منابع — درخت را می‌دهیم، ParentKey را خودِ FlattenResources پر می‌کند
    await resourcePublicService.SyncModuleResourcesAsync(GetMyResourceDefinitions(), ct);

    // ۲) مجوز پیش‌فرض برای نقش ادمین
    var adminAssigneeId = await roleService.GetAdminRolePermissionAssigneeIdAsync(ct);

    var permissions = new List<PermissionDto>
    {
        new()
        {
            ResourceKey = "mymodule.entity",
            Action      = PermissionAction.Full,
            Effect      = PermissionEffect.allow,
            AssigneeType = AssigneeType.Role,
            AssigneeId   = adminAssigneeId,
            Scopes      = new List<ScopeDto> { new() { scope = ScopeType.All } },
            Description = "Full access to my entities"
        }
    };

    await permissionPublicService.SeedRolePermissionsAsync(permissions, ct);
    logger.LogInformation("✅ My module resources and permissions seeded.");
}
```

توجه: `AssigneeId` اینجا **شناسهٔ `PermissionAssignee` نقش** است، نه `RoleId`؛ برای همین از `GetAdminRolePermissionAssigneeIdAsync` استفاده می‌کنیم و نه از `GetAdminRoleIdAsync`.

## ۴. چک مجوز در کد Application

زنده‌ترین راه، `IAuthorizationProcessor` است. اگر داخل یک Handler صدا بزنی، `SetUserDataBehavior` قبلش `UserDataContext` را پر کرده است:

```csharp
public class ApproveContractCommandHandler : IRequestHandler<ApproveContractCommand, Result>
{
    private readonly IAuthorizationProcessor _auth;

    public ApproveContractCommandHandler(IAuthorizationProcessor auth) => _auth = auth;

    public async Task<Result> Handle(ApproveContractCommand request, CancellationToken ct)
    {
        if (!await _auth.CheckAccessAsync("mymodule.entity", "Edit"))
            return Result.Fail("Access denied");

        // ... عملیات اصلی
        return Result.Ok();
    }
}
```

اگر منطق دقیق‌تری لازم داری، `UserDataContext.Permissions` را بخوان و خودت با `PermissionDto.AppliesTo(...)` و `Scopes` فیلتر کن؛ ولی بدان که `ResourceKey` و `Scopes` در این DTO همیشه قابل اتکا نیستند (بخش ۵ README). و اگر می‌خواهی مثل خود Authorization رفتار کنی، به‌جای برگرداندن `Result.Fail` می‌توانی `UnauthorizedAccessException` پرتاب کنی — همان کاری که `RowLevelSecurityProcessor.CheckPermissionAsync` روی نوشتن‌ها می‌کند.

## ۵. محدودهٔ داده (Scope) برای موجودیت‌های من

روی موجودیتت `[SecuredResource("mymodule.entity")]` بگذار و `IOwnerableEntity` را پیاده کن (چهار فیلد `OwnerUserId`/`OwnerPersonId`/`OwnerPositionId`/`OwnerOrganizationUnitId`). از آن لحظه:

- `EfRepository.AddAsync` خودش `SetOwnerDefaults` را صدا می‌زند و مالک رکورد را از `UserDataContext` پر می‌کند؛ لازم نیست دستی ست کنی.
- هر `GetByIdAsync` / `GetAllAsync` / `Query` از فیلتر `ApplyFilter` عبور می‌کند و بر اساس Scope کاربر `Where` می‌خورد.
- `CheckPermissionAsync` روی Edit/Delete، هم مجوز پایه و هم «آیا این رکورد در Scope من است» را چک می‌کند.
- اگر `[DynamicFilterable]` هم بگذاری، شرط‌های `PermissionRule` (فیلد/عملگر/مقدار و Join) هم به کوئری اضافه می‌شوند.

در عمل امروز فقط `None`، `All`، `Unit` و `Self` در `ApplyScope` شاخه دارند (`Account`، `UnitAndBelow` و `SpecificProperty` به `Where("false")` می‌افتند)، و به دلیل اینکه مجموعهٔ `Scopes` در Snapshot کاربر بارگذاری نمی‌شود، Scope عملاً به `All` میل می‌کند. پس Scope را «یک قابلیت در حال تکمیل» بدان، نه ضمانت امنیتی. ضمناً یادت باشد `Employment` و `ContactResource` و `TradingRun` امروز `[SecuredResource]` ندارند؛ یعنی لیست‌هایشان از این مسیر فیلتر نمی‌شوند و فقط چک کنترلر را دارند.

## ۶. قواعد — چه چیزی را هرگز مستقیم رفرنس نده

- **`AuthorizationDbContext` و موجودیت‌های `Resource`/`Permission`/`Scope`/`PermissionRule`/`PermissionAssignee`** — API عمومی نیستند. برای نوشتن از `IPermissionInternalService`/`IResourceInternalService` (که در Authorization.Application هستند) یا از Command/Query استفاده کن.
- **`RowLevelSecurityProcessor`، `AuthorizationProcessor`، `ResourceMetadataProvider`، `PermissionService` و `ResourceService`** — پیاده‌سازی‌اند. فقط اینترفیس‌ها (`IAuthorizationProcessor`، `IRowLevelSecurityProcessor<>`، `IResourcePublicService`، `IPermissionPublicService`) را بگیر.
- **کلیدهای کش (`auth:access:*`، `auth:permissions:*`، `auth:resource:*`)** — داخلی‌اند. برای تازه‌سازی، مجوز را از سرویس تغییر بده؛ خودش `RemoveByPatternAsync` می‌زند.
- **`ScopeType`** از namespace `Core.Shared.Enums` بیاید (بقیهٔ enumها در `Core.Shared.Enums.Authorization` هستند) — این ناهماهنگی فعلی است، پس نمی‌توانی همه را از یک `using` بگیری.
- **`Specification`های Authorization** (`UserPermissionsSpec`، `ResourceByKeySpec`، …) مال داخل ماژول‌اند؛ برای فیلتر خودت Specification بنویس.
- **به `IPermissionChecker`/`IAuthorizationChecker`/`IPermissionRulePublicService` رفرنس نده** — بدنه‌شان کامنت است و چیزی برای تزریق وجود ندارد.

## ۷. چک‌لیست وقتی یک قابلیت جدید را محافظت می‌کنی

1. کلید منبع را lowercase و بدون فاصله انتخاب کن (`mymodule.entity`) و همان را در Seed بنویس.
2. منبع را قبل از مجوز Seed کن؛ ترتیب را عوض نکن.
3. روی همهٔ Actionها `[AuthorizeResource]` بگذار — حتی `GetSelectionList`؛ Endpoint بدون Attribute عملاً باز است.
4. اگر موجودیت Ownerable است، `[SecuredResource]` را فراموش نکن، وگرنه فیلتر سطری بی‌صدا غیرفعال می‌ماند.
5. تست: با کاربر بدون مجوز (باید ۴۰۳ بدهد)، با کاربر دارای `Full` (باید رد شود)، و با کاربر دارای Scope=`Unit` (باید فقط رکورد واحد خودش را ببیند).
