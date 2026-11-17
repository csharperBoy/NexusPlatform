# استفاده از ماژول Contact در ماژول‌های دیگر

این فایل فقط یک سؤال را جواب می‌دهد: **اگر موجودیت من باید شماره تلفن/ایمیل/آدرس داشته باشد، چطور آن را به Contact بدهم بدون اینکه به دیتابیس یا سرویس داخلی‌اش وابسته شوم؟**

## ۱. خلاصهٔ در یک نگاه

| کاری که می‌خواهی | از چه چیزی استفاده کن |
|---|---|
| ساختن پروفایل تماس برای موجودیت خودت | `IContactPublicService.CreateContactProfileAsync(title, type)` |
| نوشتن/همگام‌سازی شماره‌ها و ایمیل‌ها | `IContactPublicService.SyncProfileContacts(type, values, profileId)` |
| خواندن همهٔ تماس‌های چند پروفایل | `IContactPublicService.GetContactsByProfilesIdsAsync(profileIds)` |
| ذخیرهٔ تغییرات | `IContactPublicService.SaveAsync()` |
| دیدن دفترچه تلفن (فقط UI) | `GET /api/Contact/PhoneBook/GetList` |

## ۲. قرارداد عمومی

```csharp
// مسیر واقعی: Core.Application/Abstractions/Contact/IContactPublicService.cs
public interface IContactPublicService
{
    Task<Guid> CreateContactProfileAsync(string Title, ContactProfileTypeEnum Type,
        CancellationToken cancellationToken = default);

    Task<bool> SyncProfileContacts(ContactTypeEnum type, List<string>? values, Guid profileId);

    Task<List<ContactItemDto>> GetContactsByProfilesIdsAsync(List<Guid> profilesId);

    Task SaveAsync();
}
```

- `ContactProfileTypeEnum` و `ContactTypeEnum` در `Core.Shared.Enums.Contact` تعریف شده‌اند (نه در `Contact.Domain`). مقدارهای نوع تماس که در کد استفاده می‌شوند: `Mobile`، `Phone`، `Email`، `Address`، `OrganizationMobile`، `OfficePhone`، `Fax`.
- `ContactItemDto` در `Core.Shared.DTOs.Contact` است و `ProfileId`، `ContactType`، `Source`، `Value`، `Label`، `EffectiveFrom`/`EffectiveTo`، `IsCurrent` و `ChildContactItems` دارد.

> ⚠️ **نکتهٔ ساختاری:** هر دو اینترفیس `IContactPublicService` و `IPhoneBookPublicService` در `Core.Application` تعریف شده‌اند، نه در `Contact.Application`. پس مصرف‌کننده به Core وابسته می‌شود. `IPhoneBookPublicService` هم **هیچ عضوی ندارد**؛ یک نشانگر خالی است و برای خواندن دفترچه تلفن به کار نمی‌آید.

## ۳. مسیر توصیه‌شده

1. در ماژول مصرف‌کننده فقط `Core.Application` را رفرنس بده. به `Contact.Infrastructure` یا `Contact.Presentation` رفرنس نده.
2. `IContactPublicService` را در Constructor تزریق کن؛ `Contact_AddInfrastructure` آن را روی `ContactService` ثبت می‌کند.
3. `FkContactProfileId` را روی موجودیت خودت نگه دار. این الگویی است که People (`Party.FkContactProfileId`)، HR (`Employment`/`Post`/`Location` هر سه) استفاده می‌کنند.
4. اول پروفایل بساز، بعد `SyncProfileContacts` بزن، و `SaveAsync` را **یک‌بار** در انتهای Workflow صدا بزن.
5. برای هر نوع تماس، جدا `SyncProfileContacts` بزن؛ متد فقط یک `ContactTypeEnum` می‌گیرد.

نمونهٔ واقعی (همان کاری که `HR.Infrastructure/Services/LocationService.cs` می‌کند):

```csharp
public async Task<Guid> CreateLocationAsync(
    string _title, Guid? _parentId = null,
    List<string>? _orgPhone = null, List<string>? _orgEmail = null, List<string>? _orgMobile = null)
{
    // ۱) پروفایل تماس با نوع Location
    Guid contactProfileId = await _contactService.CreateContactProfileAsync(
        $"Location - {_title}", ContactProfileTypeEnum.Location);

    // ۲) موجودیت خودت، با ارجاع به همان پروفایل
    Location loc = new Location(_title, _parentId, contactProfileId);
    await _LocationRepository.AddAsync(loc);

    // ۳) همگام‌سازی کانال‌ها
    await _contactService.SyncProfileContacts(ContactTypeEnum.OrganizationMobile, _orgMobile, loc.FkContactProfileId);
    await _contactService.SyncProfileContacts(ContactTypeEnum.OfficePhone, _orgPhone, loc.FkContactProfileId);
    await _contactService.SyncProfileContacts(ContactTypeEnum.Email, _orgEmail, loc.FkContactProfileId);

    return loc.Id;
}

public async Task SaveAsync()
{
    await _uow.SaveChangesAsync();        // DbContext خودت
    await _contactService.SaveAsync();    // ContactDbContext — تراکنش مشترک وجود ندارد
}
```

## ۴. قواعد

- **`SyncProfileContacts` را «همگام‌سازی» بخوان، نه «افزودن».** چیزی که در لیست جدید نیست `DoExpire` می‌شود. اگر `null` پاس بدهی، مجموعه خالی تلقی می‌شود و همهٔ انتساب‌های فعال آن نوع منقضی می‌شوند — این تفاوت با `[]` را جدی بگیر.
- **`ContactProfile` و `ContactResource` و `ContactProfileAssignment` را دستی نساز.** ساخت انتساب بدون چک کردن کاتالوگ، رکورد تکراری در `ContactResources` تولید می‌کند.
- **`IContactInternalService` را از ماژول دیگر تزریق نکن.** متدهای `DeActiveContactProfileAsync` و `ExpireAllContactAsync` مخصوص چرخهٔ حذف داخل Contact هستند (مصرف‌کنندهٔ واقعی: `RemoveEmploymentEventHandler`).
- **`ContactDbContext`، `PhoneBookInfoView`، `GetPhoneBookSpec` و `ContactService` جزو API عمومی نیستند.** اگر داده اضافه‌تری خواستی، متد جدید را در `IContactPublicService` اضافه کن.
- **به کش تکیه نکن.** `contact:phonebook:*` نوشته می‌شود ولی خواندنش کامنت شده است؛ رفتار امروز «همیشه تازه، همیشه کند» است.
- **رویدادها را مصرف کن، نه تولید.** Contact به `ChangeEmploymentEvent`، `ChangePostEvent`، `ChangeLocationEvent`، `Remove*` و `ChangeNaturalPersonEvent` گوش می‌دهد. اگر ماژول تو Cache دارد، الگوی `DomainEventHandler<T>` را در اسمبلی Application خودت تکرار کن.
- **ترتیب ماژول‌ها را رعایت کن.** Contact باید بعد از People و HR در `Modules:Enabled` بیاید (`Order` بزرگ‌تر)، وگرنه ثبت‌های واقعی جای Null Objectها را نمی‌گیرند.

## ۵. چک‌لیست وقتی می‌خواهی یک «صاحب تماس» جدید اضافه کنی

1. آیا واقعاً صاحب جدید است یا می‌شود از یکی از چهار نوع موجود (`Party`/`Employment`/`Post`/`Location`) استفاده کرد؟ اگر نه:
2. یک عضو به `ContactProfileTypeEnum` در `Core.Shared/Enums/Contact` اضافه کن (تغییر مشترک است؛ همهٔ ماژول‌ها را تحت تأثیر می‌گذارد).
3. متد لازم را در `IContactPublicService` و `ContactService` اضافه کن؛ `NullContactPublicServices` را هم به‌روز کن (امروز دو متدش `NotImplementedException` می‌اندازند).
4. اگر باید در دفترچه تلفن دیده شود، `PhoneBookService.GetStandaloneLocationsAndPostsAsync` و `PhoneBookMappingExtensions` را گسترش بده.
5. منبع و مجوز و منو را در `ContactSeedData` (یا Seed ماژول خودت) اضافه کن — به Seedهای Contact دست نزن.
6. مصرف‌کننده‌ها را با `grep` پیدا کن: `IContactPublicService`، `SyncProfileContacts`.
7. تست: پروفایل جدید + `SyncProfileContacts` با لیست خالی (باید همه منقضی شوند) + دو بار Sync با همان مقدار (نباید انتساب تکراری بسازد).
