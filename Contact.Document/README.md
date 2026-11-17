# ماژول Contact (پروفایل تماس، کانال‌های ارتباطی و دفترچه تلفن)

> **در یک جمله:** Contact جایی است که «راه تماس گرفتن با یک چیز» ذخیره می‌شود؛ هر Party، هر استخدام، هر پست سازمانی و هر مکان یک پروفایل تماس دارد و شماره/ایمیل/آدرس‌هایش به‌صورت کانال‌های نسخه‌دار به آن پروفایل وصل می‌شوند — و روی همین داده‌ها، نمای «دفترچه تلفن» ساخته می‌شود.

| | |
|---|---|
| **پروژه‌ها** | `Contact.Domain` / `Contact.Application` / `Contact.Infrastructure` / `Contact.Presentation` |
| **قرارداد عمومی** | `IContactPublicService` و `IPhoneBookPublicService` — مسیر واقعی: `Core.Application/Abstractions/Contact/` (نه در `Contact.Application`) |
| **وابستگی به ماژول‌های دیگر** | هنگام کامپایل به `HR.Application` (و به‌واسطهٔ آن `People.Application`/`People.Domain`)؛ هنگام اجرا به `IEmploymentPublicService`، `IPostPublicService`، `ILocationPublicService` (HR) و `IPersonPublicService` (People) از طریق Core |
| **چه کسانی از آن استفاده می‌کنند** | `People.Infrastructure` (‏`PersonService`)، `HR.Infrastructure` (‏`EmploymentService`، `PostService`، `LocationService`)، `HR.IrisaSync.Extention` (‏`SyncService`) و خودِ Contact (‏`PhoneBookService`) |
| **جایگاه پیشنهادی در مدل لایه‌ای** | لایه ۲ |

---

## ۱. این ماژول به چه درد می‌خورد؟ (به زبان ساده)

فرض کن یک کارمند داریم. سه جور اطلاعات تماس برایش معنا دارد: موبایل شخصی، تلفن داخلی محل کار، و تلفن اتاق جلسات واحدش. اگر همه را در یک ستون `Phone` روی جدول کارمند بریزی، بعد از سه ماه نه می‌فهمی کدام مال کی بود، نه می‌توانی تاریخچه نگه داری.

Contact این را با سه موجودیت حل می‌کند:

- **`ContactResource`** — *کاتالوگ* مقدارها: یک شماره یا ایمیل یا آدرس، با `ContactType`، `Label` (مثل «موبایل شخصی»)، `IsPrimary`، `SortOrder`، `ParentId` (برای داخلی‌های متصل به خط اصلی) و `RelationType` (`PublicPhoneNumber`، `ExtensionOf`، `Alternative`). یکتایی مقدار در سطح نوع، در `ContactService` با `GetContactResourceSpec` چک می‌شود.
- **`ContactProfile`** — *صاحب* تماس: `Title`، `ProfileType` (از `ContactProfileTypeEnum`: `Party`/`Employment`/`Post`/`Location`)، `IsActive` و فیلدهای مالکیت.
- **`ContactProfileAssignment`** — *اتصال نسخه‌دار* بین پروفایل و منبع، با `EffectiveFrom`/`EffectiveTo`/`IsCurrent`. اینجا همان جایی است که «تاریخچه» به‌دست می‌آید: به‌جای حذف، `DoExpire()` می‌خورد.

و در بالاترین لایه، **`PhoneBookInfoView`** یک Read Model بدون کلید (`HasNoKey().ToView("PhoneBook_Info_View","contact")`) است که برای هر کارمند، شماره‌های شخصی/استخدام/پست/مکان را کنار هم می‌گذارد.

## ۲. چه چیزهایی داخلش هست؟

- **`ContactService`** (`IContactInternalService` : `IContactPublicService`) — قلب ماژول. `CreateContactProfileAsync` پروفایل می‌سازد؛ `SyncProfileContacts(type, values, profileId)` تفاضل مقدارهای فعلی و جدید را حساب می‌کند: انتساب‌های حذف‌شده `DoExpire` می‌شوند، مقدارهای جدید در کاتالوگ جست‌وجو و در صورت نبود ساخته می‌شوند، سپس انتساب تازه ثبت می‌شود. `GetContactsByProfilesIdsAsync` هم انتساب‌ها را با `ContactResource` و فرزندانش برمی‌گرداند.
- **`ContactResourceService`** — CRUD مستقیم روی کاتالوگ منابع (برای صفحهٔ «اطلاعات تماس»)، هم‌زمان `IContactResourceCommandService` و `IContactResourceQueryService`.
- **`PhoneBookService`** — نمای دفترچه تلفن. از `IEmploymentPublicService.GetFullInfoAsync()` لیست کارمندان را می‌گیرد، پروفایل استخدام/طرف/پست/مکان را جمع می‌کند، یک‌جا `GetContactsByProfilesIdsAsync` می‌زند، با `ToPhoneBookDtos` نگاشت می‌کند، کارمندهای بدون تماس را حذف می‌کند و در آخر **پست‌ها و مکان‌های بدون کارمند** را هم اضافه می‌کند تا ساختمان بدون کارمند در دفترچه گم نشود.
- **Event Handlerها** — شش هندلر `DomainEventHandler<T>`: `Change/RemoveEmployment`، `Change/RemovePost`، `Change/RemoveLocation` و `ChangeNaturalPerson` و `ChangeContactProfileResources`. کار همه تقریباً یکی است: پاک‌کردن الگوی کش `contact:phonebook:*`؛ `RemoveEmploymentEventHandler` علاوه بر آن پروفایل را `DeActive` و همهٔ انتساب‌ها را `Expire` می‌کند.
- **دستورهای دسته‌ای** — `BatchUpdateEmploymentsContactCommand`، `BatchUpdatePostsContactCommand`، `BatchUpdateLocationsContactCommand`: این‌ها **منطق تماس ندارند** و فقط `IEmploymentInternalService.UpdateEmploymentAsync` / `IPostInternalService.UpdatePostAsync` / `ILocationInternalService.UpdateLocationAsync` را با `OfficePhone`/`OrgEmail`/`OrgMobile` صدا می‌زنند.
- **زیرساخت** — `ContactDbContext` با اسکیمای `contact` و `__contactMigrationsHistory`، `Contact_AddInfrastructure`، `Contact_AddApplication` (MediatR)، `Contact_AddDomain` (خالی)، `Contact_AddHealthChecks` (**خالی**)، `Contact_UseInfrastructure`، و `ModuleInitializer` که منابع `contact.post`، `contact.employment`، `contact.location`، `contact.contactresource` و منوی «مدیریت اطلاعات تماس» را Seed می‌کند.
- **API** — پنج کنترلر زیر `api/Contact/...`: `ContactResourceController`، `EmploymentContactController`، `LocationContactController`، `PostContactController` و `PhoneBookController` (`GET /api/Contact/PhoneBook/GetList`).

## ۳. چه چیزی داخلش **نیست** (و نباید انتظارش را داشت)

- اعتبارسنجی شماره/ایمیل ندارد؛ `SyncProfileContacts` هر رشته‌ای را می‌پذیرد و فقط `ContactType` را می‌داند.
- تشخیص تکراری در سطح «شخص» ندارد؛ یک شماره می‌تواند به چند پروفایل وصل باشد (که عمداً همین است) ولی هشدار «این شماره قبلاً برای کس دیگری ثبت شده» وجود ندارد.
- جست‌وجو/فیلتر دفترچه تلفن ندارد. `organUnitId` در `GetPhoneBookListAsync` گرفته می‌شود ولی **هیچ‌جا استفاده نمی‌شود**.
- مجوز سطح فیلد یا Redaction ندارد؛ هر کسی که به Endpoint دسترسی داشته باشد همهٔ شماره‌ها را می‌بیند.
- ذخیره‌سازی فایل/عکس پروفایل ندارد (`ProfilePictureUrl` فقط یک رشته است).
- Outbox دارد و پردازشگرش ثبت می‌شود، ولی تنها رویداد تولیدی `ChangeContactProfileResourcesEvent` با `ContactProfileId` است و شناسهٔ رکورد واقعی را حمل نمی‌کند.

## ۴. یک نگاه فنی کوتاه

- **جهت وابستگی:** `Contact.Application.csproj` به `HR.Application.csproj` رفرنس دارد و در `using`ها هم `HR.Application.Interfaces` و `HR.Domain.Entities` دیده می‌شود. یعنی Contact در سطح **اینترفیس‌های داخلی** HR قفل شده است، نه فقط قراردادهای عمومی Core. این جهت، همان چیزی است که مدل لایه‌ای را می‌شکند.
- `Contact.Application` به‌طور غیرمستقیم `People.Domain.Events` را هم می‌بیند (چون `HR.Application` → `People.Application` → `People.Domain`). این یعنی `ChangeNaturalPersonEvent` بدون رفرنس صریح مصرف می‌شود.
- `Contact_AddHealthChecks` بدنهٔ خالی دارد؛ `ContactDatabase` هیچ Health Checkی ندارد، در حالی که People و HR دارند.
- در `Contact_AddInfrastructure` چند سرویس **دو بار** ثبت می‌شوند (`AddScoped<IPhoneBookInternalService, PhoneBookService>()` هم با factory و هم مستقیم؛ همین برای `IContactInternalService` و `IContactResource*`). نتیجه‌اش خطا نیست ولی دو Registration هم‌نوع برای یک سرویس باقی می‌ماند.
- `Contact.Presentation` چهار بار پشت سر هم `AddControllers().AddApplicationPart(...)` را تکرار می‌کند؛ یک `AddApplicationPart(typeof(PhoneBookController).Assembly)` کافی است.
- `ContactDbContext.EnsureViews` کامنت شده است؛ ویو `PhoneBook_Info_View` در اسکیمای `contact` ساخته نمی‌شود و `PhoneBookInfoView` (و `_PhoneBookSpecRepository` تزریق‌شده در `PhoneBookService`) **بدون مصرف** مانده‌اند. اگر روزی کسی این ویو را کوئری کند و در دیتابیس نباشد، خطا می‌گیرد.

## ۵. وضعیت واقعی امروز (صادقانه)

- **دو پیاده‌سازی دفترچه تلفن وجود دارد و فقط یکی زنده است.** `Contact` یک `PhoneBookService` فعال دارد (در `AkSteel.WebApi` با `Order: 90`) و یک ماژول مستقل `PhoneBook.*` هم روی دیسک هست که **در `NexusPlatform.sln` ثبت نشده**، به `Core.Application.Abstractions.PhoneBook` رفرنس می‌دهد که چنین Namespace‌ای وجود ندارد، و ویو خودش را در اسکیمای `phonebook` می‌سازد. یعنی PhoneBook امروز کد مرده و غیرقابل کامپایل است. **مرجع رسمی، `Contact` است.**
- **برخورد Route بالقوه:** هر دو `PhoneBookController` (در Contact و در PhoneBook) روی `[Route("api/Contact/[controller]")]` با `GET GetList` هستند؛ فعال‌شدن هم‌زمان‌شان در یک Host باعث Route Ambiguous می‌شود. `PhoneBook.WebApi` هم PhoneBook را فعال می‌کند ولی Contact را نه.
- **کش نوشته می‌شود ولی خوانده نمی‌شود:** در `GetPhoneBookListAsync` بلوک خواندن کش کامنت شده، اما در انتها `_cacheService.SetAsync(cacheKey, result, TimeSpan.FromDays(30))` اجرا می‌شود. یعنی هر درخواست کل لیست را می‌سازد، رویدادها کش را invalidate می‌کنند و هیچ‌کس از کش نمی‌خواند: هزینه بدون فایده.
- **`IPhoneBookPublicService` خالی است.** یک اینترفیس بدون هیچ عضوی در `Core.Application/Abstractions/Contact/IPhoneBookPublicService.cs` — یعنی «قرارداد عمومی دفترچه تلفن» عملاً وجود ندارد و مصرف‌کنندهٔ بیرونی مجبور است `IPhoneBookInternalService` (داخلی، در `Contact.Application`) را تزریق کند یا مستقیم به Controller بزند.
- **کشتن پروفایل بدون تراکنش:** `RemoveEmploymentEventHandler` سه کار می‌کند (`DeActive`، `ExpireAll`، `SaveAsync`) و بعد کش را پاک می‌کند؛ ولی چون HR و Contact دو DbContext جدا دارند، اگر Save دوم شکست بخورد رویداد در Outbox می‌ماند و ممکن است دوباره اجرا شود — Handler باید idempotent باشد و امروز نیست.
- **Null Object ناقص:** `NullContactPublicServices.SyncProfileContacts` و `GetContactsByProfilesIdsAsync` هر دو `NotImplementedException` می‌اندازند. یعنی با Contact غیرفعال، هر مسیری که People یا HR به تماس نیاز دارد می‌شکند.
- **`ContactProfileTypeEnum` و `ContactTypeEnum` در `Core.Shared.Enums.Contact` هستند، نه در `Contact.Domain`.** نتیجه: People و HR برای نوشتن `ContactTypeEnum.Mobile` به قرارداد مشترک وابسته‌اند — که برای یک Enum دامنه‌ای زیاد است.
