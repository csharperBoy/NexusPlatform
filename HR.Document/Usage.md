# استفاده از ماژول HR در ماژول‌های دیگر (و ساخت افزونه برای آن)

این فایل دو سؤال را جواب می‌دهد: **اگر ماژول من به داده‌های سازمانی نیاز دارد، چطور آن را از HR بگیرم؟** و **اگر HR باید به یک سیستم خارجی وصل شود، افزونه را کجا و چطور بنویسم؟**

## ۱. خلاصهٔ در یک نگاه

| کاری که می‌خواهی | از چه چیزی استفاده کن |
|---|---|
| لیست کامل کارمندان با پست و مکان (برای دفترچه تلفن/گزارش) | `IEmploymentPublicService.GetFullInfoAsync()` |
| پیدا کردن `EmploymentId` از `PersonId` | `IEmploymentPublicService.GetEmploymentId(personId)` |
| گرفتن پست‌های یک استخدام | `IPostPublicService.GetEmploymentPostsId(employmentId)` |
| گرفتن واحدهای سازمانی یک استخدام | `IPostPublicService.GetEmploymentOrganizeId(employmentId)` |
| گرفتن پست/مکان با `ContactProfileId` | `IPostPublicService.GetByContactProfileIds(...)`, `ILocationPublicService.GetByContactProfileIds(...)` |
| نوشتن (ایجاد/ویرایش/حذف) | ❌ در قرارداد عمومی نیست؛ از Commandهای MediatR استفاده کن |

## ۲. قراردادهای عمومی

```csharp
// مسیر واقعی: Core.Application/Abstractions/HR/
public interface IEmploymentPublicService
{
    Task<IEnumerable<EmploymentFullDto>> GetFullInfoAsync();
    Task<Guid?> GetEmploymentId(Guid? personId);
    Task SaveAsync();
}

public interface IPostPublicService
{
    Task<IEnumerable<PostInfoDto>> GetByContactProfileIds(List<Guid> postProfileIds);
    Task<List<Guid?>?> GetEmploymentOrganizeId(Guid? employmentId);
    Task<List<Guid>?> GetEmploymentPostsId(Guid? employmentId);
    Task<List<Guid>?> GetEmploymentPostsPermissionAssigneeId(Guid? employmentId);
}

public interface ILocationPublicService
{
    Task<List<LocationInfoDto>> GetByContactProfileIds(List<Guid> postProfileIds);
    Task SaveAsync();
}
```

> ⚠️ **نکتهٔ ساختاری:** هر سه در `Core.Application` تعریف شده‌اند، نه در `HR.Application`. `EmploymentFullDto`، `PostInfoDto` و `LocationInfoDto` هم در `Core.Shared/DTOs/HR` هستند. پس مصرف‌کننده به Core وابسته می‌شود؛ اگر Core حذف شود، این قراردادها باید به `HR.Application` منتقل شوند.

## ۳. مسیر توصیه‌شده

1. در ماژول مصرف‌کننده فقط `Core.Application` را رفرنس بده. اگر مجبوری `HR.Application` را هم رفرنس بدهی، یعنی داری از API داخلی استفاده می‌کنی و لایه‌بندی می‌شکند (Contact امروز همین اشتباه را کرده).
2. سرویس عمومی را در Constructor تزریق کن؛ `HR_AddInfrastructure` آن‌ها را روی `EmploymentService`/`PostService`/`LocationService` ثبت می‌کند.
3. `SaveAsync` را فقط اگر واقعاً نوشتی صدا بزن. خواندن‌ها Save لازم ندارند.
4. برای نوشتن، Command بفرست. مثلاً `IMediator.Send(new CreateEmploymentCommand(...))` — همان کاری که افزونهٔ IrisaSync می‌کند.

نمونهٔ واقعی (خلاصهٔ `Contact.Infrastructure/Services/PhoneBookService.cs`):

```csharp
public async Task<IReadOnlyList<PhoneBookEmploymentDto>> GetPhoneBookListAsync(Guid? organUnitId)
{
    // ۱) همهٔ کارمندان با پست و مکان
    IEnumerable<EmploymentFullDto> empList = await _employmentservice.GetFullInfoAsync();

    // ۲) همهٔ پروفایل‌های تماس درگیر
    var profileIds = empList.Where(e => e.ProfileId.HasValue).Select(e => e.ProfileId!.Value)
        .Concat(empList.Where(e => e.PartyProfileId.HasValue).Select(e => e.PartyProfileId!.Value))
        .Concat(empList.SelectMany(e => e.posts.Select(p => p.ProfileId)))
        .Concat(empList.SelectMany(e => e.empLocations.Select(l => l.ProfileId)))
        .Distinct().ToList();

    // ۳) یک‌بار خواندن تماس‌ها
    var contacts = await _contactService.GetContactsByProfilesIdsAsync(profileIds);

    return empList.ToPhoneBookDtos(contacts).Where(e => e.Contacts.Any()).ToList();
}
```

## ۴. قواعد

- **`IEmploymentInternalService`، `IPostInternalService`، `ILocationInternalService`، `IJobTitleInternalService`، `IJobLevelInternalService`، `IOrganizationUnitInternalService` و `IHRUnitOfWork<HRDbContext>` داخلی‌اند.** تزریقشان از ماژول دیگر یعنی وابستگی به پیاده‌سازی.
- **`HRDbContext`، `HRUnitOfWork`، `Employment`، `Post`، `Location`، `Assignment` و Specificationها عمومی نیستند.** اگر داده‌ای کم داری، DTO یا متد را در قرارداد Core اضافه کن.
- **`EmploymentInfoView` و `PostInfoView` ویو دیتابیس‌اند.** اگر دیتابیس تازه ساخته شده و `HR_UseInfrastructure` اجرا نشده، خواندن‌ها خطا می‌دهند.
- **`GetFullInfoAsync` تمام کارمندان را یک‌جا می‌خواند.** برای سازمان بزرگ، این متد را در حلقه صدا نزن و نتیجه را کش کن.
- **`GetEmploymentPostsPermissionAssigneeId` را باور نکن.** `Post.FkPermissionAssigneeId` هیچ‌جا مقدار نمی‌گیرد و خروجی همیشه `Guid.Empty` است.
- **حذف، حذف نرم است.** بعد از `DeleteEmploymentCommand` رکورد در دیتابیس می‌ماند و انتساب‌ها/مکان‌ها `DoExpire` می‌شوند؛ در Queryهایت `IsRemove` را فیلتر کن.

## ۵. چطور برای HR افزونه بنویسی

1. پروژه را دقیقاً با نام `HR.{نام‌افزونه}.Extention` بساز (حرف `t` در `Extention` اجباری است؛ لودر از همین الگو اسمبلی می‌سازد).
2. دو متد Static بساز: `{نام‌افزونه}_AddDependency(this IServiceCollection, IConfiguration)` و `{نام‌افزونه}_UseDependency(this IApplicationBuilder)`. نمونهٔ زنده: `IrisaSync_AddDependency` و `IrisaSync_UseDependency`.
3. `DbContext` اختصاصی خودت را با ارث‌بری از `Base_DbContext` بساز و **اسکیمای جدا یا جدول‌های پیشوندی** انتخاب کن؛ `IrisaExtentionDbContext` عمداً در اسکیمای `hr` با جدول تاریخچهٔ جدا (`__IrisaSyncMigrationsHistory`) است.
4. در `appsettings.json` زیر همان ماژول اضافه کن:

```json
{ "Name": "HR", "Order": 80,
  "Extentions": [ { "Name": "IrisaSync", "Order": 85 } ] }
```

5. اگر Migration داری، در `{نام‌افزونه}_UseDependency` معادل `RunSmartMigrations` را صدا بزن.
6. برای تغییر دادهٔ HR، **مستقیم به جدول ننویس**؛ Command بساز و با `IMediator.Send` اجرا کن — همان الگوی `SyncEmploymentsPreviewAsync`/`ApplyEmploymentsAsync`.
7. برای Preview، از `SyncCommandBundle<TCreate,TUpdate,TDelete>` و `SyncPreviewItem<T>` (در `HR.IrisaSync.Extention/Interface/ISyncService.cs`) تقلید کن تا فرانت‌اند بتواند تغییرات را قبل از اعمال نشان دهد.
8. اعتبارنامه‌ها را از `ConnectionStrings`/Secret Store بخوان؛ هرگز مثل `IrisaOracleDbContext` در کد Hard-code نکن.

## ۶. چک‌لیست وقتی می‌خواهی یک قابلیت جدید به HR اضافه کنی

1. آیا واقعاً ساختار سازمانی است یا داده کسب‌وکار دیگر؟ حقوق و حضور و غیاب جای‌شان HR نیست.
2. متد خواندن را در `I...PublicService` (در `Core.Application/Abstractions/HR`) و پیاده‌سازی‌اش در سرویس مربوطه اضافه کن.
3. `NullHrServices.cs` را هم به‌روز کن — و بدان که `NullLocationService` امروز **ثبت نمی‌شود**؛ اگر HR را غیرفعال کنی، `ILocationPublicService` در DI وجود ندارد و Contact هنگام Resolve خطا می‌دهد.
4. برای نوشتن، Command + Handler بنویس و `{...}InternalService` را تزریق کن، نه Repository را.
5. اگر `SaveAsync` را در متد جدید صدا نمی‌زنی، در XML Doc بنویس که فراخواننده مسئول ذخیره است (الگوی `AssignToEmploymentAsync`).
6. مصرف‌کننده‌ها را با `grep` پیدا کن: `IEmploymentPublicService`، `IPostPublicService`، `ILocationPublicService`.
7. تست: ایجاد استخدام تکراری با همان کد پرسنلی (باید به‌روزرسانی شود) + انتساب هم‌زمان دو نفر به یک پست (باید رفتار مستند داشته باشد) + حذف استخدام (انتساب‌ها و مکان‌ها باید Expire شوند).
