# ماژول People (هویت اشخاص حقیقی و حقوقی)

> **در یک جمله:** People «کارت هویت» پلتفرم است؛ هر شخص حقیقی یا حقوقی که قرار است در سیستم‌های دیگر (استخدام، اطلاعات تماس، مجوزها، حساب کاربری) به او ارجاع داده شود، اول باید در این ماژول هویت بگیرد.

| | |
|---|---|
| **پروژه‌ها** | `People.Domain` / `People.Application` / `People.Infrastructure` / `People.Presentation` (و `People.Shared` که فقط `.csproj` دارد و هیچ فایلی ندارد) |
| **قرارداد عمومی** | `IPersonPublicService` — مسیر واقعی: `Core.Application/Abstractions/People/IPersonPublicService.cs` (نه در `People.Application`) |
| **وابستگی به ماژول‌های دیگر** | هنگام کامپایل فقط `Core.Domain`/`Core.Shared`/`Core.Application`/`Core.Infrastructure`/`Core.Presentation`. هنگام اجرا از طریق قراردادها: `IContactPublicService` (Contact) و `IPermissionPublicService` (Authorization) |
| **چه کسانی از آن استفاده می‌کنند** | `HR.Application` (‏`CreateEmploymentCommand`، `BatchCreateEmploymentsCommand`)، `HR.Infrastructure` (‏`EmploymentService`)، `HR.IrisaSync.Extention` (‏`SyncService`)، `Contact.Infrastructure` (‏`PhoneBookService`)، `Authorization` (‏`UserDataContextProvider`، `PermissionEvaluator`)، `Identity` (‏`UserService`) |
| **جایگاه پیشنهادی در مدل لایه‌ای** | لایه ۲ |

---

## ۱. این ماژول به چه درد می‌خورد؟ (به زبان ساده)

در هر سیستم سازمانی، قبل از هر چیز باید بدانیم «این آدم/شرکت کیست». People دقیقاً همین را جواب می‌دهد: یک هویت پایدار برای هر طرف (Party) می‌سازد و اطلاعات هویتی را روی آن سوار می‌کند.

مدل داده سه لایه دارد و این تفکیک، مهم‌ترین تصمیم طراحی ماژول است:

- **`Party`** — خود «طرف»: یک `Guid` که بقیه ماژول‌ها به آن ارجاع می‌دهند، به‌همراه `FkPermissionAssigneeId` (برای سیستم مجوزها) و `FkContactProfileId` (برای اطلاعات تماس). `Party` یک `IAggregateRoot` است.
- **`NaturalPerson`** — اطلاعات «ثابت» شخص حقیقی: `NationalCode` و `FullName` به‌صورت Value Object، به‌علاوه `BirthDate`، `BirthPlace`، `FatherName` و `Gender`. کامنت خودِ کلاس می‌گوید این‌ها «هرگز تغییر نمی‌کنند».
- **`LegalPerson`** — شخص حقوقی: `Title`، `Description` و `RegisterCode` (شناسه/شماره ثبت).
- **`NaturalPersonProfile`** — اطلاعات «قابل تغییر» به‌صورت نسخه‌دار: `EffectiveFrom`/`EffectiveTo`، `IsCurrent`، `Enablity`، `Visiblity`، `Remove` و فیلدهای مالکیت (`OwnerPersonId`، `OwnerUserId`، `OwnerPositionId`، `OwnerOrganizationUnitId` از `IOwnerableEntity`).
- **`PartiesRelation`** — رابطه بین دو طرف (`FkSourcePartyId` → `FkDestinationPartyId`) با نوع `PartiesRelationsType`: `Owner`، `LegalRepresentative`، `Parent`، `Child`، `Sibling`، `Spouse`، `Colleague`، `Friend`، `Other`. یعنی می‌شود گفت «این شرکت مالک آن شرکت است» یا «این شخص نماینده قانونی آن شخص حقوقی است».

نکتهٔ کلیدی: People **اطلاعات تماس نگه نمی‌دارد**؛ فقط `FkContactProfileId` را می‌گیرد و بقیه را به Contact می‌سپارد. همین مرز، ماژول را قابل نگهداری می‌کند.

## ۲. چه چیزهایی داخلش هست؟

- **`PersonService`** — تنها پیاده‌سازی واقعی، هم‌زمان `IPersonInternalService` و `IPersonPublicService`. منطق `CreatePersonAsync`: اول با کد ملی چک می‌کند شخص وجود دارد یا نه. اگر نه: `Party` می‌سازد (به‌همراه `ContactProfile` از نوع `Party` و همگام‌سازی موبایل/تلفن/آدرس/ایمیل)، `NaturalPerson` را اضافه می‌کند و یک `NaturalPersonProfile` جاری هم می‌سازد. اگر بله: با `ApplyChange` (extension روی `BaseEntity` با ماسک فیلدها) رکورد موجود را به‌روز می‌کند و همان `Id` را برمی‌گرداند. یعنی ایجاد، در عمل «upsert» است.
- **`CreatePersonCommand`** — تنها Command واقعی ماژول. جالب است که خودش `IPersonPublicService` را تزریق می‌کند، نه `IPersonInternalService`.
- **`UpdatePersonAsync`** — با `Optional<T>` کار می‌کند (فقط فیلدهای `IsSet` اعمال می‌شوند) و در انتها `ChangeNaturalPersonEvent` را ثبت می‌کند؛ ولی `SaveChangesAsync` را خودش صدا نمی‌زند.
- **Specification‌ها** — `GetNaturalPersonByPartyId` (استفاده می‌شود) و `GetPersonByNationalCodeSpec` (تعریف شده ولی مصرف نمی‌شود؛ جست‌وجوی کد ملی در `PersonService` به‌صورت inline نوشته شده است).
- **`PeopleDbContext`** — اسکیمای `people`، جدول تاریخچه `__PeopleMigrationsHistory`، `DbSet`های `Parties`، `naturalPerson`، `legalPersons`، `NaturalPersonProfiles`، `PartiesRelations` و `OutboxMessages`، به‌همراه ایندکس یکتای `IX_Persons_Unique_NationalCode` و ایندکس `IX_Persons_FullName_FastLookup`.
- **زیرساخت** — `People_AddInfrastructure` (DbContext، Repository و SpecificationRepository برای هر پنج موجودیت، ثبت `IPersonPublicService` روی `PersonService`، پردازشگر Outbox)، `People_AddHealthChecks` (چک `PeopleDatabase`) و `People_UseInfrastructure` (اجرای Migration).
- **API** — `PersonController` با یک Endpoint: `POST /api/People/Person/Create` و `[AuthorizeResource("people.person","Create")]`.

## ۳. چه چیزی داخلش **نیست** (و نباید انتظارش را داشت)

- حذف/غیرفعال‌سازی شخص ندارد. `DeletePersonCommand` و `UpdatePersonCommand` فقط کلاس‌های خالی `internal` هستند.
- خواندن ندارد: `GetPersonByIdQuery` و `GetPersonsSelectionListQuery` هم کلاس خالی‌اند. تنها راه خواندن، `IPersonPublicService` است.
- اعتبارسنجی کد ملی در لایهٔ دامنه نیست؛ `NationalCode.Create` صدا زده می‌شود ولی قانون «کد ملی تکراری نباشد» به ایندکس یکتای دیتابیس سپرده شده است.
- Merge/ادغام دو هویت، تاریخچهٔ تغییرات هویتی و حذف منطقی (`ISoftRemovable`) ندارد؛ `NaturalPerson` نسخه‌بندی نمی‌شود و فقط پروفایل نسخه‌دار است.
- نقش «کاربر» ندارد؛ اتصال شخص به حساب کاربری در Identity انجام می‌شود.

## ۴. یک نگاه فنی کوتاه

- ثبت در DI با کنوانسیون پلتفرم است، ولی **ناقص**: `People_AddInfrastructure`، `People_AddHealthChecks` و `People_UseInfrastructure` درست نام دارند؛ اما در `People.Application/DependencyInjection` متد `User_AddApplication` نام گرفته (باقی‌ماندهٔ کپی از ماژول User). لودر Reflection دنبال `People_AddApplication` می‌گردد، پس **هیچ‌وقت** اجرا نمی‌شود و هندلرهای MediatR ثبت نمی‌شوند.
- `People_AddPresentation` بدنهٔ خالی دارد؛ نه `AddControllers()` می‌زند و نه `AddApplicationPart`. یعنی اسمبلی کنترلرهای People به ASP.NET معرفی نمی‌شود و `POST /api/People/Person/Create` در عمل Route فعال ندارد.
- `People.Domain` فایل DI ندارد؛ پس `People_AddDomain` وجود ندارد و در لاگ راه‌اندازی فقط Warning «پیدا نشد» می‌بینی.
- `PersonService` سه SaveChanges مجزا دارد: `_uow.SaveChangesAsync()` روی `PeopleDbContext` و `_contactService.SaveAsync()` روی `ContactDbContext`. تراکنش مشترک وجود ندارد؛ اگر دومی شکست بخورد، Party و Person بدون اطلاعات تماس می‌مانند.
- Outbox فعال است، اما تنها رویداد تولیدی `ChangeNaturalPersonEvent` است و فقط از مسیر `UpdatePersonAsync` بیرون می‌آید — که امروز هیچ Endpoint‌ای آن را صدا نمی‌زند و تنها مصرف‌کننده‌اش HR و IrisaSync است.
## ۵. وضعیت واقعی امروز (صادقانه)

- **باگ قطعی در `CreatePartyAsync`:** `perAssigneeId` از `IPermissionPublicService` ساخته می‌شود ولی استفاده نمی‌شود و بعد `new Party(contactProfileId, contactProfileId)` نوشته شده است. سازندهٔ `Party` پارامترهایش `_FkPermissionAssigneeId` و `_FkContactProfileId` است؛ یعنی `FkPermissionAssigneeId` اشتباهاً برابر `ContactProfileId` می‌شود. نتیجه: `GetPersonPermissionAssigneeIdAsync` عدد بی‌معنا برمی‌گرداند و ارزیابی مجوز بر اساس Party از روز اول خراب است.
- **دو نقص نام‌گذاری که ماژول را از کار می‌اندازد:** `User_AddApplication` و `People_AddPresentation` خالی. تا این دو اصلاح نشود، People از بیرون قابل استفاده نیست و فقط به‌عنوان سرویس تزریقی برای HR/Contact کار می‌کند.- **قرارداد در جای اشتباه:** `IPersonPublicService` در `Core.Application/Abstractions/People/` است، نه در `People.Application`. هر مصرف‌کننده به‌جای رفرنس به People، به Core وابسته می‌شود. همچنین امضای متد شامل `List<PhoneNumber>`، `List<Email>` و `Optional<T>` از `Core.Domain`/`Core.Shared` است؛ پس قرارداد کاملاً از Core جدا نیست.
- **Null Object شکسته است:** `NullPersonService` در `Core.Application/Abstractions/People/NullPeopleServices.cs` دو اورلود `CreatePersonAsync` دارد (یکی با `PhoneNumber` تکی که در اینترفیس نیست و `NotImplementedException` می‌اندازد)، `UpdatePersonAsync` را با امضای قدیمی (رشته‌ای) هم پیاده کرده، و `GetNaturalPersonIdAsync` به‌جای `Task.FromResult<Guid?>(null)` فقط `return null;` دارد که در زمان `await` باعث `NullReferenceException` می‌شود. این کلاس فقط برای این ساخته شده که بدون People برنامه بالا بیاید، ولی در عمل خودش خطا تولید می‌کند.
- **`Party.FkPermissionAssigneeId` و `Party.FkContactProfileId` نه ایندکس دارند نه FK صریح.** `PartyConfiguration` فقط `ToTable("Parties","people")` است؛ هیچ ایندکسی روی `FkContactProfileId` که در Joinهای Contact استفاده می‌شود تعریف نشده.
- **`NaturalPersonProfile` بدون مصرف‌کننده ساخته می‌شود:** `PersonService` یک نمونه درج می‌کند ولی هیچ سرویس یا Query‌ای آن را نمی‌خواند؛ فیلدهای `MaritalStatus`/`NumberOfChildren` هم کامنت شده‌اند.
- **هیچ Test‌ای برای People وجود ندارد** (`People.Test` در فهرست پروژه‌ها نیست) در حالی که People پایهٔ هویتی است و خطایش به همه ماژول‌های دیگر نشت می‌کند. `People.Shared` هم فقط `.csproj` دارد و در Solution ثبت شده است؛ یا باید پر شود یا حذف.
