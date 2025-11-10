// Core/Infrastructure/Database/IMigrationManager.cs
using Microsoft.EntityFrameworkCore;
namespace Core.Infrastructure.Database
{
    /*
     📌 IMigrationManager
     --------------------
     این اینترفیس قرارداد پایه برای مدیریت **Database Migrations** در EF Core است.
     هدف آن جداسازی منطق مهاجرت دیتابیس از سایر بخش‌های سیستم و فراهم کردن یک API
     استاندارد برای بررسی و اعمال Migrationها می‌باشد.

     ✅ نکات کلیدی:
     - MigrateAsync<TContext>:
       • اجرای Migrationها روی دیتابیس مشخص‌شده (DbContext).
       • تضمین می‌کند که دیتابیس با آخرین تغییرات مدل همگام‌سازی شود.
       • استفاده از CancellationToken برای کنترل عملیات طولانی.

     - HasPendingMigrationsAsync<TContext>:
       • بررسی می‌کند که آیا Migrationهای اعمال‌نشده وجود دارند یا خیر.
       • مقدار بازگشتی:
         → true → دیتابیس نیاز به Migration دارد.
         → false → دیتابیس به‌روز است.
       • استفاده از CancellationToken برای کنترل عملیات.

     🛠 جریان کار:
     1. در زمان راه‌اندازی برنامه (Startup/Program.cs)، سرویس IMigrationManager فراخوانی می‌شود.
     2. ابتدا HasPendingMigrationsAsync بررسی می‌کند که آیا Migration جدید وجود دارد.
     3. اگر وجود داشته باشد، MigrateAsync اجرا می‌شود تا دیتابیس به‌روز شود.
     4. این کار باعث می‌شود تیم توسعه نیازی به اجرای دستی Migrationها نداشته باشد.

     📌 نتیجه:
     این اینترفیس پایه‌ی مکانیزم **Automated Database Migration Management** در معماری ماژولار است
     و تضمین می‌کند که دیتابیس همیشه با مدل دامنه همگام باشد.
    */

    public interface IMigrationManager
    {
        // 📌 اجرای Migrationها روی دیتابیس مشخص‌شده
        Task MigrateAsync<TContext>(CancellationToken cancellationToken = default) where TContext : DbContext;

        // 📌 بررسی وجود Migrationهای اعمال‌نشده
        Task<bool> HasPendingMigrationsAsync<TContext>(CancellationToken cancellationToken = default) where TContext : DbContext;
    }
}
