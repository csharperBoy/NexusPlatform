# استفاده از ماژول Base در ماژول‌های دیگر

این فایل جواب یک سؤال است: **از Base چه چیزی می‌توانم بگیرم و چطور؟**

خلاصهٔ صادقانه: **تقریباً هیچ.** Base امروز نه قرارداد عمومی دارد و نه سرویس قابل تزریق. تنها مصرف‌کنندهٔ واقعی‌اش فرانت‌اند است که یک Endpoint را صدا می‌زند. پس این فایل بیشتر «چه کاری نکن» است تا «چطور استفاده کن».

## ۱. خلاصهٔ در یک نگاه

| کاری که می‌خواهی | از چه چیزی استفاده کن |
|---|---|
| بفهمی در این هاست کدام ماژول‌ها فعال‌اند | `GET /api/base/setting/GetActiveModules` (فرانت) یا `ModuleHelper.GetActiveModules()` (بک) |
| فعال بودن یک ماژول مشخص را چک کنی | `ModuleHelper.IsActive(ModuleEnum.X)` — از `Core.Application.Helper`، نه از Base |
| یک تنظیم را در دیتابیس نگه داری | ❌ Base چنین چیزی ندارد؛ از appsettings + `IOptions<T>` استفاده کن |
| سرویس مشترکی تزریق کنی | ❌ `IBasePublicService` وجود ندارد |

## ۲. آن‌چه واقعاً قابل استفاده است

### الف) `ModuleHelper` (که در Core است، نه در Base)

```csharp
// مسیر: Core.Application/Helper/ModuleHelper.cs
public static class ModuleHelper
{
    public static void Initialize(ModuleSettings settings);          // Core خودش صدا می‌زند
    public static bool IsActive(ModuleEnum module);
    public static int? GetOrder(ModuleEnum module);
    public static List<ModuleItem> GetActiveModules();
    public static Dictionary<ModuleEnum, int> GetActiveModulesWithOrder();
}
```

نمونهٔ استفاده در Seed خودت:

```csharp
if (ModuleHelper.IsActive(ModuleEnum.Navigation))
{
    // فقط اگر Navigation در این هاست فعال است، منو ثبت کن
}
```

> ⚠️ `IsActive` روی `Modules:Enabled` کار می‌کند که با **نام** ماژول تطبیق داده می‌شود. `ModuleEnum` عضوی به نام `Base` یا `Sample` ندارد؛ برای این دو ماژول نمی‌توانی گارد بگذاری.

### ب) Endpoint فهرست ماژول‌های فعال

```
GET /api/base/setting/GetActiveModules
→ 200 OK   Result<List<ModuleItem>>
```

مدل پاسخ (`Core.Application.Models.ModuleItem`) چیزی شبیه `Name`، `Order` و `Extentions` است؛ فرانت‌اند فقط `name` را می‌خواند:

```ts
// FrontEnd/src/core/context/ModuleContext.tsx
const modules = await settingApi.GetActiveModules();
setActiveModules(new Set(modules.map(m => m.name)));
```

**دو نکتهٔ مهم:**
1. این Endpoint **`[Authorize]` ندارد** (اتربیوت مجوزش کامنت شده است). پس برای فیلتر UI خوب است، ولی **هرگز** آن را مبنای امنیت نگیر.
2. اگر ماژول تو UI دارد، مسیر درست این است که Routeهایت را در فرانت‌اند با همین `activeModules` فیلتر کنی، نه اینکه خودت یک Endpoint مشابه بسازی.

## ۳. چطور در ماژول خودت از همین الگو استفاده کنی

اگر ماژول تو یک قابلیت «سراسری» دارد که فرانت‌اند باید بداند، **به Base چیزی اضافه نکن**. الگوی توصیه‌شده:

1. Endpoint را در ماژول خودت بساز (مثلاً `GET /api/mymodule/settings/...`).
2. در فرانت‌اند یک API client مثل `settingApi.ts` بساز و در `ModuleContext` یا کانتکست خودت مصرف کن.
3. اگر داده باید در کل پلتفرم مشترک باشد، آن را در **Core** بگذار نه Base (Core تنها جایی است که همه به آن رفرنس دارند).

نمونهٔ درست (ایجاد یک Endpoint در ماژول خودت):

```csharp
[ApiController]
[Route("api/mymodule/[controller]")]
public class MySettingsController : BaseController
{
    [HttpGet("Ping")]
    [AuthorizeResource("mymodule.settings", "View")]   // ← برخلاف Base، مجوز بده
    public IActionResult Ping() => Ok(Result<string>.Ok("pong"));
}
```

## ۴. چه چیزی عمومی **نیست**

- `BaseDbContext`، `BaseSeedData`، `ModuleInitializer`، `BaseDbContextFactory` → داخلی.
- `Base.Infrastructure` به‌طور کلی: هیچ سرویسی برای مصرف بیرونی ندارد.
- `Base.Presentation` → فقط همان یک کنترلر؛ قابل رفرنس ولی بی‌فایده برای ماژول دیگر.

## ۵. قواعد

- **به Base وابسته نشو.** هر وابستگی به Base یعنی وابستگی به ماژولی که امروز چیزی ندارد و فردا ممکن است حذف شود.
- **تنظیمات را در appsettings نگه دار.** الگوی پلتفرم `IOptions<T>` با یک `SectionName` است (مثل `SmtpOptions` و `SchedulerOptions`).
- **برای گارد کردن Seed از `ModuleHelper` استفاده کن، نه از وجود/نبود سرویس.** `GetRequiredService<IBaseX>` روی ماژولی که ممکن است فعال نباشد، استارتاپ را می‌شکند.
- **فهرست ماژول‌های فعال = دادهٔ UI، نه دادهٔ امنیتی.** همیشه مجوز را سمت سرور چک کن.

## ۶. چک‌لیست

اگر فکر می‌کنی «این چیز باید در Base باشد»، اول این‌ها را جواب بده:

1. آیا واقعاً **همهٔ** ماژول‌ها به آن نیاز دارند؟ اگر نه، در Core یا در ماژول خودت.
2. آیا نیاز به **دیتابیس** دارد؟ اگر بله، پس تنظیمات پویا می‌خواهی — که Base امروز ندارد و ساختنش یعنی تعریف `Setting`، Command/Query، Migration، منو و مجوز. کار کوچکی نیست.
3. آیا فقط یک مقدار از appsettings می‌خواهی؟ پس `IOptions` کافی است.
4. اگر ماژول را از `Modules:Enabled` برداری، کد تو باید بدون آن هم کامپایل و اجرا شود؟
5. آیا مجوز Endpoint را گذاشتی؟ (Base این کار را نکرده؛ تو نکن.)
