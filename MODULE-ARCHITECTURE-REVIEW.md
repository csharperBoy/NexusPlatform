# بازنگری معماری: مرز ماژول‌ها و سرنوشت Core

> سند مشورتی. جمع‌بندی: **ایدهٔ لایه‌بندی ماژول‌ها درست است؛ «حذف Core» به‌شکلی که گفته شد نیمه‌درست است و اگر یک‌جا انجام شود، پروژه را زمین می‌زند.** Core باید به سه چیز متفاوت تجزیه شود، نه اینکه ناپدید شود.

---

## ۱. پاسخ کوتاه

| سؤال | پاسخ |
|---|---|
| آیا لایه‌بندی ۰→۳ ماژول‌ها ایدهٔ خوبی است؟ | **بله.** این همان «Modular Monolith با Tier» است و برای پلتفرمی که می‌خواهد پازل‌وار سرهم شود، انتخاب درستی است. |
| آیا Core باید حذف شود؟ | **به‌عنوان یک اسمبلی غول‌آسا: بله. به‌عنوان مفهوم: نه.** سه نقش در Core قاطی شده که سرنوشت متفاوتی دارند. |
| آیا مرز فعلی ماژول‌ها با این مدل جور است؟ | **نه.** امروز ۸۱ پروژه از ۱۰۲ پروژه به `Core.*` ارجاع می‌دهند و چند ارجاع مستقیم به Infrastructure ماژول دیگر وجود دارد. |
| بزرگ‌ترین خطر | بازآرایی «همه‌با‌هم» بدون تست معماری. اول باید قواعد را قابل‌اجرا کنی، بعد جابه‌جا کنی. |

---

## ۲. وضعیت امروز — با شاهد، نه حس

| مشاهده | شاهد |
|---|---|
| هستهٔ غول‌آسا | ۸۱ پروژه از ۱۰۲ پروژهٔ سلوشن به یکی از `Core.*` ارجاع می‌دهند |
| وابستگی معکوس | `Core.Application` قرارداد ۶ ماژول را تعریف می‌کند: `IPersonPublicService` (People)، `IEmploymentPublicService`/`IPostPublicService`/`ILocationPublicService` (HR)، `IContactPublicService`/`IPhoneBookPublicService` (Contact)، `IUserPublicService`/`IRolePublicService` (Identity)، `IMenuPublicService` (Navigation)، `IResourcePublicService`/`IPermissionPublicService` (Authorization) |
| Null Object به‌جای مرز | `Core_AllModuleNullServiceInject` برای همین قراردادها پیاده‌سازی Null ثبت می‌کند؛ نتیجه: نبودِ یک ماژول، خطا نمی‌دهد — سکوت می‌کند |
| نقض Tier در کامپایل | `HR.Infrastructure → People.Infrastructure`، `Trader.Infrastructure → Scheduler.Infrastructure` |
| نقض لایه درون ماژول | `User.Presentation → Core.Infrastructure` |
| پلتفرم از ماژول‌ها خبر دارد | `ModuleEnum` فهرست ثابت ۱۵ ماژول را دارد، ولی ۲۰ ماژول داری (User، Base، Sample، TraderServer، PhoneBook نیستند) |
| لودر شکننده | کشف ماژول با `Assembly.Load("{Name}.Infrastructure")` + نام متد قراردادی؛ خطا فقط `Console.WriteLine` می‌شود |
| تکرار مفهوم | `Navigation.Infrustructure` (غلط املایی، در `.sln` نیست)؛ `User.Person` در برابر `People.Person`؛ `Contact.Infrastructure.PhoneBookService` در برابر ماژول `PhoneBook` |
| بیلد شکسته | `TraderServer.*` به `BrokerageOperations.*` ارجاع می‌دهد که در کامیت آخر (`remove files`) حذف شده‌اند |
| وابستگی پنهان در زمان اجرا | `Audit.Infrastructure.ModuleInitializer` و `Base.Infrastructure.BaseSeedData` به `IResourcePublicService`، `IPermissionPublicService`، `IRolePublicService`، `IMenuPublicService` نیاز دارند — یعنی ماژول‌های «لایه ۰» در زمان اجرا به «لایه ۱» وابسته‌اند، ولی هیچ‌جا این را اعلام نکرده‌اند |

نکتهٔ مهم: چون قراردادها در Core هستند، هیچ‌کدام از این وابستگی‌ها در گراف `ProjectReference` دیده نمی‌شوند. **گراف واقعی وابستگی در زمان اجرا با گراف کامپایل یکی نیست.** تا وقتی این را قابل‌مشاهده نکنی، هر لایه‌بندی فقط روی کاغذ است.

---

## ۳. «حذف Core» یعنی سه کار، نه یک کار

Core امروز سه نقش دارد که باید از هم جدا شوند:

| نقش | چه چیزهایی | مقصد پیشنهادی |
|---|---|---|
| **۱. Shared Kernel** | `BaseEntity`، `Result`/`BatchResult`، `Optional`، Value Object ها، `ISpecification`/`BaseSpecification`، `ITimeProvider`، Enums پایه | یک بستهٔ کوچک `Platform.Kernel` **بدون هیچ وابستگی**. این «ماژول» نیست؛ کتابخانهٔ زبان مشترک است. حذفش = کپی ۲۰ برابری `BaseEntity` در ۲۰ ماژول. |
| **۲. Persistence** | `Base_DbContext`، `EfRepository`، `EfSpecificationRepository`، `EfUnitOfWork`، `BaseConfiguration`، `IMigrationManager`، `OutboxMessage` config | ماژول `Persist` در **Tier 0** با قرارداد خودش (`Persist.Application`). این همان چیزی است که خودت هم نامش را بردی. |
| **۳. Composition Root** | `ModuleLoaderExtensions`، Serilog/CorrelationId، Swagger، CORS، Exception Middleware، HealthCheck aggregator، Resilience، DataProtection، Excel Exporter | `Platform.Hosting` — و باید فقط توسط اپلیکیشن‌های **Tier 3** ارجاع داده شود. Bootstrap کار ماژول نیست. |
| **۴. Contract Registry** | قراردادهای ۶ ماژول + Null سرویس‌ها | به ماژول **مالک** منتقل شود و Null سرویس‌ها حذف شوند. |

اگر ۱ و ۲ و ۳ را با هم انجام دهی، هم مرزها روشن می‌شوند و هم هیچ قابلیتی از دست نمی‌رود.

---

## ۴. مدل Tier — با اصلاحات

### ۴.۱ اسم را عوض کن: «Tier» نه «Layer»

تو در هر ماژول چهار «لایه» داری (Domain/Application/Infrastructure/Presentation) و حالا می‌خواهی ۰..۳ هم «لایه» باشد. در هر گفت‌وگوی تیمی این دو قاطی می‌شوند. پیشنهاد: داخل ماژول = **Layer**، بین ماژول‌ها = **Tier** (یا «طبقه»/«سطح»).

### ۴.۲ قاعدهٔ وابستگی را دقیق کن

جملهٔ «فقط لایه Application و Domain به سمت Infrastructure و نه جور دیگه» تقریباً برعکسِ چیزی است که باید باشد. قاعدهٔ درست:

```
Presentation ──▶ Application ──▶ Domain
                     ▲
Infrastructure ──────┘   (Infrastructure پیاده‌سازی می‌کند، Application از آن خبر ندارد)
```

- **درون ماژول:** `Application` هرگز به `Infrastructure` ارجاع نمی‌دهد. `Infrastructure` به `Application` ارجاع می‌دهد (implements).
- **بین ماژول‌ها:** هیچ ماژولی به `Infrastructure` یا `Domain` ماژول دیگر ارجاع نمی‌دهد. فقط به `Application` (قرارداد) یا رویدادها.
- **بین Tierها:** Tier n فقط به Tierهای کوچک‌تر از خودش. پرش ممنوع (Tier 2 به Tier 0 مستقیم مجاز است، ولی Tier 0 به Tier 1 هرگز).

### ۴.۳ ماتریس مجاز

| از / به | Tier 0 | Tier 1 | Tier 2 | Tier 3 |
|---|---|---|---|---|
| **Tier 0** | ✅ (همان Tier، یک‌طرفه) | ❌ | ❌ | ❌ |
| **Tier 1** | ✅ | ✅ (یک‌طرفه) | ❌ | ❌ |
| **Tier 2** | ✅ | ✅ | ✅ (بدون چرخه) | ❌ |
| **Tier 3** | ✅ | ✅ | ✅ | — |

### ۴.۴ طبقه‌بندی پیشنهادی (با یک هشدار)

| Tier | ماژول‌ها |
|---|---|
| ۰ | `Persist`، `Cach`، `Event` (Outbox/EventBus)، `Audit`*، `WebScrapper` (قابلیت فنی) |
| ۱ | `Identity`/`Authentication`، `Authorization`، `Navigation`، `Notification`، `Scheduler`، `Base`* |
| ۲ | `People`، `HR`، `Contact`، `PhoneBook`، `Trader`، `TraderServer` |
| ۳ | `AkSteel.WebApi`، `Trader.WebApi`، `TraderServer.WebApi`، `PhoneBook.WebApi`، Gatewayها |

\* **هشدار:** `Audit` و `Base` امروز در `ModuleInitializer` منبع/مجوز/منو را در Authorization و Navigation ثبت می‌کنند؛ یعنی در واقع Tier 1 هستند. دو راه داری:
1. **پذیرفتن:** Audit و Base را Tier 1 اعلام کن. ساده، ولی Audit دیگر «سرویس‌دهندهٔ عمومی» نیست.
2. **درست‌تر:** ثبت منبع/منو را از ماژول جدا کن. هر ماژول یک **توصیف‌گر داده‌ای** بدهد (`ModuleDescriptor` شامل Resources/Menus/Permissions) و یک Bootstrapper در Tier 3 آن‌ها را به Authorization/Navigation بدهد. این‌طور Tier 0 واقعاً Tier 0 می‌ماند و کد Seed تکراری هم یک‌جا جمع می‌شود.

### ۴.۵ هر ماژول سه نوع API دارد — و فقط دو نوعش عمومی است

1. **Contracts** (قرارداد): اینترفیس + DTO، پایدار، نسخه‌دار. تنها چیزی که دیگران می‌بینند.
2. **Internal**: بقیهٔ Application و همهٔ Infrastructure. خصوصی.
3. **Events**: رویدادهای یک‌طرفه برای وقتی که نمی‌خواهی وابستگی مستقیم بسازی.

**نکتهٔ ظریفی که الان دام است:** قراردادهایی مثل `IRepository<HRDbContext, Employment, Guid>` عملاً `HRDbContext` را (که در Infrastructure است) به بیرون نشت می‌دهند. تا وقتی امضای قرارداد به DbContext/EF وابسته است، جداسازی واقعی ممکن نیست. برای قرارداد بین‌ماژولی از `ISpecificationRepository<TEntity,TKey>` یا اینترفیس اختصاصی ماژول (`IEmploymentReader` با DTO) استفاده کن، نه `IRepository` با Context.

---

## ۵. مشکلاتی که لایه‌بندی حلشان نمی‌کند

1. **خواندن مستقیم از دیتابیس ماژول دیگر.** `PhoneBookInfoView` روی داده‌های Contact/HR سوار است. اگر View در DB ماژول دیگر باشد، وابستگی واقعی در دیتابیس است و در گراف پروژه دیده نمی‌شود. راه‌حل: Read Model مالک‌دار + به‌روزرسانی با رویداد، یا پذیرش صریح این وابستگی در یک ADR.
2. **تراکنش بین ماژولی وجود ندارد.** هر ماژول `IUnitOfWork` خودش را دارد. نوشتن در دو ماژول = سازگاری نهایی با Outbox + Idempotency. این را به‌عنوان یک قاعدهٔ پلتفرم مکتوب کن، وگرنه کسی دنبال Distributed Transaction می‌رود.
3. **تکرار مفاهیم.** تا وقتی `User.Person` و `People.Person` هر دو هستند، هر لایه‌بندی فقط دو نسخه از یک مفهوم را در دو Tier می‌نشاند. اول یکی‌سازی، بعد جابه‌جایی.
4. **لودر بازتابی.** نام اسمبلی = نام ماژول = کلید config. یعنی تغییر نام اسمبلی (که در بازآرایی حتمی است) ماژول را بی‌صدا خاموش می‌کند. جایگزین پیشنهادی:

```csharp
public interface IModule
{
    string Name { get; }
    int Order { get; }
    IReadOnlyCollection<string> DependsOn { get; }
    void Register(IServiceCollection services, IConfiguration config);
    void Use(IApplicationBuilder app);
    ModuleDescriptor Describe();   // منابع، منوها، مجوزهای پیش‌فرض
}
```

به‌علاوهٔ یک اعتبارسنجی در Startup: ماژول‌های فعال موجودند؟ وابستگی‌هایشان فعال است؟ چرخه نداریم؟ اگر نه → **fail fast** با پیام واضح. همین یک تغییر، ۸۰٪ دردسرهای دیباگ «چرا ماژولم لود نشد» را حذف می‌کند.

5. **تست معماری.** قاعده‌ای که تست ندارد، وجود ندارد. یک تست با `NetArchTest`/`ArchUnitNET` (یا حتی خواندن سادهٔ csprojها) که این‌ها را بترکاند: Domain→EF، Application→Infrastructure، Tier n→Tier n+، ماژول→Infrastructure ماژول دیگر. این تست، قلابِ ایمنیِ کل بازآرایی است.

---

## ۶. نقشهٔ مهاجرت (هر گام باید بیلد سبز بدهد)

| گام | کار | معیار پایان | ریسک |
|---|---|---|---|
| ۰ | تثبیت: برگرداندن یا حذف رفرنس‌های `BrokerageOperations`، حذف `Navigation.Infrustructure` از دیسک، تصمیم مکتوب دربارهٔ `User`/`People` و `PhoneBook`/`Contact` | بیلد سبز + یک ADR کوتاه | کم |
| ۱ | `Platform.Kernel`: انتقال BaseEntity/Result/Optional/VO/Specification/ITimeProvider. **Namespace فعلی را نگه دار** تا diff منفجر نشود | هیچ ماژولی به Core.Infrastructure نیاز ندارد ولی بیلد سبز است | کم |
| ۲ | `Persist` (Tier 0): Base_DbContext، Repo، UoW، Configurationها، MigrationManager | ماژول‌ها فقط `Persist.Application`/`Persist.Infrastructure` را می‌بینند | متوسط (Migration assembly) |
| ۳ | `Platform.Hosting`: لودر، Serilog، Swagger، CORS، HealthCheck، Resilience، DataProtection | فقط ۴ اپ Tier 3 به Hosting ارجاع می‌دهند | متوسط |
| ۴ | انتقال قراردادها به ماژول مالک + حذف Null سرویس‌ها + پاک‌سازی امضاها از DbContext | گراف بدون Core.Application و بدون Null | بالا (بیشترین تغییر) |
| ۵ | `ModuleDescriptor` + Bootstrapper در Tier 3؛ حذف Seedهای Tier0→Tier1 | Audit/Base واقعاً Tier 0 | متوسط |
| ۶ | حذف پروژه‌های Core، اضافه‌کردن تست معماری به CI | گراف acyclic و مطابق ماتریس | کم |
| ۷ | (اختیاری) بازآرایی فیزیکی به `Modules/{Tier}/{Module}/{Layer}` | — | کم، ولی بی‌فایده اگر ۶ انجام نشده باشد |

**هشدار عملی دربارهٔ Migration:** با جابه‌جا شدن `DbContext` به اسمبلی دیگر، `MigrationsAssembly` و مسیر `Migrations/` عوض می‌شود. اگر اسمبلی Migration را تغییر دهی، EF ممکن است دیتابیس موجود را «بدون تاریخچه» ببیند و دوباره Migration بزند. در گام ۲ اسم اسمبلی Migration را ثابت نگه دار یا تاریخچه را در همان اسمبلی قبلی نگه دار.

---

## ۷. چه چیزی را نگه دار، چه چیزی را بشکن

**نگه دار (این‌ها درست‌اند):**
- چهار لایه داخل هر ماژول و استقلال پروژه‌های لایه.
- MediatR/CQRS، Specification، Outbox، schema-per-module، HealthCheck per module.
- ایدهٔ «پازل از ماژول‌ها + بخش اختصاصی» — این مزیت واقعی پلتفرم است.
- `Result<T>`، `BaseSpecification`، `UserDataContext` (با اصلاح مالکیت).

**بشکن:**
- Core به سه بسته (Kernel / Persist / Hosting).
- Contract Registry از Core به ماژول مالک.
- Null Object ها.
- `ModuleEnum` و کشف بازتابی، به نفع Manifest + fail-fast.
- تکرارها: `Navigation.Infrustructure`، `User.Person`، `Contact.PhoneBookService`.

**نساز:**
- سیستم پلاگین پیچیده‌تر از این. با ۴ اپ و ۲۰ ماژول، Manifest ساده کافی است.
- لایه‌بندی «سخت‌گیرانه با Reflection» بیشتر. هر Reflection اضافه، یک خطای زمان اجرای جدید است.

---

## ۸. جمع‌بندی یک‌خطی

> Core را نکش؛ **تکه‌تکه‌اش کن** و هر تکه را به جایی بده که مالکش است: زبان مشترک به Kernel، دیتا به Persist، بالا آوردن اپ به Hosting، و قراردادها به خود ماژول‌ها. لایه‌بندی ۰..۳ را با نام Tier و با تست معماری اجرا کن، نه با ایمان. و قبل از هر جابه‌جایی، تکرارها و بیلد شکسته را درست کن — وگرنه معماریِ تمیزتر روی یک پایهٔ لق ساخته می‌شود.
