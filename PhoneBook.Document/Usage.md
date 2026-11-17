# استفاده از ماژول PhoneBook در ماژول‌های دیگر

این فایل فقط یک سؤال را جواب می‌دهد: **اگر ماژول من فهرست تماس کارمندان را لازم دارد، چطور آن را از PhoneBook بگیرم؟**

## ۱. خلاصهٔ در یک نگاه

| کاری که می‌خواهی | از چه چیزی استفاده کن |
|---|---|
| گرفتن فهرست کارمندان با شماره‌هایشان | `IPhoneBookInternalService.GetPhoneBookListAsync(...)` |
| گرفتن همان داده از راه دور (HTTP) | `GET api/Contact/PhoneBook/GetList` |
| استفاده از `IPhoneBookPublicService` | ❌ ممکن نیست؛ این اینترفیس هیچ متدی ندارد |
| نوشتن یا ویرایش شماره تلفن | ❌ در حیطهٔ PhoneBook نیست؛ از `Contact` یا `HR` استفاده کن |

> ⚠️ **نکتهٔ ساختاری:** قرارداد «عمومی» (`IPhoneBookPublicService`) امروز در `Core.Application/Abstractions/Contact/IPhoneBookPublicService.cs` تعریف شده و **خالی** است. تنها متدی که کار می‌کند در `PhoneBook.Application/Interfaces/IPhoneBookInternalService.cs` است. یعنی برای استفادهٔ واقعی، به‌جای قرارداد عمومی، به اینترفیس «داخلی» وابسته می‌شوی — همین الان این تناقض را بدان و در بازنگری معماری مطرح کن.

## ۲. قرارداد عمومی

```csharp
// مسیر: PhoneBook.Application/Interfaces/IPhoneBookInternalService.cs
public interface IPhoneBookInternalService : IPhoneBookPublicService
{
    Task<IReadOnlyList<PhoneBookEmploymentDto>> GetPhoneBookListAsync(Guid? organUnitId);
}
```

```csharp
// مسیر: PhoneBook.Application/DTOs/PhoneBookEmploymentDto.cs
public class PhoneBookEmploymentDto
{
    public string EmploymentCode { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string FullName { get; }
    public string? OrganizationUnitsName { get; set; }
    public string? JobTitleName { get; set; }
    public string? JobLevelTitle { get; set; }
    public string? LocationTitle { get; set; }
    public List<ContactDetailDto> Contacts { get; set; }
    public string ContactSummary { get; }        // شماره‌ها با " - "
    public bool HasMultipleContacts { get; }
}

public class ContactDetailDto
{
    public string Title { get; set; }            // "داخلی" | "موبایل سازمانی"
    public string Value { get; set; }
    public PhoneBookContactTypeEnum Type { get; set; }    // Mobile | Phone | Email | Fax | Address
    public PhoneBookContactSourceEnum Source { get; set; } // Personal | employment | post
}
```

## ۳. مسیر توصیه‌شده

1. در ماژول مصرف‌کننده فقط `PhoneBook.Application` را رفرنس بده (نه `PhoneBook.Infrastructure` و نه `PhoneBook.Presentation`).
2. `IPhoneBookInternalService` را در Constructor تزریق کن. ثبت در DI لازم نیست؛ خودِ `PhoneBook_AddInfrastructure` آن را ثبت کرده است.
3. خروجی را **بلافاصله** به DTO خودت نگاشت کن و نوع PhoneBook را از لایهٔ خودت بیرون نده.

نمونه:

```csharp
using PhoneBook.Application.DTOs;
using PhoneBook.Application.Interfaces;

public record DirectoryEntryDto(string Code, string Name, string Unit, string Phones);

public class GetDirectoryQueryHandler : IRequestHandler<GetDirectoryQuery, Result<IReadOnlyList<DirectoryEntryDto>>>
{
    private readonly IPhoneBookInternalService _phoneBook;
    private readonly ILogger<GetDirectoryQueryHandler> _logger;

    public GetDirectoryQueryHandler(
        IPhoneBookInternalService phoneBook,
        ILogger<GetDirectoryQueryHandler> logger)
    {
        _phoneBook = phoneBook;
        _logger = logger;
    }

    public async Task<Result<IReadOnlyList<DirectoryEntryDto>>> Handle(
        GetDirectoryQuery request, CancellationToken ct)
    {
        // organUnitId فعلاً نادیده گرفته می‌شود؛ null بده و خودت فیلتر کن
        var list = await _phoneBook.GetPhoneBookListAsync(null);

        var result = list
            .Select(x => new DirectoryEntryDto(
                x.EmploymentCode,
                x.FullName,
                x.OrganizationUnitsName ?? "-",
                x.ContactSummary))
            .ToList();

        _logger.LogDebug("Directory returned {Count} entries", result.Count);
        return Result<IReadOnlyList<DirectoryEntryDto>>.Ok(result);
    }
}
```

اگر ماژول تو در همان هاست اجرا نمی‌شود، تنها راه HTTP است:

```
GET /api/Contact/PhoneBook/GetList
```

## ۴. قواعد

- **فقط‌خواندنی است.** هیچ Command‌ای وجود ندارد؛ تلاش برای نوشتن یعنی وابستگی به `PhoneBookDbContext` که ممنوع است.
- **`PhoneBookDbContext`، `PhoneBookInfoView`، `GetPhoneBookSpec` و `PhoneBookMappingExtensions` جزو API عمومی نیستند.** اگر لازم شد داده را جور دیگری ببینی، درخواستت را به PhoneBook بده، نه اینکه به ویو وصل شوی.
- **کلید نداری.** `PhoneBookInfoView` بدون کلید است و DTO هم `Id` ندارد؛ شناسهٔ عملی هر ردیف `EmploymentCode` است. برای ارجاع به یک شخص از `PersonId`/`EmploymentId` خودِ People/HR استفاده کن.
- **`organUnitId` بی‌اثر است.** فیلتر واحد سازمانی در `GetPhoneBookSpec` کامنت شده؛ اگر فیلتر می‌خواهی، خودت روی خروجی اعمال کن یا اول این باگ را درست کن.
- **`Contacts` ممکن است خالی باشد.** ویو از LEFT JOIN ساخته می‌شود؛ برای کارمند بدون تماس، لیست خالی و `ContactSummary` تهی برمی‌گردد. Null-check را جدی بگیر.
- **در حلقه صدا نزن.** هر فراخوانی کل ویو را می‌خواند؛ یک بار بگیر و در متغیر نگه دار. اگر مسیر داغ است، Cache را در ماژول خودت اضافه کن.
- **روی ویو ستون اضافه نکن.** ستون‌ها متعلق به `hr` و `people` هستند؛ تغییرشان کار PhoneBook (و در واقع HR/People) است.
- **`NationalCode` را از ویو نخواه.** در DTO نگاشت نشده و عمداً به خروجی نمی‌رسد.

## ۵. چک‌لیست وقتی می‌خواهی از PhoneBook استفاده کنی

1. آیا واقعاً فهرست تماس لازم داری، یا می‌توانی از `IEmploymentPublicService`/`IPersonPublicService` استفاده کنی؟
2. چه چیزی تزریق می‌کنی؟ `IPhoneBookInternalService` (تنها گزینهٔ کارکننده).
3. کدام پروژه را رفرنس دادی؟ فقط `PhoneBook.Application`.
4. خروجی را به DTO خودت نگاشتی و `PhoneBookEmploymentDto` را از مرز ماژول رد نکردی؟
5. برای `Contacts` حالت خالی را تست کردی؟
6. اگر ماژول تو در هاست دیگری است، مسیر HTTP و نبود احراز هویت روی آن را در نظر گرفتی؟
