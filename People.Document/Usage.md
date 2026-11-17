# استفاده از ماژول People در ماژول‌های دیگر

این فایل فقط یک سؤال را جواب می‌دهد: **اگر ماژول من به هویت یک شخص نیاز دارد، چطور آن را از People بگیرم؟**

## ۱. خلاصهٔ در یک نگاه

| کاری که می‌خواهی | از چه چیزی استفاده کن |
|---|---|
| ساخت یا به‌روزرسانی شخص با کد ملی | `IPersonPublicService.CreatePersonAsync(...)` |
| به‌روزرسانی جزئی فیلدهای هویتی | `IPersonPublicService.UpdatePersonAsync(id, Optional<...>...)` |
| گرفتن `PersonId` از `PartyId` | `IPersonPublicService.GetNaturalPersonIdAsync(partyId)` |
| گرفتن شناسهٔ مالک مجوز (Assignee) | `IPersonPublicService.GetPersonPermissionAssigneeIdAsync(personId)` یا `GetPartyPermissionAssigneeIdAsync(partyId)` |
| ذخیرهٔ تغییرات | `IPersonPublicService.SaveAsync()` |

## ۲. قرارداد عمومی

```csharp
// مسیر واقعی: Core.Application/Abstractions/People/IPersonPublicService.cs
public interface IPersonPublicService
{
    Task<Guid> CreatePersonAsync(string nationalCode, string firstName, string lastName,
        DateTime? birthDate = null, string? birthPlace = null, string? fatherName = null,
        Gender? gender = null,
        List<PhoneNumber>? Phone = null, List<string>? Address = null,
        List<Email>? Email = null, List<PhoneNumber>? Mobile = null,
        string? createBy = null);

    Task<bool> UpdatePersonAsync(Guid id,
        Optional<string> firstlName, Optional<string> lastName,
        Optional<DateTime?> birthDate, Optional<string?> birthPlace,
        Optional<string?> fatherName, Optional<string?> nationalCode,
        Optional<Gender?> gender,
        Optional<List<PhoneNumber>?> Phone, Optional<List<string>?> Address,
        Optional<List<Email>?> Email, Optional<List<PhoneNumber>?> Mobile);

    Task<Guid?> GetNaturalPersonIdAsync(Guid? partyId);
    Task<Guid?> GetPersonPermissionAssigneeIdAsync(Guid? personId);
    Task<Guid?> GetPartyPermissionAssigneeIdAsync(Guid? partyId);
    Task SaveAsync();
}
```

> ⚠️ **نکتهٔ ساختاری:** این اینترفیس امروز در `Core.Application` تعریف شده، نه در `People.Application`. پس مصرف‌کننده به‌جای وابستگی به People، به Core وابسته می‌شود. اگر Core حذف شود، اینترفیس باید به `People.Application` منتقل شود و نامش `IPersonPublicService` بماند.

## ۳. مسیر توصیه‌شده

1. در ماژول مصرف‌کننده فقط `Core.Application` را رفرنس بده (تا زمان انتقال قرارداد). به `People.Infrastructure` یا `People.Presentation` رفرنس نده.
2. `IPersonPublicService` را در Constructor تزریق کن. ثبت در DI لازم نیست؛ `People_AddInfrastructure` خودش آن را به `PersonService` وصل می‌کند و باید در `Modules:Enabled` با `Order` کمتر از ماژول تو باشد.
3. `CreatePersonAsync` را به‌عنوان «upsert» در نظر بگیر: اگر کد ملی وجود داشته باشد، همان `Id` برمی‌گردد و اطلاعات قبلی به‌روز می‌شود. برای همین برای «آیا قبلاً بود؟» به خروجی اکتفا نکن؛ اگر این تفکیک مهم است، خودت `GetNaturalPersonIdAsync` را صدا بزن.
4. `SaveAsync()` را **یک‌بار** در انتهای Workflow صدا بزن، نه بعد از هر فراخوانی. خودِ `CreatePersonAsync` چیزی را ذخیره نمی‌کند.
5. برای تغییرات جزئی از `UpdatePersonAsync` با `Optional<T>` استفاده کن. برای «تغییر نده» از `Optional<T>.Undefined` و برای «مقدار را پاک کن» از `Optional<T>.Of(null)` استفاده کن — نه از `null`.

نمونهٔ واقعی (همان الگویی که `HR.Application/Commands/Employment/CreateEmploymentCommand.cs` استفاده می‌کند):

```csharp
public class CreateEmploymentCommandHandler : IRequestHandler<CreateEmploymentCommand, Result<Guid>>
{
    private readonly IPersonPublicService _personService;
    private readonly IEmploymentInternalService _employmentService;
    private readonly ILogger<CreateEmploymentCommandHandler> _logger;

    public CreateEmploymentCommandHandler(
        IPersonPublicService personService,
        IEmploymentInternalService employmentService,
        ILogger<CreateEmploymentCommandHandler> logger)
    {
        _personService = personService;
        _employmentService = employmentService;
        _logger = logger;
    }

    public async Task<Result<Guid>> Handle(CreateEmploymentCommand request, CancellationToken ct)
    {
        // ۱) هویت: اگر شخص با این کد ملی نبود، ساخته می‌شود
        Guid personId = await _personService.CreatePersonAsync(
            request.NationalCode, request.FirstName, request.LastName,
            request.BirthDate, request.BirthPlace, request.FatherName, request.Gender,
            Phone: null, Address: request.Address, Email: null, Mobile: null);

        // ۲) استخدام روی همان PersonId
        Guid employmentId = await _employmentService.CreateEmploymentAsync(
            request.EmploymentCode, personId, request.EmploymentTypeId,
            request.EmploymentStatusId, request.StartDate, request.EndDate);

        // ۳) یک‌بار ذخیره — هر دو DbContext جدا هستند
        await _employmentService.SaveAsync();

        return Result<Guid>.Ok(employmentId);
    }
}
```

## ۴. قواعد

- **وابسته به Infrastructure نشو.** `PeopleDbContext`، `PersonService`، `NaturalPerson`، `Party` و Specificationها جزو API عمومی نیستند. `IPersonInternalService` هم امروز **خالی** است و چیزی به قرارداد عمومی اضافه نمی‌کند؛ پس تزریقش بی‌فایده است.
- **`Party` را مستقیم نساز.** ساختن `Party` نیازمند هماهنگی با Authorization (Assignee) و Contact (ContactProfile) است و همان جایی است که امروز باگ دارد. همیشه از `CreatePersonAsync` رد شو.
- **کد ملی را نرمال کن.** `PersonService` با `Trim` مقایسه می‌کند ولی فراخواننده مسئول یکسان‌سازی است؛ عدد فارسی/لاتین یا صفر ابتدایی را خودت اصلاح کن.
- **`SaveAsync` را در حلقه صدا نزن.** دو DbContext جدا (`people` و `contact`) درگیرند و هر Save یک رفت‌وبرگشت دیتابیس است.
- **خطا را به خطای دامنه تبدیل کن.** `CreatePersonAsync` برای کد ملی نامعتبر (`NationalCode.Create`) استثنا می‌اندازد؛ آن را به `Result.Fail` تبدیل کن، همان‌طور که الگوی `Result<T>` پلتفرم می‌خواهد.
- **`ChangeNaturalPersonEvent` را مصرف کن، نه تولید.** اگر ماژول تو Cache یا Read Model دارد، `DomainEventHandler<ChangeNaturalPersonEvent>` بنویس؛ امروز Contact دقیقاً همین کار را می‌کند. تولید این رویداد وظیفهٔ People است.
- **به `FkPermissionAssigneeId` فعلی اعتماد نکن.** تا باگ `CreatePartyAsync` اصلاح نشود، این مقدار برابر `ContactProfileId` است و مقایسه‌های مجوز بی‌معنا می‌شوند.

## ۵. چک‌لیست وقتی می‌خواهی یک قابلیت جدید به People اضافه کنی

1. آیا واقعاً هویت است یا داده کسب‌وکاری؟ «وضعیت تأهل» و «تعداد فرزند» جای‌شان `NaturalPersonProfile` است، ولی «حقوق پایه» جای‌اش HR است.
2. متد جدید را در `IPersonPublicService` (و پیاده‌سازی‌اش در `PersonService`) اضافه کن.
3. `NullPersonService` را هم به‌روز کن — وگرنه با People غیرفعال، برنامه با `NullReferenceException` می‌خوابد.
4. **متد DI را با پیشوند `People_` بنویس** (`People_AddApplication`). نام اشتباه یعنی MediatR هندلرهایت را ثبت نمی‌کند.
5. اگر Presentation اضافه می‌کنی، `People_AddPresentation` باید `AddControllers().AddApplicationPart(...)` بزند؛ الان خالی است.
6. مصرف‌کننده‌ها را با `grep` پیدا کن: `IPersonPublicService`.
7. تست: ایجاد شخص جدید + ایجاد شخص تکراری با کد ملی یکسان (باید upsert شود) + مسیر خطای کد ملی نامعتبر.
