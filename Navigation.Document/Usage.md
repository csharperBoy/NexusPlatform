# استفاده از ماژول Navigation در ماژول‌های دیگر

این فایل فقط یک سؤال را جواب می‌دهد: **ماژول من صفحه دارد؛ چطور منوی‌اش را به Navigation بدهم؟**

## ۱. خلاصهٔ در یک نگاه

| کاری که می‌خواهی | از چه چیزی استفاده کن |
|---|---|
| ثبت منوی ماژول خودم | یک `List<MenuDto>` در Seed ماژول بساز و به `IMenuPublicService.SyncModuleMenusAsync` بده |
| خواندن درخت منو در فرانت‌اند | `GET /api/navigation/menu/GetMenuTreeQuery` |
| خواندن درخت در کد (داخل ماژول Navigation) | `IMenuInternalService.GetByTreeStructure()` |
| حذف یک منو | ⚠️ API ندارد؛ باید ردیف اسکیمای `navigation` را دستی پاک کنی |

## ۲. قرارداد عمومی

```csharp
// مسیر: Core.Application/Abstractions/Navigation/PublicService/IMenuPublicService.cs
public interface IMenuPublicService
{
    Task SyncModuleMenusAsync(List<MenuDto> menus, CancellationToken cancellationToken);
}
```

> ⚠️ **نکتهٔ ساختاری:** این اینترفیس در `Core.Application` است، نه `Navigation.Application`. پس مصرف‌کننده به Core رفرنس می‌دهد. اگر روزی Core حذف شود، باید به `Navigation.Application` منتقل شود و نامش بماند تا مصرف‌کننده‌ها فقط یک رفرنس را عوض کنند.

## ۳. شکل دقیق `MenuDto` که باید بدهی

```csharp
// مسیر: Core.Shared/DTOs/Navigation/MenuDto.cs
public class MenuDto
{
    public Guid Id { get; set; }          // Navigation خودش پر می‌کند؛ تو خالی بگذار
    public string Title { get; set; }     // الزامی — متن دیده‌شده در منو
    public string Key { get; set; }       // الزامی — کلید یکتا (Db index unique)
    public string? Description { get; set; }
    public string Path { get; set; }      // مسیر فرانت‌اند، مثلاً "/Audit/Get"
    public string? Icon { get; set; }     // رشته؛ ⚠️ پایین را بخوان
    public int? Order { get; set; }       // ترتیب در همان سطح
    public Guid? ParentId { get; set; }   // Navigation خودش پر می‌کند
    public string? ParentKey { get; set; }// کلید والد؛ برای فرزندها الزامی
    public IReadOnlyList<MenuDto> Children { get; init; } = Array.Empty<MenuDto>();
}
```

قواعد فیلدها که `MenuService`/`MenuProcessor` عملاً اعمال می‌کنند:

| فیلد | قاعده |
|---|---|
| `Key` | باید یکتا باشد؛ در سازندهٔ `Menu` با `Trim().ToLowerInvariant()` ذخیره می‌شود ولی مقایسه با `Key.Trim()` انجام می‌شود. `Key` تکراری در دو ماژول = ردیف یکی روی دیگری به‌روزرسانی می‌شود. پیشنهاد: پیشوند ماژول (`audit`، `audit.get`، `scheduler.job`). |
| `Title` | خالی نباشد، وگرنه `Menu` سازنده `ArgumentException` می‌دهد و کل Sync می‌شکند. |
| `ParentKey` | برای ریشه `null` بگذار. برای فرزند، کلید والد **در همان لیست** (بهتر است) یا از قبل در دیتابیس. اگر پیدا نشود فقط Warning می‌گیرد و آیتم به‌عنوان ریشه ذخیره می‌شود. |
| `Children` | درخت را همین‌جا بده؛ `Flatten` خودش `ParentKey` فرزندان را از `Key` والد ست می‌کند. |
| `Order` | nullable است؛ مرتب‌سازی با `Order` بعد `Title`. |
| `Icon` | ⚠️ **همیشه مقدار معتبر enum بده.** `MenuService` از `def.Icon.ToEnumOrDefault(Icon.Default)` استفاده می‌کند و این متد با `Enum.TryParse` روی **نام** کار می‌کند. مقادیر مجاز فقط این‌ها هستند: `Default`، `User`، `Account`، `Folder`. اگر `Icon.Folder.GetIconString()` بدهی (`"fa-solid:folder"`)، پارس نمی‌شود و همه‌چیز `Default` می‌شود. |

## ۴. مسیر توصیه‌شده (نمونهٔ واقعی)

نمونهٔ زیر دقیقاً همان الگویی است که `Audit` و `Scheduler` استفاده می‌کنند:

```csharp
// مسیر: {Module}.Infrastructure/Data/{Module}SeedData.cs
using Core.Shared.DTOs.Navigation;
using Core.Shared.Enums.Navigation;

private static List<MenuDto> GetReportMenuDefinitions()
{
    return new List<MenuDto>
    {
        new()
        {
            Title = "مدیریت گزارش‌ها",
            Description = "گزارش‌های سیستم",
            Icon = Icon.Folder.ToString(),   // "Folder" — نام enum، نه رشتهٔ آیکون
            Order = 300,
            Key = "report",
            ParentKey = null,
            Path = "/Report",
            Children = new List<MenuDto>
            {
                new()
                {
                    Title = "گزارش روزانه",
                    Description = "گزارش روزانه فروش",
                    Icon = Icon.Folder.ToString(),
                    Order = 301,
                    Key = "report.daily",
                    ParentKey = "report",
                    Path = "/Report/Daily"
                }
            }
        }
    };
}

public static async Task SeedReportsForNavigationAsync(
    IMenuPublicService menuPublicService,
    ILogger logger,
    CancellationToken cancellationToken = default)
{
    var menus = GetReportMenuDefinitions();
    await menuPublicService.SyncModuleMenusAsync(menus, cancellationToken);
    logger.LogInformation("✅ Report menus synced successfully.");
}
```

و در `ModuleInitializer` ماژول:

```csharp
var menuService = scope.ServiceProvider.GetRequiredService<IMenuPublicService>();
await ReportSeedData.SeedReportsForNavigationAsync(menuService, _logger);
```

### سه شرط که اگر نباشند کار نمی‌کند

1. **`Navigation` باید در `Modules:Enabled` همان هاست باشد**، وگرنه `IMenuPublicService` در DI وجود ندارد و `GetRequiredService` استثنا می‌دهد (استارتاپ Fail می‌شود). اگر می‌خواهی ماژولت در هاستی بدون Navigation هم بالا بیاید، `GetService<IMenuPublicService>()` بگیر و `null` را چک کن.
2. **ترتیب `Order` را رعایت کن.** همهٔ سرویس‌های ماژول‌ها قبل از شروع Hosted Serviceها ثبت می‌شوند، پس `IMenuPublicService` بودن یا نبودن به `Order` بستگی ندارد؛ اما `ModuleInitializerها` به ترتیب `Order` اجرا می‌شوند و Migrationها (که در `UseEnableModulesApplicationBuilderExtensions` و **قبل** از `app.Run()` اجرا می‌شوند) هم به همین ترتیب‌اند. برای اطمینان، `Navigation` را با `Order` کوچک‌تر از ماژول خودت بگذار (در `AkSteel.WebApi` Navigation = `25`).
3. **جداول ساخته شده باشند.** `Navigation_UseInfrastructure` باید قبل از Seed اجرا شود (Migration با `IMigrationManager`).

## ۵. چه چیزی عمومی **نیست**

- `NavigationDbContext`، `Menu`، `MenuByKeySpec`، `MenuProcessor`، `MenuConfiguration` → داخلی Navigation هستند. اگر لازم شد منوها را بخوانی، `IMenuInternalService` را تزریق کن (اما آن هم فقط داخل ماژول Navigation معنا دارد).
- `Navigation.Infrastucture` (با غلط املایی) → **مرده است**؛ به آن رفرنس نده. فقط `Navigation.Infrastructure` در Solution است.
- `GetMenus(title)` عمومی است ولی پارامتر `title` را نادیده می‌گیرد؛ روی آن حساب نکن.

## ۶. قواعد

- **منو را در Seed خودت تعریف کن، نه در Navigation.** به فایل‌های Navigation دست نزن؛ اگر منوی ماژولت را آن‌جا اضافه کنی، دوباره همان وابستگی مرکزی که می‌خواستیم حذف شود برمی‌گردد.
- **`Key` را پایدار نگه دار.** `Key` هویت منو است؛ تغییرش = یک منوی جدید و یک ردیف یتیم قدیمی.
- **حذف را دستی انجام بده.** Sync چیزی را پاک نمی‌کند.
- **منو را با مجوز هم‌نام کن.** پیشنهاد: `Key` منو = `ResourceKey` مجوز (مثل `scheduler.job`). این‌طور `[AuthorizeResource("scheduler.job","View")]` و منو یک نام دارند و ماندگاری‌شان آسان‌تر است.
- **`Icon` را با نام enum بده، نه با `GetIconString()`.** (بهترین راه‌حل، ولی تا وقتی باگ `ToEnumOrDefault` در `MenuService` هست، این تنها راهی است که آیکون در دیتابیس درست ذخیره شود.)

## ۷. چک‌لیست وقتی می‌خواهی منوی یک ماژول جدید را اضافه کنی

1. `Navigation` در `Modules:Enabled` هست؟ (اگر نه، `GetService` + null-check.)
2. `Order` ماژول تو بعد از Navigation است؟
3. `Key`ها با پیشوند ماژول و یکتا هستند؟
4. برای هر فرزند `ParentKey` درست ست شده و والد هم در همان لیست هست؟
5. `Icon` نام enum است (`Folder` / `User` / `Account` / `Default`)؟
6. تست: برنامه را بالا بیاور و `GET /api/navigation/menu/GetMenuTreeQuery` را با کاربری که مجوز `navigation.menu:View` دارد صدا بزن؛ باید زیرشاخه‌ات را ببینی.
7. اگر منو را حذف کردی، یادت باشد ردیف `navigation.Menu` را هم پاک کنی.
