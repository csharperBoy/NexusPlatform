# ماژول Core (هستهٔ مشترک پلتفرم)

> **در یک جمله:** Core آن بخشی از پلتفرم است که تقریباً همهٔ ماژول‌ها از آن استفاده می‌کنند — قراردادهای عمومی، کلاس‌های پایه، ریپازیتوری، `UnitOfWork`، بیهِویورها، لودر ماژول و زیرساخت‌های مشترک. Core خودش «قابلیت کسب‌وکار» نیست؛ «زمین بازی» است.

| | |
|---|---|
| **پروژه‌ها** | `Core.Domain` / `Core.Application` / `Core.Infrastructure` / `Core.Presentation` / `Core.Shared` |
| **قرارداد عمومی** | خودش منبع قرارداد است: `IRepository<,,>`، `IUnitOfWork<>`، `ISpecificationRepository<,>`، `IEventBus`، `Result`، `UserDataContext` و… |
| **وابستگی به ماژول‌های دیگر** | هیچ (پایین‌ترین لایه) |
| **چه کسانی از آن استفاده می‌کنند** | همهٔ ماژول‌ها و همهٔ اپلیکیشن‌های لایهٔ ۳ |
| **جایگاه پیشنهادی در مدل لایه‌ای** | لایه ۰ — ولی در بازنگری معماری باید به چند ماژول مستقل تجزیه شود |

---

## ۱. این ماژول به چه درد می‌خورد؟ (به زبان ساده)

هر پروژهٔ نرم‌افزاری یک‌سری چیزهای تکراری دارد: هر جدول یک `Id` می‌خواهد، هر عملیات نوشتن باید در یک تراکنش ذخیره شود، هر خطا باید یک شکل استاندارد داشته باشد، هر ماژول باید در DI ثبت شود، لاگ و Migration و Health Check هم که جای خود دارد.

Core دقیقاً همین‌ها را یک‌جا جمع کرده تا هر ماژول جدید از روز اول این‌ها را داشته باشد. به عبارت دیگر، وقتی یک ماژول جدید می‌سازی، بیشترِ «سیم‌کشی» را Core انجام می‌دهد و تو فقط روی منطق کسب‌وکار تمرکز می‌کنی.

Core در عمل سه نقش متفاوت دارد که خوب است از هم جدا ببینیم:

1. **Shared Kernel واقعی** — چیزهایی مثل `BaseEntity`، `Result<T>`، `IRepository`، `Base_DbContext`، `BaseSpecification`. اینها واقعاً عمومی‌اند و منطقی است که مشترک باشند.
2. **Host/Bootstrap** — لودر ماژول‌ها با Reflection، Migration Manager، Serilog، CORS، Swagger، Health Checks. اینها وظیفهٔ «بالا آوردن اپلیکیشن» است، نه وظیفهٔ یک ماژول دامنه.
3. **مرکز قراردادهای بین‌ماژولی** — و اینجاست که مشکل شروع می‌شود: Core قرارداد ماژول‌های *بالادست* را هم در خودش نگه می‌دارد (`IPersonPublicService`، `IEmploymentPublicService`، `IUserPublicService`، `IMenuPublicService`، `IContactPublicService`، `ICachePublicService`، `IAuditPublicService`). یعنی Core که باید پایین‌ترین لایه باشد، از همهٔ ماژول‌ها «اسم» می‌داند.

## ۲. چه چیزهایی داخلش هست؟

**Core.Domain — زبان مشترک دامنه**
- `BaseEntity` (شناسه + Domain Events + `ApplyChange` مبتنی بر ماسک به‌روزرسانی)
- اینترفیس‌های مشخصه: `IAuditableEntity`، `IOwnerableEntity` (چهار Owner)، `ISoftRemovable`، `IHierarchicalStructureEntity`، `IHasEffectivePeriod`، `IHasPermissionRuleEntity`، `IJoinDetaileableEntity`
- `BaseSpecification<T>` و `ISpecification<T>` (Criteria، Include، OrderBy، Paging)
- Value Object ها: `Money`، `NationalCode`، `PhoneNumber`، `Email`، `FullName`
- `Optional<T>` برای «نگذاشتن مقدار» در Updateها
- `OutboxMessage`، `DomainEventBase`، `IAggregateRoot`، `SecuredResourceAttribute`، `DynamicFilterableAttribute`

**Core.Application — قراردادها و بیهِویورها**
- `IRepository<TDbContext,TEntity,TKey>`، `ISpecificationRepository<TEntity,TKey>`، `IUnitOfWork<TDbContext>` (Begin/Save/Rollback)
- `IEventBus`، `IOutboxService<TDbContext>`، `IOutboxProcessorRegistration`
- پنج Pipeline Behavior در MediatR: `LoggingBehavior`، `ValidationBehavior`، `RetryBehavior`، `AuditBehavior`، `SetUserDataBehavior`
- `UserDataContext` (کاربر جاری + Party + Postها + نقش‌ها + مجوزها) و `IUserDataContextProvider`
- قراردادهای امنیتی: `IPermissionChecker`، `ICurrentUserService`، `ISecretProtector` (بعضی کامنت شده و بلااستفاده‌اند)
- `ModuleHelper` / `ModuleSettings` (کدام ماژول‌ها فعال‌اند) و `Core_AllModuleNullServiceInject` که برای هر ماژول بالادست یک پیاده‌سازی Null ثبت می‌کند

**Core.Infrastructure — اجرای واقعی**
- `Base_DbContext`: پر کردن خودکار `CreatedAt/CreatedBy/ModifiedAt/ModifiedBy`، ساخت View/Trigger از اسکریپت Embedded
- `EfRepository`، `EfSpecificationRepository`، `EfUnitOfWork`
- `MigrationManager`، Serilog + CorrelationId، `ExceptionHandlingMiddleware`
- Health Checks (Database/Redis)، Resilience (Polly)، Excel Exporter
- `ModuleLoaderExtensions`: کشف و اجرای متدهای `{Module}_AddInfrastructure` و… با Reflection
- `DataProtectionSecretProtector` (کلیدها در Redis)

**Core.Presentation / Core.Shared**
- `BaseController` + `HandleResult/HandleBatchResult`، فیلتر `[AuthorizeResource(resourceKey, action)]`
- `Result`/`Result<T>`/`BatchResult`، DTOهای مشترک (SelectionList، Permission/Resource/Scope/Menu…)، Enums، `ITimeProvider`

## ۳. چه چیزی داخلش **نیست**

- منطق کسب‌وکار هیچ دامنه‌ای (نه پرسنل، نه ترید، نه مجوزدهی). هر چیزی که «قاعدهٔ کار» است باید در ماژول خودش باشد.
- Authentication واقعی (JWT، لاگین) در Identity است، نه Core.
- قراردادهای بین‌ماژولی هم *اینجا* تعریف شده‌اند، ولی مال Core نیستند — این یک بدهی معماری است، نه یک قابلیت.

## ۴. یک نگاه فنی کوتاه

- ترتیب ثبت در DI مهم است: `Core_AddApplication` اول Null سرویس‌ها را ثبت می‌کند، بعد ماژول واقعی (اگر در `Modules:Enabled` باشد) همان اینترفیس را با پیاده‌سازی واقعی ثبت می‌کند و last-wins برنده است. یعنی نبودِ یک ماژول باعث کرش نمی‌شود، ولی باعث می‌شود Null Object جواب بدهد.
- لودر ماژول با `Assembly.Load("{Module}.Infrastructure")` و جست‌وجوی متد static با نام قراردادی کار می‌کند. نام اسمبلی = نام ماژول؛ هر تغییری در نام‌گذاری، ماژول را بی‌صدا از مدار خارج می‌کند (فقط یک `Console.WriteLine` هشدار می‌دهد).
- `Core.Shared` هیچ وابستگی‌ای ندارد و عمداً سبک است؛ ولی DTOهای مربوط به Authorization/Navigation/HR/Contact را هم در خود دارد — یعنی باز هم «دانشِ ماژول‌های دیگر» اینجاست.
- `Core.Domain` هم مستقل است؛ همین باعث می‌شود بشود آن را بدون بقیهٔ Core به یک بستهٔ مستقل تبدیل کرد.

## ۵. وضعیت واقعی امروز (صادقانه)

- **Core بزرگ‌ترین گلوگاه معماری است.** هر چهار لایهٔ هر ماژول به Core ارجاع می‌دهند؛ در نتیجه هر تغییر در Core.Application همهٔ ماژول‌ها را rebuild و ریسک می‌کند.
- `ICurrentUserService` هیچ‌جا در DI ثبت نشده (خط ثبتش کامنت است) و عملاً منبع کاربر جاری `UserDataContext` است که با `SetUserDataBehavior` یا `AuthorizeResourceFilter` پر می‌شود. دو مکانیزم موازی برای یک کار.
- `AuditableEntity` و `DataScopedEntity` در `Core.Domain` کامنت شده‌اند و کلاس پایهٔ واقعی موجودیت‌ها `BaseEntity` + اینترفیس‌ها است. کسی که کد را می‌خواند اول گیج می‌شود.
- `IResourceDefinitionProvider`، `IAuthorizationChecker`، `IDataScopeProcessor`، `IPermissionRulePublicService`، `IScopePublicService` هم کامنت شده‌اند؛ یک نسل قبلی طراحی که هنوز در سورس است.
- `Core.Presentation.Core_AddPresentation` فقط JSON و `Optional` را تنظیم می‌کند؛ `AuthorizeResourceFilter` بدون `[Authorize]` استاندارد هم می‌تواند `Unauthorized` بدهد، ولی خودش احراز هویت نمی‌کند.
- جمع‌بندی: Core الان هم‌زمان سه نقش دارد — «Shared Kernel»، «Composition Root» و «Contract Registry». فقط نقش اول واقعاً باید در Core بماند.
