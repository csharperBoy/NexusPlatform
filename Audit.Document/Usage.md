# استفاده از ماژول Audit در ماژول‌های دیگر

این فایل فقط یک سؤال را جواب می‌دهد: **اگر ماژول من کاری انجام داد که ارزش ثبت دارد، چطور آن را به Audit بدهم؟**

## ۱. خلاصهٔ در یک نگاه

| کاری که می‌خواهی | از چه چیزی استفاده کن |
|---|---|
| ثبت یک اتفاق مهم | `IAuditPublicService.LogAsync(...)` |
| دیدن لاگ‌ها (فقط تیم فنی/ادمین) | `GET /api/audit/AuditLogs` یا `IAuditInternalService.GetRecentLogsAsync(...)` |
| ثبت خودکار همهٔ Requestهای MediatR | همین حالا فعال است (`AuditBehavior`)؛ کاری لازم نیست |

## ۲. قرارداد عمومی

```csharp
// مسیر: Core.Application/Abstractions/Auditing/PublicService/IAuditPublicService.cs
public interface IAuditPublicService
{
    Task LogAsync(
        string action,        // "Create" | "Update" | "Delete" | ...
        string entityName,    // مثلاً "Person" یا نام Command
        string entityId,      // شناسهٔ واقعی رکورد (نه Guid تصادفی)
        Guid? userId,
        object? changes = null); // هر آبجکتی؛ خودش JSON می‌شود
}
```

> ⚠️ **نکتهٔ ساختاری:** این اینترفیس امروز در `Core.Application` تعریف شده، نه در `Audit.Application`. پس ماژول مصرف‌کننده به‌جای وابستگی به Audit، به Core وابسته می‌شود. اگر Core حذف شود، اینترفیس باید به `Audit.Application` منتقل شود (و نامش `IAuditPublicService` بماند) تا مصرف‌کننده فقط به `Audit.Application` رفرنس بدهد.

## ۳. مسیر توصیه‌شده

1. در ماژول مصرف‌کننده فقط `Audit.Application` (یا تا زمان حذف Core، همان `Core.Application`) را رفرنس بده.
2. `IAuditPublicService` را در Constructor تزریق کن. ثبت در DI لازم نیست؛ Audit خودش `IAuditPublicService` را با `AuditService` ثبت کرده است.
3. `LogAsync` را **بعد از موفقیت** عملیات اصلی صدا بزن، نه قبل از آن.
4. `entityId` را از خود موجودیت بگیر (`entity.Id.ToString()`)، و `changes` را یک آبجکت کوچک بساز — کل موجودیت را نفرست.

نمونه:

```csharp
public class SuspendEmploymentCommandHandler : IRequestHandler<SuspendEmploymentCommand, Result>
{
    private readonly IRepository<HRDbContext, Employment, Guid> _repo;
    private readonly IUnitOfWork<HRDbContext> _uow;
    private readonly IAuditPublicService _audit;
    private readonly UserDataContext _user;

    public SuspendEmploymentCommandHandler(
        IRepository<HRDbContext, Employment, Guid> repo,
        IUnitOfWork<HRDbContext> uow,
        IAuditPublicService audit,
        UserDataContext user)
    {
        _repo = repo;
        _uow = uow;
        _audit = audit;
        _user = user;
    }

    public async Task<Result> Handle(SuspendEmploymentCommand request, CancellationToken ct)
    {
        var employment = await _repo.GetByIdAsync(request.EmploymentId);
        if (employment is null) return Result.Fail("Employment not found");

        employment.Suspend(request.Reason);
        await _uow.SaveChangesAsync(ct);

        await _audit.LogAsync(
            action: "Suspend",
            entityName: nameof(Employment),
            entityId: employment.Id.ToString(),
            userId: _user.UserId,
            changes: new { request.Reason });

        return Result.Ok();
    }
}
```

## ۴. قواعد

- **وابسته به Infrastructure نشو.** `AuditDbContext`، `AuditLog`، `RecentAuditLogsSpec` جزو API عمومی نیستند. اگر لازم شد لاگ‌ها را بخوانی، `IAuditInternalService` را تزریق کن؛ نه دیتابیس را.
- **در حلقه لاگ ننویس.** `LogAsync` خودش `SaveChangesAsync` می‌زند. برای عملیات دسته‌ای، یک رکورد خلاصه بنویس، نه N رکورد.
- **`changes` را کوچک نگه دار.** فقط فیلدهایی که عوض شده‌اند؛ نه کل موجودیت با Navigationها.
- **`action` را استاندارد بگذار:** `Create` / `Update` / `Delete` / `Suspend` / `Approve` / `Login`. گزارش‌گیری بعدی به همین رشته‌ها تکیه می‌کند.
- **Audit را جایگزین Logging فنی نکن.** برای خطاها و کارایی از `ILogger`/Serilog استفاده کن.
- **اگر می‌خواهی صفحهٔ لاگ ماژول خودت را داشته باشی**، منبع و مجوز و منو را در `ModuleInitializer` ماژول خودت Seed کن (مثل `AuditSeedData`)؛ به Seedهای Audit دست نزن.

## ۵. چک‌لیست وقتی می‌خواهی یک قابلیت جدید به Audit اضافه کنی

1. آیا واقعاً همهٔ ماژول‌ها به آن نیاز دارند؟ اگر نه، همان ماژول خودش `ILogger` بزند.
2. متد جدید را در `IAuditPublicService` (و پیاده‌سازی‌اش در `AuditService`) اضافه کن.
3. اگر روزی Null Object برای Audit اضافه شد، آن را هم به‌روز کن تا ماژول‌های بدون Audit نشکنند.
4. مصرف‌کننده‌ها را با `grep` پیدا کن: `IAuditPublicService`.
5. تست: مسیر موفق + مسیر خطا (نباید ثبت لاگ، عملیات اصلی را بشکند).
