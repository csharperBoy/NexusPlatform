# ماژول HR (ساختار سازمانی، استخدام‌ها، پست‌ها و مکان‌ها)

> **در یک جمله:** HR «نقشهٔ سازمان» است؛ می‌گوید چه واحدهایی وجود دارند، چه پست‌هایی در آن‌ها تعریف شده، چه کسانی با چه کد پرسنلی استخدام شده‌اند، از چه تاریخی تا چه تاریخی روی چه پستی نشسته‌اند و در کدام مکان‌ها مستقرند.

| | |
|---|---|
| **پروژه‌ها** | `HR.Domain` / `HR.Application` / `HR.Infrastructure` / `HR.Presentation` و ماژول افزونه `HR.IrisaSync.Extention` |
| **قرارداد عمومی** | `IEmploymentPublicService`، `IPostPublicService`، `ILocationPublicService` — مسیر واقعی: `Core.Application/Abstractions/HR/` (نه در `HR.Application`) |
| **وابستگی به ماژول‌های دیگر** | هنگام کامپایل به `People.Domain` و `People.Application` (بی‌استفاده) و `Core.*`؛ هنگام اجرا به `IPersonPublicService` (People) و `IContactPublicService` (Contact) از طریق Core |
| **چه کسانی از آن استفاده می‌کنند** | `Contact.Infrastructure` (‏`PhoneBookService`)، `Contact.Application` (دستورها و Queryهای تماس استخدام/پست/مکان)، `Authorization` و `Identity` (‏`UserDataContextProvider`، `UserService`)، `HR.IrisaSync.Extention` |
| **جایگاه پیشنهادی در مدل لایه‌ای** | لایه ۲ |

---

## ۱. این ماژول به چه درد می‌خورد؟ (به زبان ساده)

در یک سازمان، «کارمند» فقط یک نام نیست. یک نفر می‌تواند در دو واحد مختلف پست داشته باشد، پستش عوض شود، به یک مکان منتقل شود، یا هم‌زمان جانشین کسی باشد. HR این‌ها را با تفکیک «استخدام» از «انتصاب» مدل می‌کند:

- **`Employment`** — خودِ رابطهٔ استخدامی: `EmploymentCode` (کد پرسنلی)، `FkNaturalPersonId` (ارجاع به People)، `FkEmploymentTypeId`، `FkEmploymentStatusId`، بازهٔ `EffectiveFrom`/`EffectiveTo` و `FkContactProfileId`. با `ISoftRemovable` حذفش «نرم» است.
- **`Assignment`** — نشستن یک استخدام روی یک پست، با بازهٔ زمانی، `IsCurrent` و `AssigneeType` (`PostAssignmentType`: `Permanent`/`Delegation`). یعنی «چه کسی در فروردین ۱۴۰۳ روی این پست بود» قابل پاسخ است.
- **`Post`** — پست سازمانی: `Code`، `FkJobTitleId`، `FkOrganizationUnitId`، `FkJobLevelId`، `FkGradeId`، `FkCostCenterId`، `ParentId` (سلسله‌مراتب گزارش‌دهی)، `IsActive`، `FkPermissionAssigneeId` و `FkContactProfileId`.
- **`OrganizationUnit`** — واحد سازمانی با `Name`، `Code`، `ParentId` و `Path` (Materialized Path مثل `/root/parent/self/`) برای گرفتن سریع فرزندان.
- **`Location`** — مکان فیزیکی با سلسله‌مراتب، `IsRemove` و `FkContactProfileId`.
- **`EmploymentLocation` و `PostLocation`** — اتصال نسخه‌دار استخدام/پست به مکان؛ چون یک کارمند می‌تواند هم‌زمان در دو محل مستقر باشد.
- **داده‌های مرجع** — `JobTitle` (عنوان شغلی)، `JobLevel` (سطح شغلی)، `Grade` (طبقه/رتبه)، `CostCenter` (مرکز هزینه)، `EmploymentType` (تمام‌وقت/پاره‌وقت/پیمانی) و `EmploymentStatus` (فعال/آزمایشی/استعفا/بازنشسته).
- **Read Modelها** — `EmploymentInfoView` و `PostInfoView`: ویوهای SQL (`Employment_Info_View`، `Post_Info_View`) که در `HRDbContext.EnsureViews` با `EnsureView(...)` از اسکریپت‌های Embedded ساخته می‌شوند و Joinهای سنگین (شخص + استخدام + پست + واحد + عنوان شغلی) را یک‌جا می‌دهند. بدون این ویوها لیست‌های HR کار نمی‌کنند.

## ۲. چه چیزهایی داخلش هست؟

- **هفت سرویس در Infrastructure** — `EmploymentService`، `PostService`، `LocationService`، `OrganizationUnitService`، `JobTitleService`، `JobLevelService` و `HRUnitOfWork` (پیاده‌سازی `IHRUnitOfWork<HRDbContext>` با Repositoryهای تایپ‌شده).
- **الگوی «اینترفیس داخلی، اینترفیس عمومی»** — `IEmploymentInternalService : IEmploymentPublicService` و همین برای `IPostInternalService` و `ILocationInternalService`. متدهای نوشتن (`CreateEmploymentAsync`، `UpdatePostAsync`، `AssignToEmploymentAsync`، `DeleteAsync`) در نسخهٔ داخلی‌اند و قرارداد عمومی فقط خواندن دارد.
- **Command/Query با MediatR** — برای Employment، Post/OrgChart، Location، OrganizationUnit، JobTitle، JobLevel و Assignment؛ `Get*SelectionListQuery`ها برای Dropdownهای فرانت‌اند هستند.
- **Specification‌ها** — `ActiveAssignmentsByEmployment/Post/PostsSpec`، `GetEmploymentLocationsSpec`، `GetPostLocationsSpec`، `GetAllPostInfoViewSpec`، `OrganizationUnitByIdsSpec`، `OrganizationUnitsByParentSpec`، `PostsByUnitSpec`، `PostsByUnitManagerialSpec` و `GetEmploymentByPersonIdSpec`.
- **زیرساخت** — `HRDbContext` با اسکیمای `hr` و `__hrMigrationsHistory`، Outbox، `HR_AddApplication`، `HR_AddInfrastructure`، `HR_AddHealthChecks` (`HRDatabase`)، `HR_UseInfrastructure` و `ModuleInitializer` که منابع `hr.post`/`hr.employment`/`hr.location` و منوی «مدیریت منابع انسانی» را Seed می‌کند.
- **API** — سه کنترلر زیر `api/HR/...`: `EmploymentController`، `OrgChartController` (پست + Dropdownهای متادیتا) و `LocationController`.

### ۲.۱ افزونهٔ `HR.IrisaSync.Extention`

این پروژه نشان می‌دهد ماژول HR چطور بدون دست‌خوردن کد اصلی، به یک سیستم خارجی وصل می‌شود:

- **قرارداد نام‌گذاری:** نام پروژه `{Module}.{Ext}.Extention` است (توجه: `Extention` با `t`). لودر Reflection در `Core.Infrastructure/DependencyInjection/ModuleLoaderExtensions.cs` اسمبلی `HR.IrisaSync.Extention` را از روی همین الگو می‌سازد.
- **نقطه‌های ورود:** `IrisaSync_AddDependency(IServiceCollection, IConfiguration)` و `IrisaSync_UseDependency(IApplicationBuilder)` — به‌جای `HR_Add*`.
- **فعال‌سازی:** در `appsettings.json` داخل ورودی ماژول HR:

```json
{ "Name": "HR", "Order": 80,
  "Extentions": [ { "Name": "IrisaSync", "Order": 85 } ] }
```

- **دو DbContext:** `IrisaExtentionDbContext` (SQL Server، اسکیمای `hr`، تاریخچهٔ `__IrisaSyncMigrationsHistory`) برای جدول‌های نگاشت (`IrisaSyncJobTitleMap`، `IrisaSyncJobLevelMap`، `IrisaSyncOrganizationUnitMap`) و `IrisaOracleDbContext` برای خواندن از سیستم مبدأ (Oracle) با موجودیت `PdsIdeaInformationViw`.
- **منطق همگام‌سازی:** `SyncService` برای هر حوزه (Employment، Post، Assignment، JobTitle، JobLevel، OrganizationUnit) دو مرحله دارد: `Sync*PreviewAsync` که فقط `SyncCommandBundle<TCreate,TUpdate,TDelete>` می‌سازد (با `Summary` فارسی برای تأیید کاربر) و `Apply*Async` که همان Commandها را با `IMediator` روی خودِ HR اجرا می‌کند. یعنی افزونه هیچ‌وقت مستقیم در جداول HR نمی‌نویسد.
- **API:** `IrisaSyncController` (‏`Sync*Preview`/`Apply*`/`SyncWithIrisa`) و `MapController` (‏`FillJobTitleMap`/`FillJobLevelMap`/`FillOrganizationUnitMap`) زیر `api/HR/...`؛ به‌علاوهٔ Seed منابع و منوها در `IrisaExtentionSeedData`.

## ۳. چه چیزی داخلش **نیست** (و نباید انتظارش را داشت)

- محاسبهٔ حقوق، حضور و غیاب، مرخصی و ارزیابی عملکرد ندارد. HR فقط «ساختار و انتصاب» است.
- اعتبارسنجی تداخل زمانی ندارد؛ دو انتساب هم‌زمان روی یک پست از نظر دیتابیس مجاز است (بخش ۵).
- تاریخچهٔ تغییرات پست/عنوان شغلی ندارد (فقط انتساب‌ها و مکان‌ها بازه‌دارند).
- حذف فیزیکی و گردش کار تأیید (Approval) ندارد؛ همه‌چیز `SoftRemove` + `DoExpire` است و تغییر بلافاصله اعمال می‌شود.
- مجوز سطح پست عملاً کار نمی‌کند (بخش ۵).

## ۴. یک نگاه فنی کوتاه

- **جهت وابستگی‌ها در csproj:** `HR.Domain` → `People.Domain`، `HR.Application` → `People.Application`، `HR.Infrastructure` → `People.Infrastructure`، `HR.Presentation` → `People.Presentation`. اما در هیچ‌کدام از این چهار پروژه **یک خط `using People.*` وجود ندارد**؛ تنها استفاده از People، `using Core.Application.Abstractions.People` است. یعنی این چهار رفرنس وابستگی واقعی نیستند و فقط گراف Build را شلوغ می‌کنند.
- `HR.Domain/DependencyInjection` متد را `Sample_AddDomain` نام گذاشته (کپی از ماژول Sample)، پس `HR_AddDomain` هرگز پیدا نمی‌شود؛ خوشبختانه بدنه‌اش خالی است.
- **Save سه‌گانه:** `EmploymentService.SaveAsync` هم `_uow.SaveChangesAsync()` (HR)، هم `_contactService.SaveAsync()` (Contact) و هم `_personService.SaveAsync()` (People) را صدا می‌زند. `PostService.SaveAsync` هم `_hrUow` و `_uow` را پشت سر هم Save می‌کند در حالی که هر دو به یک `HRDbContext` اشاره می‌کنند. تراکنش توزیع‌شده‌ای وجود ندارد.
- **کد تکراری در خود HR:** `GetEmploymentListAsync` و `GetFullInfoAsync` هر دو `EmployementInfoViewRepository` را می‌خوانند و تقریباً همان نگاشت را دوباره نوشته‌اند؛ `PostService.GetPostListAsync` و `GetByContactProfileIds` هم همین‌طور. همچنین پنج اینترفیس داخلی (`IPost*`، `IEmployment*`، `IJobTitle*`، `IJobLevel*`، `IOrganizationUnit*`) هر کدام **دو بار** ثبت می‌شوند (یک‌بار با factory و یک‌بار مستقیم).
- **Create بدون مجوز:** در هر سه کنترلر، `[AuthorizeResource(... , "Create")]` روی متدهای `Create` **کامنت شده** است؛ فقط `View`/`Edit`/`Delete` مجوز دارند.

## ۵. وضعیت واقعی امروز (صادقانه)

- **`Post.FkPermissionAssigneeId` هرگز مقدار نمی‌گیرد.** این فیلد فقط خوانده می‌شود (`GetEmploymentPostsPermissionAssigneeId` و `RemovePostEvent`)، ولی نه سازندهٔ `Post` آن را می‌گیرد و نه `CreatePostAsync`. نتیجه: این متد همیشه `Guid.Empty` برمی‌گرداند و هر مجوزی که به «پست» وصل شود بی‌اثر است.
- **تنها یک استخدام می‌تواند روی یک پست بنشیند.** `AssignToEmploymentAsync` همهٔ انتساب‌های فعال **افراد دیگر** روی همان پست را `DoExpire` می‌کند. این برای «پست یک‌نفره» درست است، ولی برای پست چندنفره (مثل «کارشناس قراردادها» با سه نفر) مدل را می‌شکند و این محدودیت هیچ‌جا مستند نشده است.
- **`PostService.AssignToEmploymentAsync` بدون `SaveAsync` برمی‌گردد.** متد `UpdateAsync` صدا می‌زند ولی ذخیره نمی‌کند؛ اگر فراخواننده یادش برود، تغییر از دست می‌رود. همین الگو در `AssignLocationsToEmployment` هم هست.
- **`UpdateEmploymentAsync` مرز تراکنش ندارد ولی سه مسئولیت دارد:** استخدام + هویت شخص (People) + تماس (Contact). یک متد، سه Aggregate، سه DbContext.
- **وابستگی `HR.Infrastructure` → `People.Infrastructure`** از نظر لایه‌بندی غلط است: Infrastructure نباید به Infrastructure ماژول دیگر وصل شود. اگر روزی People.Infrastructure عوض شود، HR.Infrastructure بدون هیچ استفادهٔ واقعی می‌شکند.
- **`Assignment` بدون اعتبارسنجی هم‌پوشانی:** `EnsureTriggers` کامنت شده و تریگر `trg_Assignments_CheckOverlap` ساخته نمی‌شود؛ سه Setter مربوط به `IHasEffectivePeriod` هم برای انتساب‌ها صدا زده نمی‌شود و `DoExpire` مستقیم فیلدها را عوض می‌کند.
- **افزونه، اعتبارنامهٔ Oracle را Hard-code کرده:** `options.UseOracle("User Id=TPOUT_PDS;Password=irisatpout;Data Source=//192.168.7.5:1521/prod;")` در `IrisaSync_AddDependency`؛ این باید از `ConnectionStrings`/Secret Store بیاید. همچنین `ApplyEmploymentsAsync` افزودن/ویرایش را Batch می‌کند ولی حذف را تک‌تک، پس عملیات می‌تواند نیمه‌کاره بماند.
- **HR.Test وجود ندارد**، در حالی که منطق انتصاب و بازه‌های زمانی بدون تست است.
