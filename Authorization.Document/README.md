# ماژول Authorization (منابع، مجوزها و محدودهٔ داده)

> **در یک جمله:** Authorization به یک سؤال جواب می‌دهد: «چه کسی، روی چه چیزی، چه کاری، و چقدر از آن را می‌تواند انجام دهد؟» — و پاسخش را هم به‌صورت یک چک ساده (`bool`) می‌دهد و هم به‌صورت فیلتری که روی کوئری‌های EF می‌نشیند.

| | |
|---|---|
| **پروژه‌ها** | `Authorization.Domain` / `Authorization.Application` / `Authorization.Infrastructure` / `Authorization.Presentation` |
| **قرارداد عمومی** | `IResourcePublicService` و `IPermissionPublicService` (در حال حاضر داخل `Core.Application` تعریف شده‌اند) |
| **وابستگی به ماژول‌های دیگر** | Identity، People، HR (برای ساختن Snapshot کاربر) و Navigation (فقط هنگام Seed منو) |
| **چه کسانی از آن استفاده می‌کنند** | `Core.Presentation.Filters.AuthorizeResourceFilter`، `EfRepository` / `EfSpecificationRepository` و ModuleInitializer تقریباً همهٔ ماژول‌ها (Audit، Identity، Navigation، People، HR، Contact، Scheduler، Trader، Base) |
| **جایگاه پیشنهادی در مدل لایه‌ای** | لایه ۱ (از Identity و People/HR خدمات می‌گیرد، به بقیه مجوز می‌دهد) |

---

## ۱. این ماژول به چه درد می‌خورد؟ (به زبان ساده)

در سیستم‌های معمولی می‌گوییم «این کاربر نقش Admin دارد، پس هر کاری می‌کند». این کافی نیست. سؤال واقعی: کاربر قراردادهای *همهٔ* واحدها را می‌بیند یا فقط واحد خودش؟ قرارداد بالای یک میلیارد را می‌تواند ویرایش کند؟

Authorization این چهار تکه را از هم جدا می‌کند:

1. **Resource** — «آن چیز» که تحت کنترل است: صفحهٔ UI، دکمه، یا جدول داده. منابع درختی‌اند (`authorization` ← `authorization.permission`) و کلیدشان (`Key`) شناسهٔ اصلی است، نه `Id`.
2. **PermissionAction** — «چه کاری»: `View`، `Create`، `Edit`، `Delete`، `Export` و `Full` یعنی همه‌چیز.
3. **Scope** — «چقدر از آن»: `None`، `Account`، `Self`، `Unit`، `UnitAndBelow`، `SpecificProperty` و `All`. این همان چیزی است که RBAC ساده ندارد.
4. **PermissionAssignee** — «چه کسی»: ردیفی مستقل با `AssigneeType` از نوع `Party`، `Post`، `Role` یا `User`. Permission به `FkPermissionAssigneeId` وصل می‌شود، نه مستقیم به `RoleId` یا `UserId`؛ همین indirection اجازه می‌دهد یک مجوز به هر چهار نوع سوژه بچسبد.

و لایهٔ پنجم: **PermissionRule** — شرط‌های ریز روی یک مجوز؛ مثلاً «Edit روی `hr.employment`، اما فقط رکوردهایی که `FieldName = ContractType` و `Value = 2`» با `ComparisonOperator` (`Equal`/`NotEqual`/`GreateThan`/`LessThan`) و `GroupOrder` برای گروه‌بندی. اگر شرط به جدول دیگری نیاز دارد، `JoinEntity` + `JoinLocalKey` + `JoinForeignKey` مسیر navigation را می‌دهد و `RowLevelSecurityProcessor` آن را با Expression Tree به `Any(...)` تبدیل می‌کند.

پس مدل می‌شود **Resource + Action + Scope + Assignee + Rule** — چیزی فراتر از RBAC که فقط `(Role, Resource, Action)` را می‌شناسد.

## ۲. چه چیزهایی داخلش هست؟

- **موجودیت‌ها** — `Resource` (درختی، با `ResourcePath` که خودکار از والد ساخته می‌شود)، `Permission`، `PermissionAssignee`، `Scope` و `PermissionRule`. هر پنج `[SecuredResource]` دارند تا خودشان هم قابل کنترل باشند.
- **Processors** — `AuthorizationProcessor` (چک `bool` با کش ۱۰ دقیقه‌ای)، `RowLevelSecurityProcessor<TEntity>` (`ApplyFilter`، `CheckPermissionAsync`، `SetOwnerDefaults`)، `ResourceProcessor` (`BuildTree`/`FlattenResources`) و `ScopeProcessor` که خالی است.
- **سرویس‌ها** — `ResourceService`، `PermissionService`، `ScopeService`، `UserDataContextProvider` (Snapshot کاربر را از Identity/People/HR می‌سازد) و `ResourceMetadataProvider` (فیلدها و Joinهای هر موجودیت را با Reflection از مدل EF می‌کشد).
- **Application و API** — Command/Query و Handler منابع، مجوزها و DataScope؛ `ResourceHierarchyChangedEventHandler`؛ و کنترلرهای `api/authorization/admin/[controller]` و `api/authorization` (`check`، `my-permissions`).
- **Seed + Migration + HealthCheck** — اسکیمای `authorization`، جدول `__AuthorizationMigrationsHistory`، چک سلامت `AuthorizationDatabase` و `ModuleInitializer` که هنگام بالا آمدن اپ منابع و مجوزهای پیش‌فرض را می‌نویسد.

## ۳. چه چیزی داخلش **نیست** (و نباید انتظارش را داشت)

- **احراز هویت نیست.** لاگین، توکن و Claims کار Identity است؛ Authorization فقط `ClaimTypes.NameIdentifier` را می‌خواند.
- **UI نیست.** اینکه دکمه‌ای نمایش داده شود یا نه تصمیم FrontEnd است؛ اینجا فقط پاسخ `true/false` تولید می‌شود.
- **Row Level Security در سطح دیتابیس نیست.** هیچ Policy یا View امنیتی در SQL Server ساخته نمی‌شود؛ فیلتر با ترکیب `IQueryable` در EF انجام می‌شود و `FromSql`/Dapper از آن عبور نمی‌کند.
- **Field-Level Masking و مدیریت منو نیست.** فیلدهای `hasFieldBaseCondition` و `hasRelationBaseCondition` روی `Resource` هستند ولی هیچ‌جا خوانده نمی‌شوند؛ منوها هم مال Navigation است.
- **ابطال کش توزیع‌شده ندارد.** ابطال با الگوی کلید + TTL انجام می‌شود، نه با رویداد سراسری.

## ۴. یک نگاه فنی کوتاه

- **نقطهٔ اعمال (enforcement point)** ریشه‌ای: `EfRepository` و `EfSpecificationRepository` هر خواندن را از `IRowLevelSecurityProcessor<TEntity>.ApplyFilter` و هر نوشتن را از `CheckPermissionAsync` + `SetOwnerDefaults` عبور می‌دهند؛ یعنی موجودیتی که از Repository رد شود خودکار فیلتر می‌شود — به شرط داشتن `[SecuredResource]`.
- **نقطهٔ چک در Presentation**: `[AuthorizeResource("module.resource", "View")]` از `Core.Presentation.Filters`؛ یک `TypeFilterAttribute` که `AuthorizeResourceFilter` را می‌سازد: اول `SetUserData`، بعد `IAuthorizationProcessor.CheckAccessAsync`، و در نهایت `UnauthorizedResult` (۴۰۱) یا `ForbidResult` (۴۰۳).
- **ثبت در DI با کنوانسیون نام**: `Authorization_AddApplication`، `Authorization_AddInfrastructure`، `Authorization_AddHealthChecks` و `Authorization_UseInfrastructure`؛ لودر با Reflection آن‌ها را از `Modules:Enabled` پیدا می‌کند. Authorization با `Order = 34` بعد از Core/Identity/Base/Audit/Navigation و قبل از People/HR سوار می‌شود.
- **Null Object**: `Authorization_NullInject` در `Core_AddApplication` نسخهٔ Null همه‌چیز را ثبت می‌کند؛ چون Authorization بعد از Core ثبت می‌شود، آخرین ثبت برنده است و پیاده‌سازی واقعی جای Null می‌نشیند.

## ۵. وضعیت واقعی امروز (صادقانه)

این ماژول ۱۵۳ فایل و حدود ۱۰٫۴ هزار خط دارد، اما بخش قابل توجهی از آن **کامنت شده** است. اگر روی نسخهٔ کامنت‌شده حساب کنی، رفتار سیستم را اشتباه می‌فهمی:

- **کلاس‌های کاملاً کامنت‌شده:** `PermissionEvaluator`، `AuthorizationService`، `DataScopeEvaluator`، `DataScopeProcessor`، `DataScopeService`، `PermissionRuleService`، `ResourceTreeBuilder`، `SeedService`، `JoinDetail` و enumهای Domain. `AuthorizeResourceAttribute` لایهٔ Application هم کامنت است و فقط نسخهٔ `Core.Presentation` زنده است.
- **قرارداد عمومی سرِ جای غلط:** `IResourcePublicService`، `IPermissionPublicService` (و `IScopePublicService` که خالی است و `IPermissionRulePublicService` که کامنت شده) در `Core.Application` هستند، نه در `Authorization.Application`؛ پس مصرف‌کننده به Core وابسته می‌شود. `IPermissionChecker` و `IAuthorizationChecker` هم فقط **فایل** دارند و بدنه‌شان کامنت است.
- **Seed سه‌بار نوشته شده:** در `AuthorizationSeedData` سه پیاده‌سازی موازی داریم (`SeedResourcesAsync`، `ProcessResourceDefinition`، `SeedResourcesBFSAsync`) که فقط اولی صدا زده می‌شود. `GetResourceDefinitions` برای فرزندان `ParentKey` ست نمی‌کند و `FlattenResourceTree` هم آن را پر نمی‌کند؛ پس seed خود Authorization همهٔ ۵ منبع را **ریشه** می‌سازد، در حالی که مسیر مصرف‌کننده‌ها (`SyncModuleResourcesAsync` + `ResourceProcessor.FlattenResources`) درست کار می‌کند.
- **کلیدهای بدون مجوز و DataScope توخالی:** seed مجوز Full را فقط روی `authorization.resource`، `.permission`، `.permissionRule` و `.scope` می‌دهد، اما `DataScopesController` روی `authorization.datascopes` و `authorization` چک می‌کند؛ پس حتی ادمین ۴۰۳ می‌گیرد. هر چهار Handler زندهٔ DataScope هم فقط لاگ می‌زنند و `Ok(Guid.Empty)` برمی‌گردانند. `CheckPermissionQuery` هم Handler ندارد، پس `POST /api/authorization/check` کار نمی‌کند.
- **Scope عملاً بی‌اثر است:** `UserPermissionsSpec` مجموعهٔ `Scopes` را Include نمی‌کند و Lazy Loading روشن نیست؛ پس `PermissionDto.Scopes` همیشه خالی است و `CalculateScopeFromList` روی شاخهٔ «Scopes خالی ⇒ `ScopeType.All`» می‌افتد. نتیجه: هر کسی یک Allow داشته باشد عملاً Scope=All می‌گیرد و فیلتر سطری اعمال نمی‌شود. `SeedRolePermissionsAsync` هم اصلاً ردیف `Scope` نمی‌نویسد.
- **باگ‌های منطقی:** در `IsEntityInScope` حالت `Self` مقدار `OwnerPersonId` را با `userId` مقایسه می‌کند (باید `personId` باشد)؛ `UnitAndBelow` مثل `Unit` رفتار می‌کند؛ `ApplyScope` برای `Account` و `SpecificProperty` شاخه‌ای ندارد و به `Where("false")` می‌افتد؛ `BuildFilterExpression` مقدار `LogicalOperator` را نادیده می‌گیرد.
- **ابطال کش ناقص:** `ResourceHierarchyChangedEventHandler` کلید `auth:resourcetree:full` را پاک می‌کند، در حالی که `ResourceService` با `auth:resource:tree:full` کار می‌کند. `InvalidatePermissionCachesAsync` هم کلیدها را با `assigneeId` می‌سازد، در حالی که کلیدهای `auth:access:*` با `UserPermissionAssigneeId` ساخته می‌شوند؛ پس تغییر مجوز یک نقش کش کاربر را پاک نمی‌کند و تا ۱۰ دقیقه Allow/Deny قدیمی سرو می‌شود.
- **هزینهٔ هر درخواست:** هر درخواست دارای `[AuthorizeResource]` یک‌بار در فیلتر و یک‌بار در `SetUserDataBehavior`، `BuildAsync` را صدا می‌زند؛ هر بار چند فراخوانی بین‌ماژولی (Identity، People، HR) و یک کوئری مجوز با Includeهای `Resource`/`PermissionAssignee`/`Rules`. `ApplyFilter` هم به ازای هر خواندن Repository یک `_scope.Permissions.ToList()` می‌سازد. کش ۱۰ دقیقه‌ای روی چک کمک می‌کند، ولی Snapshot کاربر کش نمی‌شود.
- **جزئیات کوچک‌تر:** `Authorization_AddDomain` وجود ندارد (تابع Domain هنوز `Sample_AddDomain` است)؛ `Authorization_AddInfrastructure` وسط ثبت سرویس‌ها `BuildServiceProvider()` می‌زند؛ `IPermissionInternalService` و `IResourceInternalService` دو بار ثبت می‌شوند.
