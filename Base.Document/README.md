# ماژول Base (تنظیمات پایه — در واقع یک اسکلت نیمه‌خالی)

> **در یک جمله:** Base امروز تقریباً هیچ چیزی ندارد؛ نه موجودیت، نه سرویس دامنه، نه جدول واقعی. تنها چیز ارزشمندش یک Endpoint است که فهرست ماژول‌های فعال (`Modules:Enabled`) را به فرانت‌اند می‌دهد تا منو و Routeها را فیلتر کند.

| | |
|---|---|
| **پروژه‌ها** | `Base.Domain` / `Base.Application` / `Base.Infrastructure` / `Base.Presentation` |
| **قرارداد عمومی** | ندارد. نه `IBasePublicService` وجود دارد و نه چیزی مشابه آن؛ نقطهٔ تماس واقعی، یک Endpoint HTTP است |
| **وابستگی به ماژول‌های دیگر** | Core ( چارچوب ماژول‌ها، `ModuleHelper`)، Authorization و Identity (فقط هنگام Seed، آن هم با لیست خالی) |
| **چه کسانی از آن استفاده می‌کنند** | فرانت‌اند: `FrontEnd/src/core/api/settingApi.ts` و `ModuleContext.tsx` از `GET /api/base/setting/GetActiveModules` استفاده می‌کنند |
| **جایگاه پیشنهادی در مدل لایه‌ای** | لایه ۰ (سرویس‌دهنده به بقیه) — ولی امروز فقط در حد اسم |

---

## ۱. این ماژول به چه کار می‌آید؟ (به زبان ساده)

اسم Base تداعی می‌کند که قرار است «تنظیمات پایه»، «داده‌های مرجع» یا «سرویس مشترک» همهٔ ماژول‌ها را نگه دارد. اما اگر کد را باز کنی، چیزی از این‌ها نیست.

آن‌چه واقعاً وجود دارد این است: یک کنترلر با یک متد که `ModuleHelper.GetActiveModules()` را صدا می‌زند و فهرست ماژول‌های فعال را برمی‌گرداند. فرانت‌اند این فهرست را می‌گیرد (`ModuleContext.tsx`) و در یک `Set<string>` نگه می‌دارد تا فقط Routeها و ویجت‌های ماژول‌های فعال را نشان دهد. همین یک قابلیت، تنها دلیل زنده بودن Base است.

بقیهٔ چیزها عنوان دارند ولی محتوا ندارند: `BaseSeedData` دو متد دارد که یکی لیست منابع خالی و دیگری لیست مجوز خالی برمی‌گرداند، و `SeedBaseAsync` تماماً کامنت است.

## ۲. چه چیزهایی داخلش هست؟

- **`Base.Domain`** — فقط `DependencyInjection/ServiceCollectionExtensions.cs`. هیچ موجودیت، Value Object، Event یا Specification ندارد.
- **`Base.Application`** — فقط `Base_AddApplication` که MediatR را ثبت می‌کند. هیچ Command، Query، Handler یا Validator ندارد.
- **`BaseDbContext`** — یک DbContext با اسکیمای `base` که **تنها** `DbSet<OutboxMessage>` دارد. Migration `20260720081845_Edit_1_Base` فقط جدول `base.OutboxMessages` و ایندکس‌هایش را می‌سازد.
- **`BaseSeedData`** — `SeedBaseForAuthorizationAsync` با گارد `ModuleHelper.IsActive(ModuleEnum.Authorization)` و `SeedBaseAsync` که بدنه‌اش صد‌درصد کامنت است.
- **`ModuleInitializer`** — HostedService که همان دو متد بالا را صدا می‌زند و در خطا `throw` می‌کند.
- **`SettingController`** — `GET /api/base/setting/GetActiveModules`، بدون `[Authorize]` (خط `[AuthorizeResource("core.setting","View")]` کامنت شده) که `ModuleHelper.GetActiveModules()` را در یک `Result<List<ModuleItem>>` برمی‌گرداند.
- **Health Check** — `Base_AddHealthChecks` یک `AddDbContextCheck<BaseDbContext>("BaseDatabase")` ثبت می‌کند.
- **Migration** — اسکیمای `base` با تاریخچهٔ `__BaseMigrationsHistory`.

## ۳. چه چیزی داخلش **نیست**

- **هیچ موجودیتی مثل `Setting` / `AppSetting` / `Configuration`** وجود ندارد. «تنظیمات» در این پلتفرم همچنان از appsettings و `IOptions` می‌آید، نه از دیتابیس.
- **هیچ Command/Query‌ای برای خواندن یا نوشتن تنظیمات** وجود ندارد.
- **هیچ منو، منبع یا مجوزی** Seed نمی‌شود (لیست‌های تعریف‌شده خالی هستند)؛ پس در پنل صفحه‌ای برای Base وجود ندارد.
- **هیچ تست، `.Shared`، `.Test` یا `.Document`** (تا امروز) ندارد.

## ۴. یک نگاه فنی کوتاه

- ثبت با کنوانسیون: `Base_AddApplication`، `Base_AddInfrastructure`، `Base_AddDomain` (خالی)، `Base_AddPresentation`، `Base_AddHealthChecks`، `Base_UseInfrastructure` (اجرای Migration).
- `Base_AddInfrastructure` فقط DbContext + `IUnitOfWork<BaseDbContext>` + `AddHostedService<ModuleInitializer>` + پردازشگر Outbox برای `BaseDbContext` را ثبت می‌کند. **هیچ `IRepository` یا `ISpecificationRepository` ثبت نمی‌شود** (خط مربوطه کامنت است).
- Outbox برای Base ثبت می‌شود، اما Base هیچ Domain Event‌ای تولید نمی‌کند؛ یعنی این پردازشگر همیشه با جدول خالی می‌چرخد.
- `ModuleEnum` در `Core.Domain/Enums/ModuleEnum.cs` **عضوی به نام `Base` ندارد** (اعضا: Core, Audit, Authorization, Cach, Event, Identity, Notification, HR, People, WebScrapper, Navigation, Contact, StreamBus, Scheduler, Trader). پس نمی‌توان با `ModuleHelper.IsActive` فعال بودن Base را چک کرد — به همین دلیل Seed آن به `ModuleEnum.Authorization` گارد شده.

## ۵. وضعیت واقعی امروز (صادقانه)

بگذارید صریح بگویم: **Base امروز یک اسکلت نیمه‌خالی است.** سه لایه از چهار لایه‌اش (Domain، Application و بخش عمدهٔ Infrastructure) فقط قالب هستند. تنها بخشی که کد واقعی دارد `Base.Presentation/Controllers/SettingController.cs` است.

موارد مشخص:

- **`GetAuditResourceDefinitions()` و `GetBasePermissionDefinitions()` هر دو `new List<>()` خالی برمی‌گردانند.** اسم متد اول هم «Audit» است (کپی‌پیست از ماژول Audit). این دو متد در `ModuleInitializer` صدا زده می‌شوند و `SyncModuleResourcesAsync` با لیست خالی هیچ‌کاری نمی‌کند (حلقه روی آیتم‌ها خالی است و حذفی هم انجام نمی‌شود) — پس بی‌خطر ولی بی‌فایده‌اند.
- **`SeedBaseAsync` کاملاً کامنت است** ولی با `IUnitOfWork<BaseDbContext>` از DI گرفته می‌شود؛ یک فراخوانی به متدی که هیچ کاری نمی‌کند.
- **`BaseDbContext` فقط برای Outbox ساخته شده.** یک دیتابیس/اسکیما با یک جدول که هیچ‌کس در آن نمی‌نویسد. اگر Outbox لازم نیست، این DbContext و Migration آن قابل حذف است.
- **کنترلر مجوز ندارد.** `[AuthorizeResource("core.setting","View")]` کامنت شده و هیچ منبع `core.setting` هم Seed نمی‌شود. یعنی امروز هر کاربری (حتی بدون توکن، اگر مسیر در Gateway باز باشد) می‌تواند فهرست ماژول‌های فعال را بخواند. برای این داده حساس نیست، ولی الگوی بدی است.
- **`Base_AddPresentation` دوباره `AddControllers()` صدا می‌زند.** Core هم این کار را می‌کند؛ نتیجه‌اش کار می‌کند ولی ثبت تکراری و بار اضافه است.
- **فرانت‌اند این مسیر را از کلید `core` صدا می‌زند:** در `settingApi.ts` مقدار `API_MODULE = "core"` است ولی مسیر `/api/base/...` است؛ یعنی در Gateway، Route مربوط به `core` باید این مسیر را هم پوشش دهد. اگر روزی Routeها جدا شوند، این یکی می‌شکند.
- **جمع‌بندی:** Base را یا باید جدی گرفت (تعریف `Setting`، Command/Query، منو و مجوز) یا باید حذفش کرد و همان یک Endpoint را به Core منتقل کرد. حالت فعلی — «ماژولی که وجود دارد، فعال می‌شود، Migration می‌گیرد و هیچ کاری نمی‌کند» — بدترین حالت است.
