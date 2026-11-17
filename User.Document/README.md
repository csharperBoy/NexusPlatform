# ماژول User (Person / PersonProfile — نسخهٔ قدیمی)

> **در یک جمله:** User یک ماژول نیمه‌کاره و رهاشده است که می‌خواست «شخص» را جدا از «حساب کاربری» نگه دارد؛ اما امروز نه در `NexusPlatform.sln` هست، نه در `Modules:Enabled`، و حتی به‌عنوان یک پروژهٔ مستقل هم کامپایل نمی‌شود.

| | |
|---|---|
| **پروژه‌ها** | `User.Domain` / `User.Application` / `User.Infrastructure` / `User.Presentation` / `User.Shared` — هیچ‌کدام در `NexusPlatform.sln` نیستند |
| **قرارداد عمومی** | ندارد. `IPersonInternalService` در `User.Application/Interfaces` خالی است و `PersonService` به `IPersonPublicService` (قرارداد ماژول People در `Core.Application/Abstractions/People`) وصل شده ولی هیچ متدی پیاده نمی‌کند |
| **وابستگی به ماژول‌های دیگر** | فقط در سطح اسمبلی: `Core.Domain`، `Core.Application`، `Core.Infrastructure`. از نظر مفهومی با People و Identity هم‌پوشانی کامل دارد |
| **چه کسانی از آن استفاده می‌کنند** | هیچ‌کس: صفر `ProjectReference` بیرونی، صفر `using User.*` بیرون از خودش، صفر رکورد در `Modules:Enabled` |
| **جایگاه پیشنهادی در مدل لایه‌ای** | لایه ۱ — وضعیت: مشکوک به تکرار/قدیمی |

---

## ۱. این ماژول به چه درد می‌خورد؟ (به زبان ساده)

ایدهٔ پشت User ساده و درست بود: «شخص» با «کاربر» یکی نیست. یک کارمند ممکن است هیچ‌وقت وارد سیستم نشود، و یک کاربر می‌تواند به چند شخص وصل شود. پس User دو موجودیت داشت: `Person` برای اطلاعات ثابت (کد ملی، نام، تاریخ تولد، نام پدر) و `PersonProfile` برای اطلاعات قابل تغییر (آدرس، وضعیت تأهل، تعداد فرزند، تلفن، ایمیل).

مشکل این است که این ایده در ماژول People دوباره و کامل‌تر پیاده‌سازی شد: `Party` (طرف حساب)، `NaturalPerson` و `NaturalPersonProfile` با `IOwnerableEntity` برای Data Scope، به‌همراه `LegalPerson` و `PartiesRelation`. People امروز در `Modules:Enabled` فعال است (با `Order: 70`) و `IPersonPublicService` را واقعاً پیاده می‌کند؛ Identity هم `FkPartyId` را به همان `Party` وصل می‌کند. یعنی زنجیرهٔ واقعی «کاربر → شخص» از Identity به People می‌رود و از User عبور نمی‌کند.

به این ترتیب User نه یک ماژول مستقل است و نه یک لایهٔ مشترک؛ فقط یک اسکلت قدیمی است. برای همین هم پیشنهاد لایه‌بندی «لایه ۱» را با احتیاط می‌پذیرم: اگر روزی این قابلیت برگردد، جای درستش لایه ۱ است، ولی امروز چیزی برای سرویس‌دادن ندارد.

## ۲. چه چیزهایی داخلش هست؟

- **`Person`** — `NationalCode` و `FullName` (ValueObject)، `BirthDate`، `BirthPlace`، `FatherName`، `Gender`، `OneTimePassword`، به‌همراه `GetAge()`، `SetFullName(...)` و `ApplyChange(...)`. با `IAuditableEntity`، `IAggregateRoot` و `IEntity<Guid>` علامت خورده و `Id` را خودش `Guid.NewGuid()` می‌گیرد.
- **`PersonProfile`** — یک رکورد «تاریخی» برای هر تغییر: `Address`، `JobTitle`، `EducationLevel`، `PersonalCode`، `MaritalStatus` (یک enum محلی)، `DateOfHire`، `DateOfMarried`، `MemberQty`، `DependantsQty`، `NumberOfChildren`، `StudentChildCount`، `ZipCode`، `Phone`/`Mobile`/`Email` (ValueObject)، `Descriptions`، `Image`، `Enablity`/`Visiblity`/`Remove` و `PasswordEmergency`. فقط یک Constructor و هیچ متد تغییردهنده‌ای ندارد.
- **دو رویداد دامنه** — `PersonProfileCreatedEvent` و `PersonProfileUpdatedEvent` با فیلدهای `NumberOfChildren` و نسخهٔ قبلی/جدید.
- **`IPersonInternalService`** — یک اینترفیس **کاملاً خالی** بدون هیچ متد.
- **`PersonService`** — کلاس `public` که هر دو اینترفیس را امضا می‌کند ولی بدنه‌اش فقط سه فیلد تزریق‌شده و یک Constructor است؛ هیچ متدی ندارد.
- **`UserDbContext`** — اسکیمای `user`، جدول‌های `Persons` و `PersonProfiles`، به‌همراه `OutboxMessages`، پیکربندی `OwnsOne` برای ValueObjectها و `OnDelete(Restrict)` برای رابطهٔ Profile→Person.
- **Migration** — دو فایل: `20251104160121_Initial_User_db` و `20260415073538_Edit2_User`، با تاریخچهٔ `__UserMigrationsHistory` و یک `UserDbContextFactory` برای زمان Design.
- **DI** — `User_AddApplication` و `User_AddPresentation` هر دو خالی‌اند؛ `User_AddInfrastructure` فقط DbContext را می‌سازد و `IPersonPublicService`/`IPersonInternalService` را ثبت می‌کند؛ `User_AddHealthChecks` چک `UserDatabase` را اضافه می‌کند و `User_UseInfrastructure` فقط Migration را اجرا می‌کند.

## ۳. چه چیزی داخلش **نیست** (و نباید انتظارش را داشت)

- هیچ Command، Query، Handler، Validator یا Controller‌ای ندارد؛ `User.Presentation` صفر فایل کنترلر دارد.
- هیچ Seed، Resource، Permission یا Menu‌ای ثبت نمی‌کند؛ پس حتی اگر فعال شود، در پنل چیزی از آن دیده نمی‌شود.
- هیچ Specification‌ای ندارد (پوشهٔ `Specifications\` در csproj خالی است) و `User.Shared` هم هیچ فایل `.cs` ندارد.
- منطق ساخت یا ویرایش شخص ندارد: `PersonProfile` فقط Constructor دارد و رویدادهایش هیچ‌جا raise نمی‌شوند.
- تست ندارد؛ `User.Test` وجود ندارد.
- هیچ متدی برای خواندن/نوشتن `Person` در `PersonService` نیست، پس عملاً API ندارد.

## ۴. یک نگاه فنی کوتاه

- **پروژه کامپایل نمی‌شود.** `User.Infrastructure.csproj` فقط `Core.Application`، `Core.Infrastructure` و `Core.Shared` را رفرنس می‌دهد و هیچ رفرنسی به `User.Domain` و `User.Application` ندارد؛ در حالی که `PersonService.cs` هم `using User.Domain.Entities` دارد و هم `using User.Application.Interfaces`. چون پروژه در Solution نیست، هیچ Buildی این خطا را نشان نمی‌دهد.
- `UserDbContext` از `DbContext` خالص ارث می‌برد، نه از `Core.Infrastructure.Data.Base_DbContext` و نه حتی `IBase_DbContext`؛ پس `UpdateAuditableEntities` و `EnsureView`/`EnsureTrigger` را ندارد و `CreatedBy`/`ModifiedBy` را کسی پر نمی‌کند.
- ثبت در DI با کنوانسیون پلتفرم انجام می‌شود (`User_AddApplication`، `User_AddInfrastructure`، `User_AddPresentation`، `User_AddHealthChecks`، `User_UseInfrastructure`)، اما هیچ `User_AddDomain` وجود ندارد — چون `User.Domain` هیچ کلاس DI ندارد.
- `User_AddInfrastructure` هم `services.BuildServiceProvider()` می‌زند تا `IOutboxProcessorRegistration` بگیرد؛ یعنی اگر ماژول Event فعال نباشد، ثبت این ماژول هم شکست می‌خورد.
- در `Modules:Enabled` فایل `AkSteel.WebApi/appsettings.json` هیچ رکوردی به نام `User` وجود ندارد؛ ماژول‌های هویت و افراد `Identity` و `People` هستند.

## ۵. وضعیت واقعی امروز (صادقانه)

- **تکرار People، نه تکمیل آن.** نگاشت تقریباً یک‌به‌یک است: `Person` ↔ `NaturalPerson`، `PersonProfile` ↔ `NaturalPersonProfile`، `MaritalStatus` ↔ همان enum در People. چیزی که User دارد و People ندارد فقط چند ستون است: `PersonalCode`، `DateOfHire`، `PasswordEmergency`، `FkchildId`، `Image` و پرچم‌های `Enablity`/`Visiblity`/`Remove`. بقیه (از جمله `OwnerPersonId` برای Data Scope و ارتباط با `Party`) فقط در People هست.
- **قدیمی‌تر است، نه جدیدتر.** اولین Migration مربوط به User تاریخ `20251104` است و Migration ماژول People تاریخ `20260903`؛ یعنی User نسخهٔ اولیه بوده و People جای آن را گرفته است. این با توصیف «خواهر بزرگ‌تر و کوچک‌تر» جور درنمی‌آید: User کوچک‌تر است، ولی از نظر زمانی قدیمی‌تر.
- **هیچ مصرف‌کننده‌ای ندارد.** جست‌وجوی `using User.` در کل مخزن فقط داخل خود این پروژه‌ها نتیجه می‌دهد؛ حتی `NexusPlatform.sln` هم این پنج پروژه را ندارد. یعنی کد مرده است، نه «کم‌استفاده».
- **ثبت DI اشتباه است.** `User_AddInfrastructure` این‌ها را دارد: `AddScoped<IPersonPublicService>(sp => sp.GetRequiredService<PersonService>())` و همان برای `IPersonInternalService`؛ اما خود `PersonService` هیچ‌جا `AddScoped` نشده. اگر این ماژول فعال شود، اولین resolve از `IPersonPublicService` استثنای `InvalidOperationException` می‌دهد. در People همان الگو درست نوشته شده (`AddScoped<PersonService>()` قبل از اینترفیس‌ها).
- **`PersonService` و `IPersonInternalService` خالی‌اند.** سرویس هیچ متدی ندارد، پس ادعای پیاده‌سازی `IPersonPublicService` (که هفت متد دارد) فقط با ارث‌بری از یک اینترفیس دیگر ممکن شده — و در واقع هیچ‌کدام از متدهای آن قرارداد قابل استفاده نیست.
- **اگر روزی فعال شود، قرارداد People را می‌دزدد.** هر دو ماژول `IPersonPublicService` را ثبت می‌کنند؛ چون کلید DI یکی است، آخری برنده می‌شود. با `Order` بزرگ‌تر برای User، تمام مصرف‌کننده‌های واقعی (`HR`، `Contact`، `Authorization`، `Identity`) به‌طور خاموش به یک پیاده‌سازی خالی وصل می‌شوند. این خطرناک‌ترین بخش ماجراست.
- **نام تکراری، سردرگمی می‌سازد.** دو اینترفیس هم‌نام `IPersonInternalService` در `User.Application.Interfaces` و `People.Application.Interfaces` وجود دارد؛ در یک فایل که هر دو فضای‌نام را using کند، خطای ابهام می‌گیری.

### پیشنهاد صریح

User را حذف کن، نه احیا. اگر ستون‌هایی مثل `PersonalCode`، `Image` یا پرچم‌های `Enablity`/`Remove` واقعاً لازم‌اند، اول آن‌ها را به `NaturalPersonProfile` در People منتقل کن (با یک Migration و بدون تغییر رفتار)، بعد پنج پروژهٔ User و پوشهٔ `User.Document` را از مخزن بردار. نگه‌داشتن یک ماژول غیرقابل‌کامپایل و خارج از Solution فقط هزینهٔ نگهداری و سردرگمی برای تیم جدید است.
