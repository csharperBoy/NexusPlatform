# استفاده از ماژول Cach در ماژول‌های دیگر

این فایل فقط یک سؤال را جواب می‌دهد: **اگر ماژول من می‌خواهد چیزی را کش کند، چطور این کار را بکند که به Cach گره نخورد؟**

## ۱. خلاصهٔ در یک نگاه

| کاری که می‌خواهی | از چه چیزی استفاده کن |
|---|---|
| خواندن/نوشتن یک کلید | `ICachePublicService.GetAsync<T>` / `SetAsync<T>` |
| حذف یک کلید بعد از تغییر داده | `RemoveAsync(key)` |
| حذف گروهی (مثلاً بعد از تغییر یک موجودیت) | `RemoveByPatternAsync("people:person:*")` |
| انتخاب Redis یا حافظه | `CacheSettings:UseRedis` در appsettings — کد مصرف‌کننده تغییر نمی‌کند |
| دسترسی به `ICacheInternalService` | **نکن.** در DI ثبت نشده و Resolve آن خطا می‌دهد |

## ۲. قرارداد عمومی

```csharp
// مسیر: Core.Application/Abstractions/Caching/PublicService/ICachePublicService.cs
public interface ICachePublicService
{
    Task<T?> GetAsync<T>(string key);
    Task SetAsync<T>(string key, T value, TimeSpan? expiration = null);
    Task RemoveAsync(string key);
    Task<bool> ExistsAsync(string key);
    Task RemoveByPatternAsync(string pattern);
}
```

> ⚠️ **نکتهٔ ساختاری:** این اینترفیس امروز در `Core.Application` است، نه در `Cach.Application`. پس ماژول تو به‌جای وابستگی به Cach، به Core وابسته می‌شود. `Cach.Application/Interface/ICacheInternalService.cs` هم وجود دارد ولی فقط یک اینترفیس خالی است که هیچ‌جا ثبت نمی‌شود؛ سراغش نرو.

## ۳. مسیر توصیه‌شده

1. در پروژهٔ مصرف‌کننده (Application یا Infrastructure) به `Core.Application` رفرنس بده. به `Cach.Infrastructure` رفرنس نده.
2. `ICachePublicService` را در Constructor تزریق کن. ثبت در DI لازم نیست؛ خود `Cach_AddInfrastructure` (وقتی `Cach` در `Modules:Enabled` باشد) آن را ثبت کرده است.
3. الگوی cache-aside را رعایت کن: اول `GetAsync`، اگر `null` بود از Repository بخوان، بعد `SetAsync`.
4. در مسیر نوشتن، **بعد از** `SaveChangesAsync` کش را باطل کن، نه قبل از آن.
5. مقدار کش را یک DTO کوچک بگذار، نه موجودیت دامنه (دلیلش در بخش ۴).

نمونه (الگوی واقعی همین پروژه در `Sample.Infrastructure/Services/SampleQueryService.cs`):

```csharp
using Core.Application.Abstractions;                        // IRepository<,,> و IUnitOfWork<>
using Core.Application.Abstractions.Caching.PublicService;  // ICachePublicService
using Microsoft.Extensions.Logging;
using People.Domain.Entities;
using People.Infrastructure.Data;

namespace People.Infrastructure.Services;

// DTO سبک برای کش — نه خود NaturalPerson
public sealed record PersonSummary(Guid Id, string FullName, string NationalCode);

public sealed class PersonCacheReader
{
    private readonly IRepository<PeopleDbContext, NaturalPerson, Guid> _repo;
    private readonly IUnitOfWork<PeopleDbContext> _uow;
    private readonly ICachePublicService _cache;
    private readonly ILogger<PersonCacheReader> _logger;

    public PersonCacheReader(
        IRepository<PeopleDbContext, NaturalPerson, Guid> repo,
        IUnitOfWork<PeopleDbContext> uow,
        ICachePublicService cache,
        ILogger<PersonCacheReader> logger)
    {
        _repo = repo;
        _uow = uow;
        _cache = cache;
        _logger = logger;
    }

    public async Task<PersonSummary?> GetAsync(Guid id, CancellationToken ct)
    {
        var key = $"people:person:{id}";

        var cached = await _cache.GetAsync<PersonSummary>(key);
        if (cached is not null)
            return cached;

        var person = await _repo.GetByIdAsync(id);
        if (person is null)
            return null;

        var summary = new PersonSummary(
            person.Id,
            person.GetFullName(),
            person.NationalCode.ToString());

        await _cache.SetAsync(key, summary, TimeSpan.FromMinutes(10));
        return summary;
    }

    public async Task RenameAsync(Guid id, string firstName, string lastName, CancellationToken ct)
    {
        var person = await _repo.GetByIdAsync(id);
        if (person is null) return;

        person.SetFullName(firstName, lastName);
        await _uow.SaveChangesAsync(ct);   // اول دیتابیس

        await _cache.RemoveAsync($"people:person:{id}");          // بعد کش
        await _cache.RemoveByPatternAsync("people:person:list:*"); // و لیست‌های وابسته
    }
}
```

## ۴. قواعد

- **به Infrastructure وابسته نشو.** `MemoryCacheService`، `RedisCacheService` و `CacheHealthCheck` جزو API عمومی نیستند. `CacheSettings` را هم مستقیم نخوان؛ TTL را به‌عنوان پارامتر `expiration` پاس بده.
- **موجودیت دامنه را کش نکن.** `RedisCacheService` با `System.Text.Json` سریالایز می‌کند و موجودیت‌های این پروژه پراپرتی `private set` و Value Object دارند (`NationalCode`، `FullName`)؛ نتیجه یا استثناست یا آبجکت نیمه‌ساخته. DTO بساز.
- **کلید را namespace‌دار بگذار:** `{module}:{aggregate}:{id}` یا `{module}:{aggregate}:list:{filter}`. الگوی حذف گروهی (`*`) به همین ساختار تکیه می‌کند.
- **TTL بده.** بدون TTL، Redis مقدار `DefaultExpirationMinutes` (۳۰ دقیقه) و حافظه ۳۰ دقیقه Sliding اعمال می‌کند. برای داده‌های حساس به تازگی، TTL کوتاه و باطل‌کردن صریح را با هم داشته باش.
- **کش را منبع حقیقت نکن.** هیچ منطق کسب‌وکاری نباید به وجود کلید وابسته باشد؛ شکست کش باید فقط کندی باشد، نه خطا. `ExistsAsync` را هم برای تصمیم‌گیری جای `GetAsync` استفاده نکن (بین این دو کلید می‌تواند منقضی شود).
- **باطل‌کردن را در Event Handler بگذار، نه در Controller.** الگوی موجود پروژه همین است: `Contact.Application/EventHandlers/ChangePostEventHandler.cs` بعد از `ChangePostEvent` الگوی `contact:phonebook:*` را پاک می‌کند.
- **الگوی حذف را ساده نگه دار.** `*` و `?` کافی است؛ regex در سمت حافظه اجرا می‌شود و الگوی بد می‌تواند طولانی بگیرد.
- **در حلقه کش ننویس.** برای N آیتم یک کلید لیست بساز، نه N کلید.

## ۵. چک‌لیست وقتی می‌خواهی کش را به یک ماژول جدید اضافه کنی

1. آیا این داده واقعاً گران است و به‌اندازهٔ کافی پرتکرار خوانده می‌شود؟ اگر نه، کش نکن.
2. `ICachePublicService` را تزریق کردی (نه `ICacheInternalService`، نه `IMemoryCache`)?
3. کلید namespace‌دار و قابل پیش‌بینی است؟
4. TTL مشخص است و می‌دانی با کدام provider چه اتفاقی می‌افتد؟
5. مسیر نوشتن، کش را **بعد از** `SaveChangesAsync` باطل می‌کند؟
6. DTO کش‌شده قابل سریالایز/دی‌سریالایز است (تست کن: `Set` بعد `Get` همان مقدار را برگرداند)?
7. رفتار «کش خالی/کش خراب» را تست کردی؟ خروجی باید همان مسیر دیتابیس باشد، نه خطا.
8. اگر `UseRedis = true` است، کلیدها را در Redis با پیشوند `RedisInstanceName` دیدی و الگوی حذف هم با همان پیشوند کار می‌کند؟
9. مصرف‌کننده‌ها را با `grep` پیدا کن: `ICachePublicService`.
