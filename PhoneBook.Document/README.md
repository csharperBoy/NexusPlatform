# ماژول PhoneBook (دفترچه تلفن سازمانی)

> **در یک جمله:** PhoneBook یک **مدل خواندنی** (read model) است، نه یک ماژول داده‌محور: هیچ جدول قابل‌نوشتنی ندارد و فقط از طریق یک **ویو SQL** که روی اسکیماهای `hr` و `people` ساخته می‌شود، اطلاعات تماس کارمندان را گروه‌بندی‌شده برمی‌گرداند.

| | |
|---|---|
| **پروژه‌ها** | `PhoneBook.Domain` / `PhoneBook.Application` / `PhoneBook.Infrastructure` / `PhoneBook.Presentation` (+ `PhoneBook.WebApi` به‌عنوان **هاست اپلیکیشن**، نه یک لایه) |
| **قرارداد عمومی** | `IPhoneBookPublicService` (داخل `Core.Application/Abstractions/Contact` تعریف شده و **خالی** است) |
| **وابستگی به ماژول‌های دیگر** | در کد به هیچ ماژولی وابسته نیست؛ در داده به اسکیماهای `hr` و `people` وابسته است |
| **چه کسانی از آن استفاده می‌کنند** | امروز هیچ ماژولی؛ تنها مصرف‌کننده، کنترلر و Query خودِ ماژول است. ماژول `Contact` یک کپی موازی و فعال‌تر از همین قابلیت دارد |
| **جایگاه پیشنهادی در مدل لایه‌ای** | لایه ۲ (با کاندید تبدیل شدن به اپ لایه ۳) |

---

## ۱. این ماژول به چه درد می‌خورد؟ (به زبان ساده)

شماره داخلی یک همکار در چند جدول پخش است: موبایل شخصی در `people.PartyContacts`، داخلی پست سازمانی در `hr.PostContacts` و شماره سازمانی کارمند در `hr.EmploymentContacts`. کسی که «دفترچه تلفن» را باز می‌کند نمی‌خواهد بداند داده از کدام جدول آمده؛ می‌خواهد یک ردیف ببیند: نام، واحد سازمانی، عنوان شغلی و چند شماره.

PhoneBook همین کار را می‌کند: ویو `phonebook.PhoneBook_Info_View` همهٔ JOINها را انجام می‌دهد، ماژول آن را به‌صورت موجودیت بدون کلید `PhoneBookInfoView` می‌خواند و لایه Application ردیف‌ها را بر اساس `EmploymentCode` گروه می‌کند.

نکتهٔ کلیدی: PhoneBook **مالک داده نیست**. اگر HR جدول `PostContacts` را عوض کند، PhoneBook باید ویو را اصلاح کند؛ ولی خودش هیچ نوشتنی و هیچ Command‌ای ندارد.

## ۲. چه چیزهایی داخلش هست؟

- **موجودیت `PhoneBookInfoView`** — کلاسی بدون کلید با `NationalCode`، `FirstName`، `LastName`، `EmploymentCode`، `OrganizationUnitsName`، `JobTitleName`، `JobLevelTitle`، `LocationTitle` و دوازده فیلد تماس (`PartyMobile`، `PostContactPhone`، `EmploymentContactMobile` و…).
- **ویو SQL** — اسکریپت `PhoneBook.Infrastructure/SqlScript/CreatePhoneBookInfoViewScript.sql` به‌صورت `EmbeddedResource` ذخیره شده و هنگام Migration با `EnsureView(...)` اجرا می‌شود. ویو از `hr.[ Employment]`، `people.naturalPersons`، `people.Parties`، `hr.Assignments` (فقط `IsCurrent = 1`)، `hr.Post` و دوازده LEFT JOIN به `people.PartyContacts`، `hr.PostContacts` و `hr.EmploymentContacts` ساخته می‌شود.
- **گروه‌بندی و حذف تکرار** — `PhoneBookMappingExtensions.ToPhoneBookDtos` ردیف‌ها را بر اساس `EmploymentCode` گروه می‌کند و `AddContact` مقدارهای خالی و تکراری (بر اساس `Type` + `Source` + `Value`) را رد می‌کند.
- **مدل خروجی** — `PhoneBookEmploymentDto` با `Contacts`، `ContactSummary` (چسباندن شماره‌ها با ` - `) و `HasMultipleContacts` (برای آکاردئون فرانت‌اند).
- **Query و API** — `GetPhoneBookListQuery(Guid? organUnitId = null)` با MediatR، `GetPhoneBookSpec` که فقط `ApplyOrderBy(u => u.EmploymentCode)` می‌کند، و `GET api/Contact/PhoneBook/GetList` روی `PhoneBookController`.
- **زیرساخت استاندارد** — `PhoneBookDbContext` از `Base_DbContext`، اسکیمای `phonebook`، تاریخچهٔ `__phonebookMigrationsHistory`، سه Migration، Repository مبتنی بر Specification، UnitOfWork و پردازشگر Outbox.

## ۳. چه چیزی داخلش **نیست** (و نباید انتظارش را داشت)

- **هیچ نوشتنی ندارد.** نه Command، نه اعتبارسنجی. برای تغییر شماره، جای درست `Contact` یا `HR` است.
- **جست‌وجو، فیلتر، صفحه‌بندی و Cache ندارد.** Query یک پارامتر `organUnitId` می‌گیرد که نادیده گرفته می‌شود؛ هر فراخوانی کل ویو را می‌خواند و در حافظه گروه‌بندی می‌کند.
- **مجوز و منبع امنیتی ندارد.** نه `[SecuredResource]` و نه `[AuthorizeResource]` (بخش ۵).
- **Refresh رویدادی ندارد.** `ChangePartyEventHandler` عملاً وجود ندارد؛ کل بدنه‌اش کامنت شده است.

## ۴. یک نگاه فنی کوتاه

- ثبت در DI با کنوانسیون پلتفرم: `PhoneBook_AddDomain`، `PhoneBook_AddApplication`، `PhoneBook_AddInfrastructure`، `PhoneBook_AddPresentation`، `PhoneBook_AddHealthChecks` و `PhoneBook_UseInfrastructure`؛ این متدها با Reflection از `Modules:Enabled` پیدا و اجرا می‌شوند.
- `PhoneBookInfoViewConfiguration` با `HasNoKey().ToView("PhoneBook_Info_View", "phonebook")` نگاشت می‌شود و ستون‌های دارای `_` را با `HasColumnName` وصل می‌کند؛ `base.Configure` کامنت شده تا فیلدهای Audit روی ویو اعمال نشوند — انتخاب درست.
- ویو در `MigrationManager.MigrateAsync` (`Core.Infrastructure/Database/MigrationManager.cs`) با `context.EnsureViews(...)` ساخته می‌شود؛ به همین دلیل ویو همیشه تازه است و داده‌اش کش نمی‌شود.
- اتصال فقط از مسیر `ISpecificationRepository<PhoneBookInfoView, Guid>` انجام می‌شود؛ `PhoneBookService` هیچ `DbContext`ای تزریق نمی‌کند. `PhoneBook_AddHealthChecks` هم خالی است و چیزی به `/health` اضافه نمی‌کند.

## ۵. وضعیت واقعی امروز (صادقانه)

- **ماژول موازی با Contact.** ماژول `Contact` عیناً همین قابلیت را دارد (`Contact.Domain/Entities/PhoneBookInfoView.cs`، `Contact.Infrastructure/Services/PhoneBookService.cs`، `Contact.Application/Queries/GetPhoneBookListQuery.cs`، `Contact.Presentation/Controllers/PhoneBookController.cs`) و حتی **همان مسیر** `api/Contact/PhoneBook/GetList` را استفاده می‌کند. اپ اصلی (`AkSteel.WebApi`) در `Modules:Enabled` ماژول `Contact` را دارد و `PhoneBook` را ندارد؛ پس نسخهٔ زنده همان کپی Contact است و اگر هر دو در یک هاست فعال شوند، خطای Ambiguous Match می‌گیریم.
- **قرارداد عمومی خالی است.** `Core.Application/Abstractions/Contact/IPhoneBookPublicService.cs` فقط `public interface IPhoneBookPublicService { }` است؛ تنها متد خواندن (`GetPhoneBookListAsync`) در `IPhoneBookInternalService` است که «داخلی» نام گرفته اما عملاً تنها راه استفاده است.
- **`ChangePartyEventHandler` هیچ کاری نمی‌کند.** فایل کاملاً کامنت شده و namespace آن `Sample.Application.EventHandlers` است (کپی از Sample)؛ پس هیچ مسیر رویدادی برای تازه‌سازی وجود ندارد و `organUnitId` هم بی‌اثر است.
- **تناقض اتصال دیتابیس.** ویو به `hr.*` و `people.*` اشاره می‌کند، ولی `PhoneBook.WebApi/appsettings.json` به دیتابیس `PhoneBookDb` وصل می‌شود. در SQL Server یک ویو نمی‌تواند به جدول‌های دیتابیس دیگر JOIN بزند؛ پس `EnsureView` تنها وقتی موفق می‌شود که اسکیماهای `hr` و `people` **در همان دیتابیس** باشند (کامنت‌های `DefaultConnection` در همان فایل به `AkSteelWelfarePlatformDb` اشاره می‌کنند).
- **با csprojهای فعلی build نمی‌شود.** `PhoneBook.Infrastructure.csproj` فقط `Core.Infrastructure` را رفرنس می‌دهد ولی از `PhoneBook.Application` و `PhoneBook.Domain` استفاده می‌کند؛ `PhoneBook.Presentation.csproj` فقط `Core.Presentation` و `Core.Shared` را دارد ولی `PhoneBook.Application.Queries` را using می‌کند. سه فایل هم `using Core.Application.Abstractions.PhoneBook;` دارند که چنین namespace‌ای وجود ندارد (نسخهٔ واقعی `Core.Application.Abstractions.Contact` است).
- **در Solution نیست.** هیچ‌کدام از پروژه‌های `PhoneBook.*` و `PhoneBook.WebApi` در `NexusPlatform.sln` نیستند؛ به همین دلیل خرابی‌های بالا در build روزمره دیده نمی‌شوند.
- **بدون احراز هویت.** `PhoneBookController` نه `[Authorize]` دارد و نه `[AuthorizeResource]`، در حالی که خروجی شامل شماره موبایل و داخلی همهٔ کارمندان است. (نکتهٔ مثبت: `NationalCode` در ویو خوانده می‌شود ولی در `PhoneBookEmploymentDto` نگاشت نمی‌شود.)
- **بدون تست.** پروژهٔ `PhoneBook.Test` وجود ندارد و `AddScoped<IPhoneBookInternalService, PhoneBookService>()` دو بار در DI ثبت شده است.

