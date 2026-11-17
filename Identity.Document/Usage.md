# استفاده از ماژول Identity در ماژول‌های دیگر

این فایل فقط یک سؤال را جواب می‌دهد: **اگر ماژول من به «شناسهٔ کاربر»، «نام کاربری» یا «نقش‌های کاربر» نیاز دارد، چطور آن را از Identity بگیرم بدون اینکه به دیتابیس یا `UserManager` دست بزنم؟**

## ۱. خلاصهٔ در یک نگاه

| کاری که می‌خواهی | از چه چیزی استفاده کن |
|---|---|
| از نام کاربری به `UserId` برسی (یا برعکس) | `IUserPublicService.GetUserId` / `GetUserName` |
| `FkPermissionAssigneeId` کاربر را بگیری | `IUserPublicService.GetUserPermissionAssigneeIdAsync` |
| `PartyId` کاربر را بگیری (برای ماژول People) | `IUserPublicService.GetPartyId` |
| شناسهٔ Assignee نقش ادمین را برای Seed بگیری | `IRolePublicService.GetAdminRolePermissionAssigneeIdAsync` |
| نقش‌های کاربر را بگیری | `IRolePublicService.GetAllUserRolesId` / `GetAllUserRolesPermissionAssigneeId` |
| کاربر یا نقش بسازی و نقش تخصیص بدهی | `CreateUserCommand` / `CreateRoleCommand` / `AssignRoleCommand` با MediatR، یا Endpointهای `api/identity/*` |
| ورود، رفرش و خروج | `POST api/identity/Auth/{login,refresh,logout}` — نه `IAuthService` |

## ۲. قرارداد عمومی

```csharp
// مسیر: Core.Application/Abstractions/Identity/PublicService/IUserPublicService.cs
public interface IUserPublicService
{
    Task<Guid?> GetPartyId(Guid userId);
    Task<Guid> GetUserId(string userName);
    Task<string?> GetUserName(Guid userId);
    Task<Guid> GetUserPermissionAssigneeIdAsync(Guid userId);
}
```

```csharp
// مسیر: Core.Application/Abstractions/Identity/PublicService/IRolePublicService.cs
public interface IRolePublicService
{
    Task<Guid> GetAdminRolePermissionAssigneeIdAsync(CancellationToken cancellationToken = default);
    Task<List<Guid>> GetAllUserRolesPermissionAssigneeId(Guid id);
    Task<Guid> GetAdminRoleIdAsync(CancellationToken cancellationToken = default);
    Task<List<Guid>> GetAllUserRolesId(Guid userId);
}
```

> ⚠️ **نکتهٔ ساختاری:** این دو اینترفیس در `Core.Application` تعریف شده‌اند، نه در `Identity.Application`. پس برای استفاده از آن‌ها هیچ رفرنسی به Identity لازم نیست — و همین هم مشکل است: مرز ماژول در Core شکسته شده و اگر Core حذف شود، این قراردادها باید به `Identity.Application` منتقل شوند تا مصرف‌کننده فقط به همان رفرنس بدهد.

`IUserInternalService` و `IRoleInternalService` (در `Identity.Application/Interfaces`) نسخهٔ داخلی‌اند و متدهای نوشتن دارند (`CreateUserAsync`، `UpdateUserAsync`، `AssignRoleToUserAsync`، `CreateRoleAsync`، `GetUserRolesAsync` و…). آن‌ها API عمومی نیستند؛ امروز فقط Handlerهای داخل خود Identity از آن‌ها استفاده می‌کنند.

## ۳. مسیر توصیه‌شده

1. اگر فقط `Guid` و `string` لازم داری، هیچ رفرنسی به پروژه‌های Identity نده؛ `Core.Application` را که همین حالا داری کافی است.
2. `IUserPublicService` یا `IRolePublicService` را در Constructor تزریق کن. ثبت در DI لازم نیست؛ `Identity_AddInfrastructure` هر دو را با `RoleService`/`UserService` ثبت کرده است.
3. برای نوشتن، از Commandهای MediatR استفاده کن (نه `UserManager`). این کار به رفرنس `Identity.Application` نیاز دارد و امروز هیچ ماژولی این رفرنس را ندارد؛ اگر لازم شد، اول با تیم Identity هماهنگ کن.
4. `FkPermissionAssigneeId` را فقط به‌عنوان یک شناسهٔ مبهم جابه‌جا کن؛ هرگز آن را به‌جای `UserId` به کسی نده.

نمونهٔ خواندن (بدون هیچ رفرنسی به Identity):

```csharp
// در ماژول خودت، بدون هیچ رفرنسی به پروژه‌های Identity
public class GetEmploymentOwnerQueryHandler : IRequestHandler<GetEmploymentOwnerQuery, Result<EmploymentOwnerDto>>
{
    private readonly IUserPublicService _users;

    public GetEmploymentOwnerQueryHandler(IUserPublicService users) => _users = users;

    public async Task<Result<EmploymentOwnerDto>> Handle(GetEmploymentOwnerQuery request, CancellationToken ct)
    {
        // نام کاربری → شناسه
        var userId = await _users.GetUserId(request.UserName);

        // شناسه → نام کاربری و شناسهٔ Assignee
        var userName = await _users.GetUserName(userId);
        var assigneeId = await _users.GetUserPermissionAssigneeIdAsync(userId);

        return Result<EmploymentOwnerDto>.Ok(
            new EmploymentOwnerDto(userId, userName ?? request.UserName, assigneeId));
    }
}
```

نمونهٔ Seed ماژول (الگویی که همهٔ ماژول‌ها استفاده می‌کنند):

```csharp
var adminAssigneeId = await roleService.GetAdminRolePermissionAssigneeIdAsync(cancellationToken);

var permissions = new List<PermissionDto>
{
    new()
    {
        ResourceKey = "contact.phonebook",
        Action = PermissionAction.Full,
        AssigneeType = AssigneeType.Role,
        AssigneeId = adminAssigneeId,
        Effect = PermissionEffect.allow,
        Scopes = new List<ScopeDto> { new() { scope = ScopeType.All } }
    }
};

await permissionPublicService.SeedRolePermissionsAsync(permissions, cancellationToken);
```

## ۴. قواعد

- **به Infrastructure وابسته نشو.** `IdentityDbContext`، `ApplicationUser`، `ApplicationRole`، `UserManager<ApplicationUser>`، `RoleManager<ApplicationRole>` و `IJwtTokenService` جزو API عمومی نیستند.
- **`IAuthService` را مستقیم صدا نزن.** این سرویس فقط برای Handlerهای خود Identity است و در هیچ ماژول دیگری استفاده نشده؛ برای ورود از HTTP استفاده کن.
- **روی تخصیص نقش پیش‌فرض حساب نکن.** `UserRegisteredEvent` هیچ‌جا publish نمی‌شود، پس `AssignDefaultRoleEventHandler` اجرا نمی‌شود و کاربر تازه‌ساخته‌شده بدون نقش می‌ماند. نقش را صریح بده (`CreateUserCommand.roles` یا `AssignRoleCommand`).
- **`GetUsers`/`getRoles` را برای فیلترهای دقیق به کار نبر.** نتیجه با کلید ثابت `identity:user:full` / `identity:role:full` کش می‌شود و اولین درخواست فیلترشده، کش بقیه را خراب می‌کند.
- **«کاربر جاری» را از Identity نگیر.** از `UserDataContext` / `IUserDataContextProvider` استفاده کن؛ آن‌ها همان Snapshot آماده (UserId، PartyId، نقش‌ها، PermissionAssigneeها) را می‌دهند.
- **`GetPartyId` را با People اشتباه نگیر.** این متد فقط `FkPartyId` را برمی‌گرداند؛ برای شخص از `IPersonPublicService` در `Core.Application/Abstractions/People` استفاده کن. دقت کن برای کاربر ادمین این مقدار می‌تواند `Guid.Empty` باشد، نه `null`.
- **در حالت خاموش بودن Identity، Null Serviceها خطرناک‌اند.** `Core.Application` برای این دو اینترفیس `NullUserService`/`NullRoleService` ثبت می‌کند که `Task` برنمی‌گردانند و `null` می‌دهند؛ هر فراخوانی NRE می‌شود. اگر ماژول تو به Identity وابستهٔ واقعی است، فعال بودنش را در `Modules:Enabled` تضمین کن.

## ۵. چک‌لیست وقتی می‌خواهی از Identity استفاده کنی

1. آیا واقعاً `Identity` را لازم داری، یا `UserDataContext` همان اطلاعات را دارد؟
2. آیا از `IUserPublicService`/`IRolePublicService` استفاده می‌کنی و نه از `UserManager`؟
3. آیا هیچ نوعی از `Identity.Infrastructure` یا `Identity.Domain` در امضای عمومی ماژول تو ظاهر نشده؟
4. اگر کاربر می‌سازی، نقشش را صریح تخصیص داده‌ای؟
5. آیا `FkPermissionAssigneeId` را جایی به‌اشتباه به‌عنوان `UserId` استفاده نکرده‌ای؟
6. تست: مسیر موفق + حالت «کاربر وجود ندارد» (`GetUserId` برای نام ناموجود `Exception` می‌اندازد، نه `null`).
