# ماژول Navigation (درخت منو / ناوبری)

> **در یک جمله:** Navigation تنها جایی است که ساختار منوهای پنل نگه داشته می‌شود؛ هر ماژول منوی خودش را «به‌صورت کد» تعریف می‌کند و Navigation آن را در یک جدول مرکزی درج/به‌روزرسانی می‌کند تا فرانت‌اند یک درخت واحد بگیرد.

| | |
|---|---|
| **پروژه‌ها** | `Navigation.Domain` / `Navigation.Application` / `Navigation.Infrastructure` / `Navigation.Presentation` — به‌علاوهٔ یک کپی مردهٔ غلط‌املایی‌شده: `Navigation.Infrustructure` |
| **قرارداد عمومی** | `IMenuPublicService.SyncModuleMenusAsync(...)` — اما در `Core.Application/Abstractions/Navigation/PublicService/IMenuPublicService.cs` تعریف شده، نه در `Navigation.Application` |
| **وابستگی به ماژول‌های دیگر** | Authorization و Identity (فقط هنگام Seed: منابع، مجوزها، نقش ادمین)، Cach (`ICachePublicService`) |
| **چه کسانی از آن استفاده می‌کنند** | Seed ماژول‌های Audit، Authorization، Contact، HR، HR.IrisaSync.Extention، Identity، Scheduler و Trader؛ فرانت‌اند از طریق `GET /api/navigation/menu/GetMenuTreeQuery` |
| **جایگاه پیشنهادی در مدل لایه‌ای** | لایه ۱ (سرویس‌دهنده به بقیه، بدون این‌که به دامنهٔ آن‌ها وابسته باشد) |

---

## ۱. این ماژول به چه درد می‌خورد؟ (به زبان ساده)

هر ماژول صفحه‌هایی دارد و این صفحه‌ها باید در منوی سمت راست پنل دیده شوند. اگر هر ماژول مستقیم روی جدول منو بنویسد، هر کسی می‌تواند ساختار کل منو را خراب کند و هیچ‌کس هم صاحب آن نیست.

ایدهٔ «منو به‌صورت کد» (menus-as-code) این است: **مالک منو، خودِ ماژول است، ولی محل نگهداری منو، Navigation است.** هر ماژول در همان کلاس Seed خودش یک لیست `List<MenuDto>` می‌سازد (مثلاً `Audit.Infrastructure/Data/AuditSeedData.cs` → `GetAuditMenuDefinitions()`)، و در `ModuleInitializer` خودش آن را به `IMenuPublicService.SyncModuleMenusAsync` می‌دهد. Navigation هر آیتم را با `Key` پیدا می‌کند: اگر نبود ایجاد، اگر بود و تغییری داشت به‌روزرسانی.

چرا هر ماژول باید منوی خودش را ثبت کند؟ سه دلیل ساده:

1. **مالکیت:** اگر منوی «مدیریت لاگ» عوض شود، کسی که کد Audit را می‌نویسد تغییرش می‌دهد، نه یک فایل مرکزی که همه دستش می‌زنند.
2. **فعال/غیرفعال شدن ماژول:** اگر ماژول در `Modules:Enabled` نباشد، `ModuleInitializer` آن اجرا نمی‌شود، پس منوی‌اش هم ثبت نمی‌شود. منو خودکار با ماژول می‌آید و می‌رود.
3. **بدون Migration:** تغییر منو یک تغییر کد است، نه یک Insert دستی در دیتابیس.

## ۲. چه چیزهایی داخلش هست؟

- **`Menu`** — موجودیت درختی با `IHierarchicalStructureEntity<Menu, Guid?>`: `Title`، `Key` (یکتا، Trim + lowercase در سازنده)، `Description`، `Path`، `Icon` (enum از `Core.Shared.Enums.Navigation`)، `Order` (nullable)، `ParentId`/`Parent`/`Children`. همچنین `IAuditableEntity`، `IOwnerableEntity` و `IAggregateRoot` را پیاده می‌کند و `[SecuredResource("Navigation.Menu")]` دارد.
- **`MenuConfiguration`** — جدول `navigation.Menu` با ایندکس یکتا روی `Key` و `HasMaxLength(100).IsUnicode(false)`.
- **`MenuByKeySpec`** — یک Specification که `Parent` و `Children` را Include می‌کند.
- **`MenuService`** — پیاده‌سازی `IMenuInternalService` (که خودش از `IMenuPublicService` ارث می‌برد). سه کار می‌کند: `SyncModuleMenusAsync`، `GetByTreeStructure`، `GetMenus`.
- **`MenuProcessor`** — `Flatten` (درخت → لیست خطی والد قبل از فرزند) و `BuildTree` (لیست → درخت، مرتب‌شده با `Order` سپس `Title`).
- **API** — `GET /api/navigation/menu/GetMenuTreeQuery` با `[AuthorizeResource("navigation.menu", "View")]` که `GetMenuTreeQuery` را به MediatR می‌دهد.
- **Seed** — `NavigationSeedData` منبع `navigation.menu` و مجوز Full برای نقش ادمین را ثبت می‌کند (`GetAdminRolePermissionAssigneeIdAsync`). متد `SeedNavigationAsync` وجود دارد اما **تمام بدنه‌اش کامنت است**.
- **Migration و Health Check** — اسکیمای `navigation`، جدول تاریخچهٔ `__NavigationMigrationsHistory`، Health Check با نام `NavigationDatabase`.

## ۳. چه چیزی داخلش **نیست** (و نباید انتظارش را داشت)

- **حذف منو ندارد.** `SyncModuleMenusAsync` فقط Create/Update می‌کند؛ اگر یک منو را از کد حذف کنی، ردیفش در دیتابیس می‌ماند. «همگام‌سازی» یک‌طرفه است.
- **مجوز نمی‌سازد.** Navigation فقط منو است؛ اینکه چه کسی کدام منو را ببیند کار Authorization است. منو خودش فیلتر دسترسی ندارد.
- **مرتب‌سازی سمت دیتابیس ندارد.** `GetByTreeStructure` کل جدول را با `GetAllAsync()` می‌خواند و در حافظه درخت می‌سازد.
- **کش فعال ندارد.** کد کش در `GetByTreeStructure` کامنت شده؛ فقط `InvalidateCachesAsync` صدا زده می‌شود (بی‌ضرر، ولی بی‌فایده).
- **ویرایش منو از طریق API وجود ندارد.** فقط خواندن؛ منو فقط از کد می‌آید.

## ۴. یک نگاه فنی کوتاه

- `NavigationDbContext : Base_DbContext` با `HasDefaultSchema("navigation")`، `DbSet<Menu>` و `DbSet<OutboxMessage>`. یعنی ماژول Outbox دارد و `IOutboxProcessorRegistration` هم برایش ثبت می‌شود — اما هیچ Domain Event‌ای تولید نمی‌کند (خط `AddDomainEvent` در `ChangeParent` کامنت است).
- ثبت با کنوانسیون پلتفرم: `Navigation_AddInfrastructure` (DbContext + `MenuService` + `IMenuInternalService` + `IMenuPublicService` + `IRepository`/`ISpecificationRepository`/`IUnitOfWork` + `AddHostedService<ModuleInitializer>`)، `Navigation_AddApplication` (MediatR)، `Navigation_AddPresentation` (کنترلر + MediatR)، `Navigation_AddHealthChecks`، `Navigation_UseInfrastructure` (اجرای Migration با `IMigrationManager`).
- `ModuleInitializer` اول Seed مجوز/منبع، بعد Seed داده را صدا می‌زند و **در صورت خطا `throw` می‌کند** → یعنی خطای Seed باعث Fail شدن استارتاپ می‌شود.
- در `SyncModuleMenusAsync` هر آیتم در یک `try/catch` است، لاگ می‌زند و **دوباره throw می‌کند**؛ پس یک منوی خراب، کل Sync را متوقف می‌کند.

## ۵. وضعیت واقعی امروز (صادقانه)

### ۵.۱. کپی تکراری `Navigation.Infrustructure` — مرده است، حذفش کنید

دو پوشهٔ Infrastructure با نام‌های `Navigation.Infrastructure` و `Navigation.Infrustructure` (با یک «r» جاافتاده) وجود دارد. بررسی کردم:

- **فقط `Navigation.Infrastructure` در `NexusPlatform.sln` است** (خط ۱۷۸). `Navigation.Infrustructure` هیچ‌جا در Solution نیست.
- **هیچ `.csproj` یا فایل کدی در کل مخزن به `Navigation.Infrustructure` رفرنس نمی‌دهد.** جست‌وجوی رشته‌ای روی `*.sln`، `*.csproj`، `*.cs` و `*.json` صفر نتیجه داد.
- **بارگذار ماژول‌ها اسمبلی را با نام می‌سازد:** `ModuleLoaderExtensions.FindModuleMethod` دقیقاً `Assembly.Load($"{moduleName}.Infrastructure")` را صدا می‌زند. یعنی فقط `Navigation.Infrastructure` قابل بارگذاری است.
- **کپی غلط، قدیمی و غیرقابل کامپایل است:** Migration آن `20260630104219` است در حالی که نسخهٔ واقعی `20260903110137` دارد. مهم‌تر: کد آن `Menu.FkParentId` را صدا می‌زند، ولی `Navigation.Domain/Entities/Menu.cs` امروز فقط `ParentId` دارد → این پروژه در بهترین حالت خطای کامپایل می‌دهد. همچنین نام Health Check آن `"BaseDatabase"` و لاگ‌هایش «Base module initialization» است (نشانهٔ کپی از قالب Base) و گارد Seed آن روی `ModuleEnum.Authorization` است نه `Navigation`.

نتیجه: `Navigation.Infrustructure` یک نسخهٔ قدیمی/موازی است که هیچ نقشی در Build و اجرا ندارد. توصیه: پوشه حذف شود؛ نگه‌داشتنش فقط باعث می‌شود نفر بعدی گمراه شود و ابزارهای جست‌وجو نتایج تکراری بدهند. (من به کد دست نزدم — این فقط گزارش است.)

### ۵.۲. بقیهٔ بدهی‌ها

- **قرارداد عمومی جای غلطی است.** `IMenuPublicService` در `Core.Application` است نه `Navigation.Application`؛ پس هر ماژولی که منو ثبت می‌کند، در واقع به Core وابسته می‌شود. این همان الگوی «نشتی قرارداد به Core» است.
- **آیکون Seedها در دیتابیس گم می‌شود.** در Seedها `Icon = Icon.Folder.GetIconString()` نوشته می‌شود که مقدار `"fa-solid:folder"` می‌دهد، اما `MenuService` آن را با `def.Icon.ToEnumOrDefault(Icon.Default)` می‌خواند و این متد با `Enum.TryParse` روی **نام** enum کار می‌کند. `"fa-solid:folder"` یک نام معتبر نیست → همهٔ منوها با `Icon.Default` ذخیره می‌شوند. برای درست شدن، Seed باید نام enum بدهد (`Icon.Folder.ToString()` = `"Folder"`) یا `MenuService` باید Description را ترجمه کند.
- **`GetMenus(string? title)` پارامتر `title` را نادیده می‌گیرد** و همهٔ منوها را بدون فیلتر و بدون `Children` برمی‌گرداند.
- **`IMenuPublicService` دو پیاده‌سازی هم‌زمان ندارد** ولی `MenuService` سه بار در DI ثبت می‌شود (`MenuService`، `IMenuInternalService`، `IMenuPublicService`) — درست است، فقط کمی گیج‌کننده.
- **منو حذف نمی‌شود** (بخش ۳)؛ اگر روزی منویی را از کد پاک کنی، باید دستی ردیفش را در اسکیمای `navigation` پاک کنی.
- **`NavigationSeedData.SeedNavigationAsync` کاملاً کامنت است** ولی در `ModuleInitializer` صدا زده می‌شود — یک فراخوانی به متدی که هیچ کاری نمی‌کند.
- **کش غیرفعال شده ولی invalidate می‌شود.** `ICachePublicService` تزریق شده و `RemoveByPatternAsync("navigation:menu:*")` اجرا می‌شود؛ چون هیچ‌چیز کش نمی‌شود، این فقط یک رفت‌وبرگشت اضافه به Redis است.
