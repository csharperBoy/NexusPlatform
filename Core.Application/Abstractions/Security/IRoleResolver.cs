using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace Core.Application.Abstractions.Security
{
    /*
     📌 IRoleResolver
     ----------------
     این اینترفیس قرارداد سرویس "تشخیص نقش‌های کاربر" (Role Resolution) در لایه Application است.
     هدف آن فراهم کردن یک API عمومی برای دریافت نقش‌های کاربر بر اساس شناسه کاربری (UserId) می‌باشد.

     ✅ نکات کلیدی:
     - متد اصلی: GetUserRolesAsync(Guid userId)
       → لیست نقش‌های کاربر مشخص‌شده را برمی‌گرداند.
       → خروجی: IList<string> شامل نام نقش‌ها (مثلاً "Admin", "Manager", "User").

     🛠 جریان کار:
     1. سرویس‌های امنیتی یا Handlerها نیاز دارند بدانند کاربر چه نقش‌هایی دارد.
     2. IRoleResolver با دریافت UserId نقش‌های کاربر را استخراج می‌کند.
     3. نقش‌ها می‌توانند از دیتابیس، Identity Server، یا Claims در JWT Token خوانده شوند.
     4. سرویس‌های دیگر (مثل PermissionChecker یا Authorization Handlerها) از این نقش‌ها برای کنترل دسترسی استفاده می‌کنند.

     📌 نتیجه:
     این اینترفیس پایه‌ی مکانیزم **Role-Based Access Control (RBAC)** در معماری ماژولار است.
     با این طراحی، لایه Application فقط قرارداد را می‌شناسد و پیاده‌سازی در لایه Infrastructure انجام می‌شود
     (مثلاً RoleResolver مبتنی بر ASP.NET Identity یا یک سرویس خارجی).
    */

    public interface IRoleResolver
    {
        Task<IList<string>> GetUserRolesAsync(Guid userId); // 📌 دریافت نقش‌های کاربر بر اساس UserId
    }
}
