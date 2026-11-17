# ماژول Cach (کش)

> **در یک جمله:** Cach یک لایهٔ نازک و مشترک روی کش است؛ یک قرارداد پنج‌متدی به بقیهٔ ماژول‌ها می‌دهد و پشت آن یا `IMemoryCache` می‌نشیند یا Redis — مصرف‌کننده هیچ‌وقت نمی‌فهمد کدام.

| | |
|---|---|
| **پروژه‌ها** | `Cach.Domain` (بدون کد) / `Cach.Application` / `Cach.Infrastructure` / `Cach.Presentation` (بدون کد) — به‌همراه `Cach.Test` |
| **قرارداد عمومی** | `ICachePublicService` در `Core.Application/Abstractions/Caching/PublicService/ICachePublicService.cs`؛ `ICacheInternalService` در `Cach.Application/Interface/ICacheInternalService.cs` (در DI ثبت نمی‌شود) |
| **وابستگی به ماژول‌های دیگر** | هیچ ماژول دامنه‌ای؛ فقط `Core.Application` و `Core.Infrastructure` |
| **چه کسانی از آن استفاده می‌کنند** | Authorization، Identity، Navigation (دو کپی)، Contact، Sample و Audit (فقط تزریق، بدون استفاده) |
| **جایگاه پیشنهادی در مدل لایه‌ای** | لایه ۰ (سرویس فنی مشترک) |

---

## ۱. این ماژول به چه درد می‌خورد؟ (به زبان ساده)

بدون یک لایهٔ کش مشترک، هر ماژول برای خودش `MemoryCache` می‌سازد یا مستقیم به `IDistributedCache` وصل می‌شود: کلیدها بین ماژول‌ها تداخل می‌کنند، سیاست انقضا یک‌جا نیست و روزی که بخواهی از Redis به حافظهٔ پروسه برگردی باید ده فایل را دست بزنی. Cach همین را جمع می‌کند: یک اینترفیس که کلیدش همیشه `string` و مقدارش همیشه `T` است، به‌همراه دو پیاده‌سازی که هنگام راه‌اندازی و بر اساس `CacheSettings:UseRedis` یکی از آن‌ها انتخاب و ثبت می‌شود.

نکتهٔ مهم این است که انتخاب Redis یا حافظه، تصمیم زمان راه‌اندازی است نه تصمیم هر فراخوانی. الگوی مصرف هم cache-aside کلاسیک است: اول `GetAsync`، اگر `null` بود از دیتابیس بخوان و `SetAsync` کن؛ در مسیر نوشتن `RemoveAsync` یا `RemoveByPatternAsync`. نمونهٔ واقعی: `Sample.Infrastructure/Services/SampleQueryService.cs` و باطل‌کردن گروهی در `Contact.Application/EventHandlers/ChangePostEventHandler.cs`.

## ۲. چه چیزهایی داخلش هست؟

- **`ICachePublicService`** — قرارداد اصلی با پنج متد: `GetAsync<T>`، `SetAsync<T>`، `RemoveAsync`، `ExistsAsync`، `RemoveByPatternAsync`.
- **`ICacheInternalService`** — اینترفیس خالی که فقط از `ICachePublicService` ارث می‌برد و هیچ عضوی اضافه نمی‌کند.
- **`MemoryCacheService`** — روی `IMemoryCache`. برای `RemoveByPatternAsync` یک ایندکس استاتیک از کلیدها نگه می‌دارد و با Regex روی آن می‌گردد.
- **`RedisCacheService`** — روی `IDistributedCache` با `System.Text.Json` (camelCase). همهٔ متدها `try/catch` دارند: خطای Redis لاگ می‌شود و عملیات «انجام‌نشده» برمی‌گردد، نه Exception.
- **`CacheSettings`** — `UseRedis`، `DefaultExpirationMinutes` (۳۰) و `RedisInstanceName` (`MyApp`).
- **Health Check** — `.AddRedis(redis, name: "Redis")` به‌همراه `CacheHealthCheck` که یک کلید آزمایشی می‌نویسد، می‌خواند و زمان پاسخ را گزارش می‌کند.
- **تست** — `Cach.Test` چهار فایل: حافظه، Redis، Health Check و ثبت در DI.

## ۳. چه چیزی داخلش **نیست** (و نباید انتظارش را داشت)

- **DbContext، Migration یا اسکیمای اختصاصی ندارد**؛ داده‌ای را پایدار نمی‌کند و با خالی‌شدن Redis همه‌چیز می‌رود.
- **Controller و Endpoint ندارد**؛ `Cach.Presentation` فقط یک متد ثبت خالی دارد.
- **آمار کش ندارد** (hit ratio، حجم، زمان پاسخ) جز همان Health Check.
- **محافظت در برابر Cache Stampede ندارد**؛ ده درخواست هم‌زمان روی کلید خالی، ده بار دیتابیس را می‌خوانند.
- **باطل‌کردن توزیع‌شده ندارد**؛ با `UseRedis = false` هر پروسه کش خودش را دارد و Pub/Subی در کار نیست.
- **رمزنگاری، فشرده‌سازی و نسخه‌بندی مقدار کش ندارد.**

## ۴. یک نگاه فنی کوتاه

- کلیدها namespace اجباری ندارند؛ عرف پروژه این است که ماژول نام خودش را جلو بگذارد: `contact:phonebook:full-list` (نگاه کن به `Contact.Domain/Helper/CacheKeyHelper.cs`).
- با Redis، پیشوند `RedisInstanceName` به کلید اضافه می‌شود (رفتار خود `AddStackExchangeRedisCache`). `RemoveByPatternAsync` هم همین پیشوند را به الگو می‌زند، پس الگوی `contact:phonebook:*` روی کلیدهای واقعی `PhoneBook_contact:phonebook:...` می‌نشیند.
- `SetAsync` با TTL صریح، Absolute Expiration می‌گذارد؛ در Redis پیش‌فرض `DefaultExpirationMinutes` است و در حافظه، اگر TTL ندهی، ۳۰ دقیقه Sliding.
- ثبت با کنوانسیون پلتفرم انجام می‌شود (`Cach_AddApplication/AddInfrastructure/AddPresentation/AddHealthChecks/UseInfrastructure`) و لودر ماژول این متدها را به‌ترتیب Infrastructure → Application → Domain → Presentation صدا می‌زند. `Cach_AddDomain` وجود ندارد، چون `Cach.Domain` کد ندارد.
- در مدل لایه‌ای پیشنهادی (که فعلاً پیشنهاد است، نه وضعیت کد) لایه ۰ سرویس‌های عمومی مثل cache، audit، persistence و eventing است و Cach دقیقاً همین‌جاست: هیچ منطق کسب‌وکاری نمی‌داند.

## ۵. وضعیت واقعی امروز (صادقانه)

- **`ICacheInternalService` عملاً مرده است.** هر دو پیاده‌سازی آن را implement می‌کنند، اما DI آن‌ها را به `ICachePublicService` map می‌کند؛ تزریقش در زمان Resolve خطا می‌دهد. تنها جای استفاده‌اش همان دو کلاس پیاده‌سازی است.
- **قرارداد در Core است، نه در `Cach.Application`.** یعنی مصرف‌کننده به Core وابسته می‌شود و `Cach.Application` هیچ مصرف‌کننده‌ای ندارد — همان نشتی معروف.
- **ماژول تقریباً یک پروژه است.** `Cach.Domain` و `Cach.Presentation` هیچ کدی ندارند و `Cach_AddApplication`، `Cach_AddPresentation` و `Cach_UseInfrastructure` هر سه خالی‌اند؛ تنها چیزی که کار می‌کند `Cach_AddInfrastructure` است.
- **fallback به حافظه در عمل کار نمی‌کند.** `try/catch` دور `ConfigureRedisCache` فقط زمان *ثبت* اجرا می‌شود، ولی `ConnectionMultiplexer.Connect` داخل لامبدای Singleton است و تا اولین Resolve اجرا نمی‌شود؛ پس خطای Redis در اولین درخواست رخ می‌دهد و آن catch هیچ‌وقت مسیر حافظه را فعال نمی‌کند.
- **Health Check مستقل از تنظیمات است.** `Cach_AddHealthChecks` همیشه `.AddRedis(...)` را ثبت می‌کند حتی وقتی `UseRedis = false` است؛ اگر `ConnectionStrings:Redis` خالی باشد همان خط موقع راه‌اندازی استثنا می‌دهد. ضمناً `conn` خوانده و استفاده نمی‌شود و `AddDbContextCheck` کامنت است.
- **رفتار دو پیاده‌سازی یکی نیست.** `DefaultExpirationMinutes` فقط در Redis خوانده می‌شود؛ در حافظه عدد ۳۰ دقیقه hardcode و نوع انقضا Sliding است. عوض‌کردن `UseRedis` رفتار کش را عوض می‌کند، بدون اینکه کد مصرف‌کننده تغییر کند.
- **ایندکس کلیدهای حافظه نشتی دارد.** کلیدهای منقضی‌شده از `_keys` پاک نمی‌شوند و `RemoveByPatternAsync` کل ایندکس را با Regexی که از بیرون می‌آید اسکن می‌کند.
- **حذف الگویی روی Redis به یک سرور تکی وابسته است:** `GetServer(_redisMultiplexer.GetEndPoints().First())` در محیط چندسروره یا cluster قابل اتکا نیست.
- **لاگ‌ها پرحرف‌اند:** هر Get/Set/Remove در سطح Information، که در بار واقعی خودش هزینه است.
- **مصرف‌کننده‌ها:** Authorization (چهار سرویس و دو Processor)، Identity (دو سرویس)، Navigation (نسخهٔ `Infrastructure` و کپی غلط‌املای `Infrustructure`)، Contact (یک سرویس و هشت Event Handler) و Sample (دو سرویس). `Audit.Infrastructure/Services/AuditService.cs` هم آن را تزریق می‌کند ولی هرگز صدا نمی‌زند.
