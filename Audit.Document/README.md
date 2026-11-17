# ماژول Audit (ثبت وقایع / لاگ حسابرسی)

> **در یک جمله:** Audit «دفتر یادداشت پلتفرم» است؛ هر اتفاق مهمی که در ماژول‌های دیگر می‌افتد (ایجاد، ویرایش، حذف یک رکورد) را در یک جدول جداگانه ثبت می‌کند تا بعداً بفهمیم چه کسی، چه زمانی، چه چیزی را تغییر داده است.

| | |
|---|---|
| **پروژه‌ها** | `Audit.Domain` / `Audit.Application` / `Audit.Infrastructure` / `Audit.Presentation` |
| **قرارداد عمومی** | `IAuditPublicService` (در حال حاضر داخل `Core.Application` تعریف شده) |
| **وابستگی به ماژول‌های دیگر** | Authorization، Identity، Navigation (فقط هنگام Seed اولیه) |
| **چه کسانی از آن استفاده می‌کنند** | `Core.Application.Behaviors.AuditBehavior` و هر ماژولی که بخواهد رویداد مهمی را ثبت کند |
| **جایگاه پیشنهادی در مدل لایه‌ای** | لایه ۰ (سرویس‌دهنده به بقیه) |

---

## ۱. این ماژول به چه درد می‌خورد؟ (به زبان ساده)

فرض کن چند نفر روی یک سیستم کار می‌کنند و یک روز رکورد یک کارمند عوض می‌شود. سؤال‌هایی که پیش می‌آید این‌هاست: چه کسی عوض کرد؟ کِی؟ مقدار قبلی چه بود؟

اگر هر ماژول خودش این اطلاعات را جایی بنویسد، بعداً ده جای مختلف برای بررسی داریم. Audit این کار را یک‌جا انجام می‌دهد: یک جدول مرکزی به نام `AuditLogs` که هر ردیف آن یک «اتفاق» است — نام عملیات، نام موجودیت، شناسهٔ آن رکورد، شناسهٔ کاربر و یک JSON از تغییرات.

نکتهٔ مهم این است که Audit **منطق کسب‌وکار ماژول‌های دیگر را نمی‌شناسد**. فقط یک سرویس ساده ارائه می‌دهد که می‌گوید «این اتفاق را ثبت کن». به همین دلیل هر ماژول می‌تواند بدون اینکه به جزئیات Audit وابسته شود، از آن استفاده کند.

در عمل امروز یک ثبت خودکار هم وجود دارد: `AuditBehavior` یک Pipeline Behavior در MediatR است که در `Core_AddApplication` ثبت شده و **بعد از اجرای هر Request** روی `IAuditPublicService.LogAsync` صدا می‌زند. یعنی لازم نیست دستی `LogAsync` را صدا بزنی — ولی هزینه‌اش این است که هر Query هم یک ردیف Audit تولید می‌کند و `entityId` هم یک `Guid` تصادفی است، نه شناسهٔ واقعی رکورد. این را در بخش ۵ با جزئیات نوشته‌ام.

## ۲. چه چیزهایی داخلش هست؟

- **موجودیت `AuditLog`** — همان ردیف دفتر یادداشت. فیلدها: `Action`، `EntityName`، `EntityId`، `UserId`، `Changes` (JSON) و `Timestamp`. با `[SecuredResource("audit.auditlog")]` علامت خورده تا در سیستم مجوزها به‌عنوان یک منبع قابل‌کنترل شناخته شود.
- **`AuditService`** — پیاده‌سازی `IAuditPublicService`. تغییرات را با `System.Text.Json` (camelCase) سریالایز می‌کند و اگر سریالایز شکست بخورد، به‌جای پرتاب خطا یک متن جایگزین می‌نویسد و Warning لاگ می‌کند.
- **خواندن لاگ‌ها** — `RecentAuditLogsSpec` (صفحه‌بندی + مرتب‌سازی نزولی بر اساس زمان) و `GetRecentAuditLogsQuery` که با MediatR اجرا می‌شود.
- **API آماده** — `GET /api/audit/AuditLogs` که ۱۰۰ لاگ آخر را برمی‌گرداند.
- **Seed** — هنگام بالا آمدن اپ، ماژول خودش را در Authorization و Navigation ثبت می‌کند: منبع `audit.auditlog`، مجوز Full برای نقش ادمین و منوی «مدیریت لاگ» با زیرمنوی «مشاهده لاگ‌ها».
- **Migration و Health Check** — دیتابیس اختصاصی با اسکیمای `audit` و چک سلامت `AuditDatabase`.

## ۳. چه چیزی داخلش **نیست** (و نباید انتظارش را داشت)

- موتور جست‌وجو یا گزارش‌گیری پیشرفته روی لاگ‌ها نیست؛ فقط «آخرین‌ها» را می‌دهد.
- لاگ‌ها را پاک‌سازی/آرشیو نمی‌کند؛ سیاست نگهداری ندارد.
- جایگزین Serilog یا Observability نیست. Serilog برای دیباگ فنی است، Audit برای «چه کسی داده را تغییر داد».
- Audit به‌صورت Outbox یا صف، ثبت را از مسیر اصلی جدا نمی‌کند؛ `LogAsync` همان‌جا `SaveChangesAsync` روی `AuditDbContext` می‌زند.

## ۴. یک نگاه فنی کوتاه

- دیتابیس مستقل با `HasDefaultSchema("audit")` و جدول تاریخچهٔ Migration به نام `__AuditMigrationsHistory`.
- `AuditDbContext` از `Base_DbContext` ارث می‌برد؛ یعنی فیلدهای Audit خودکار (CreatedAt/ModifiedAt) و امکان `EnsureView/EnsureTrigger` را به ارث می‌برد.
- ثبت در DI با کنوانسیون پلتفرم انجام می‌شود: `Audit_AddApplication`، `Audit_AddInfrastructure`، `Audit_AddHealthChecks`، `Audit_UseInfrastructure`. این متدها با Reflection از روی نام ماژول در `Modules:Enabled` پیدا و اجرا می‌شوند.
- ماژول به `IOutboxProcessorRegistration` وابسته است و پردازشگر Outbox مخصوص `AuditDbContext` را ثبت می‌کند؛ پس Outbox داری اما مصرف‌کنندهٔ عملی برای رویدادهای Audit نداری.
- `AuditResourceDefinitionProvider` کامنت شده و بلااستفاده است؛ کار ثبت منابع در `AuditSeedData` انجام می‌شود.

## ۵. وضعیت واقعی امروز (صادقانه)

- قرارداد `IAuditPublicService` جای عجیبی است: در `Core.Application` تعریف شده نه در `Audit.Application`. یعنی برای استفاده از Audit، عملاً به Core وابسته می‌شوی. این دقیقاً همان چیزی است که در بازنگری معماری باید حل شود.
- `IAuditInternalService` فقط یک متد خواندن (`GetRecentLogsAsync`) به قرارداد عمومی اضافه می‌کند و در Presentation استفاده می‌شود.
- در `AuditService` فیلد `_cache` تزریق می‌شود ولی استفاده نمی‌شود — بدهی فنی کوچک.
- کنترلر لاگ‌ها هیچ `[Authorize]` یا چک مجوزی ندارد و فقط به `ControllerBase` ارث‌بری می‌کند؛ یعنی منبع `audit.auditlog` در Seed تعریف شده ولی در Endpoint اعمال نمی‌شود.
- `AuditBehavior` همهٔ Requestها را لاگ می‌کند (Query و Command را نمی‌تواند تفکیک کند)، `entityId` را تصادفی می‌سازد و در `AuditService.LogAsync` یک `SaveChangesAsync` جدا روی `AuditDbContext` می‌زند. نتیجه: نویز زیاد + یک رفت‌وبرگشت دیتابیس اضافه به ازای هر Request. تا این‌ها اصلاح نشود، جدول AuditLogs قابل اتکا به‌عنوان تاریخچهٔ تغییرات نیست.
- تنها مصرف‌کنندهٔ واقعی روز به امروز، همان `AuditBehavior` است.
