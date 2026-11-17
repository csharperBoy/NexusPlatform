# ماژول Identity (احراز هویت، کاربران و نقش‌ها)

> **در یک جمله:** Identity «دروازهٔ ورود» پلتفرم است؛ کاربر را می‌سازد، رمز عبور را بررسی می‌کند، توکن JWT و RefreshToken می‌دهد و نقش‌ها را نگه می‌دارد — ولی تصمیم «چه کسی چه کاری می‌تواند بکند» را به ماژول Authorization واگذار می‌کند.

| | |
|---|---|
| **پروژه‌ها** | `Identity.Domain` / `Identity.Application` / `Identity.Infrastructure` / `Identity.Presentation` / `Identity.Shared` (و `Identity.Test`) |
| **قرارداد عمومی** | `IUserPublicService` و `IRolePublicService` — هر دو در `Core.Application/Abstractions/Identity/PublicService` تعریف شده‌اند، نه در `Identity.Application` |
| **وابستگی به ماژول‌های دیگر** | Authorization (PermissionAssignee، منابع، مجوزها)، Navigation (منو)، Event (پردازشگر Outbox)، People و HR (فقط قرارداد تزریق‌شده و بلااستفاده) |
| **چه کسانی از آن استفاده می‌کنند** | Seed بیشتر ماژول‌ها با `IRolePublicService.GetAdminRolePermissionAssigneeIdAsync`، `UserDataContextProvider` در Authorization، و `Notification.Application` از راه `Identity.Shared.Events.UserRegisteredEvent` |
| **جایگاه پیشنهادی در مدل لایه‌ای** | لایه ۱ (سرویس‌دهندهٔ هویت به بقیهٔ ماژول‌ها) |

---

## ۱. این ماژول به چه درد می‌خورد؟ (به زبان ساده)

هر سیستم چند کاربره سه سؤال دارد: «تو کی هستی؟»، «چه نقش‌هایی داری؟» و «با این نقش‌ها چه کاری می‌توانی بکنی؟». Identity دو سؤال اول را جواب می‌دهد و سؤال سوم را عمداً به Authorization می‌سپارد.

جواب سؤال اول `ApplicationUser` است (بر پایهٔ `IdentityUser<Guid>`) و جواب دوم `ApplicationRole` (بر پایهٔ `IdentityRole<Guid>`). نکتهٔ کلیدی این است که Identity «مجوز» را نمی‌فهمد: هر کاربر و هر نقش یک شناسهٔ بی‌معنا به نام `FkPermissionAssigneeId` دارد که کلید اتصال به Authorization است؛ آنجا جدول `PermissionAssignee` و `Permission` می‌گوید این شناسه روی کدام منبع (`Resource`) و کدام عمل (`View`/`Create`/`Edit`/`Delete`) دسترسی دارد. پس Identity می‌گوید «این کاربر نقش `Admin` دارد» و Authorization می‌گوید «نقش `Admin` روی `identity.user` دسترسی Full دارد». `FkPartyId` هم اشاره‌ای به Party در People است، بدون اینکه Identity چیزی از شخص بداند.

## ۲. چه چیزهایی داخلش هست؟

- **موجودیت‌ها** — `ApplicationUser` (با `IsActive`، `IsLocked`، `LastLoginIp`، `FkPartyId`)، `ApplicationRole` (`Description`، `OrderNum`)، `RefreshToken` و `UserSession`. دو موجودیت اصلی `[SecuredResource("Identity.User")]` و `[SecuredResource("Identity.Role")]` دارند.
- **ورود و ساخت توکن** — `LoginCommand` → `AuthService.LoginAsync`: اگر ورودی `@` داشته باشد `FindByEmailAsync`، وگرنه `FindByNameAsync`؛ سپس `CheckPasswordSignInAsync(user, password, false)`. نقش‌ها از `IRoleInternalService.GetUserRolesAsync` گرفته می‌شوند و `JwtTokenService.GenerateTokensAsync` یک AccessToken با HMAC-SHA256 و Claimهای `sub`/`unique_name`/`email`/`ClaimTypes.Role` (پیش‌فرض ۶۰ دقیقه) به‌همراه یک RefreshToken تصادفی ۶۴ بایتی Base64 می‌سازد که در `identity.RefreshTokens` ذخیره می‌شود.
- **رفرش** — `RefreshTokenCommand` → `AuthService.RefreshTokenAsync`: با `RefreshTokenByValueSpec` توکن را می‌خواند، `IsRevoked` و `ExpiryDate` را چک می‌کند، جفت تازه می‌سازد و توکن قبلی را با `Revoke(newToken)` باطل می‌کند.
- **خروج** — `LogoutCommand` فقط همان RefreshToken را باطل می‌کند و `AuthController` Cookie را پاک می‌کند.
- **CRUD** — `CreateUserCommand` / `UpdateUserCommand` / `DeleteUserCommand` و معادل‌های نقش، به‌همراه Queryهای `GetUsersQuery`، `GetUserByIdQuery`، `GetRolesQuery` و دو SelectionList.
- **API** — `api/identity/Auth/{register,login,refresh,logout}`، `api/identity/Users/*` و `api/identity/Roles/*`. مسیرهای داده‌ای با `[AuthorizeResource("identity.user","View")]` محافظت می‌شوند.
- **Seed** — `IdentitySeedData` از `IdentityModuleInitializer` (یک `IHostedService`) اجرا می‌شود: نقش‌های `Admin` و `User` با یک PermissionAssignee از نوع Role، کاربر ادمین از روی `Seed:AdminEmail`/`Seed:AdminUserName`/`Seed:AdminPassword` (پیش‌فرض `admin@local` / `admin` / `ChangeMe123!`) و کاربر `intitializer` با رمز ثابت `Init@123456789` ساخته می‌شوند، نقش `Admin` به ادمین داده می‌شود، منابع `identity`/`identity.user`/`identity.role` با مجوز `Full` برای Assignee نقش ادمین ثبت می‌شوند و منوی «مدیریت حساب های کاربری» با زیرمنوهای `/Identity/Users` و `/Identity/Roles` در Navigation همگام می‌شود.

## ۳. چه چیزی داخلش **نیست** (و نباید انتظارش را داشت)

- بازیابی رمز، تأیید ایمیل، ورود دومرحله‌ای و OTP نیست.
- مدیریت سشن نیست: `UserSession` تعریف و در Migration ساخته می‌شود، اما هیچ کدی آن را پر نمی‌کند.
- ارزیابی مجوز اینجا نیست؛ `Permission`، `Scope` و `PermissionRule` در Authorization هستند.
- پروفایل شخص (کد ملی، آدرس، تلفن) اینجا نیست؛ در People است.
- انتشار رویداد نیست: `UserRegisteredEvent` تعریف شده و سه Handler دارد، اما هیچ‌جا publish نمی‌شود.

## ۴. یک نگاه فنی کوتاه

- `IdentityDbContext` از `IdentityDbContext<ApplicationUser, ApplicationRole, Guid, ...>` ارث می‌برد و **نه** از `Core.Infrastructure.Data.Base_DbContext`؛ فقط `IBase_DbContext` را دستی پیاده کرده و `EnsureView`/`EnsureTrigger` را کپی کرده است. نتیجه: `UpdateAuditableEntities` و پرکردن خودکار `CreatedBy`/`ModifiedBy` را ندارد.
- اسکیمای `identity`، تاریخچهٔ `__IdentityMigrationsHistory` و یک Migration به نام `20260903110049_Edit_1_Identity` که در واقع کل اسکیمای اولیه را می‌سازد (نامش گمراه‌کننده است).
- ثبت در DI با کنوانسیون پلتفرم: `Identity_AddApplication` (MediatR)، `Identity_AddDomain` (خالی)، `Identity_AddInfrastructure`، `Identity_AddPresentation`، `Identity_AddHealthChecks` (چک `IdentityDatabase`) و `Identity_UseInfrastructure` (اجرای Migration).
- `Identity_AddInfrastructure` برای گرفتن `IOutboxProcessorRegistration` وسط ثبت سرویس‌ها `services.BuildServiceProvider()` می‌زند؛ یعنی به ماژول Event وابسته است و `Order` در `Modules:Enabled` اهمیت دارد. جدول `OutboxMessages` هم ساخته می‌شود ولی هیچ کدی در Identity داخلش نمی‌نویسد.

## ۵. وضعیت واقعی امروز (صادقانه)

- **قرارداد عمومی جای غلطی است.** `IUserPublicService` و `IRolePublicService` در `Core.Application` هستند، پس مصرف‌کننده به Core وابسته می‌شود نه به Identity؛ امروز هیچ ماژول کسب‌وکاری به `Identity.Application` رفرنس نداده است.
- **رویداد ثبت‌نام هرگز منتشر نمی‌شود.** `AuthService` فضای‌نام `Identity.Shared.Events` را using می‌کند ولی هیچ `Publish` یا Outbox‌ای ندارد. نتیجه: `AssignDefaultRoleEventHandler` اجرا نمی‌شود و کاربر تازه‌ثبت‌نام‌شده هیچ نقشی نمی‌گیرد؛ Handlerهای خوش‌آمد Notification هم هرگز اجرا نمی‌شوند.
- **`Identity.Test` همین را ادعا می‌کند و سبز نیست.** `AuthServiceTests` انتظار دارد `IOutboxService<IdentityDbContext>.AddEventsAsync` با یک `UserRegisteredEvent` صدا زده شود، در حالی که `AuthService` حتی این وابستگی را ندارد.
- **قفل حساب تعریف شده ولی اعمال نمی‌شود.** `options.Lockout` تنظیم شده اما `CheckPasswordSignInAsync(..., lockoutOnFailure: false)` است؛ `IsActive`/`IsLocked` در مسیر ورود چک نمی‌شوند و `Lock/Unlock/Activate/Deactivate/UpdateLoginInfo` فراخوانی ندارند. کاربر غیرفعال هم توکن می‌گیرد و `LastLoginIp`/`LastLoginTime` خالی می‌ماند.
- **RefreshToken امنیت سشن ندارد.** `CreatedByIp` همیشه `"system"` و `DeviceInfo` همیشه `"unknown"` است، ایندکس `Token` یکتا نیست و استفادهٔ مجدد از توکن باطل‌شده خانوادهٔ توکن‌ها را باطل نمی‌کند.
- **`CreateUserAsync` نوع اشتباهی می‌سازد.** در `UserService.CreateUserAsync` آمده `CreatePermissionAssigneeAsync(AssigneeType.Role)` در حالی که باید `AssigneeType.User` باشد (در `AuthService.RegisterAsync` درست است).
- **کش به فیلترها توجه نمی‌کند.** `UserService.GetUsers` و `RoleService.getRoles` نتیجه را با کلید ثابت `identity:user:full` / `identity:role:full` کش می‌کنند، هرچند پارامتر جست‌وجو دارند؛ اولین درخواست فیلترشده کش بقیه را مسموم می‌کند.
- **SelectionList کاربر شناسهٔ اشتباه می‌دهد.** `GetUsersSelectionListQueryHandler` مقدار `Value` را `FkPermissionAssigneeId` می‌گذارد، نه `Id` کاربر؛ `GetUsers` هم روی `user.NickName.Contains(...)` بدون چک null کار می‌کند.
- **کد مرده کم نیست.** `UserDataContextProvider` داخل Identity فقط وقتی ثبت می‌شود که Authorization خاموش باشد؛ `UserLoggedInEvent` منتشرکننده ندارد؛ و `_personService`/`_positionService`/`_employmentService` در `UserService` فقط در کد کامنت‌شده استفاده شده‌اند.
- **وابستگی سخت به Navigation.** Seed همیشه `IMenuPublicService` را resolve می‌کند و برای Navigation نسخهٔ Null وجود ندارد؛ اگر Navigation فعال نباشد، Startup خطا می‌دهد. برای Authorization محافظ `ModuleHelper.IsActive(ModuleEnum.Authorization)` گذاشته شده است.
