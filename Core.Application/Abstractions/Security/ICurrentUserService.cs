using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace Core.Application.Abstractions.Security
{
    /*
     📌 ICurrentUserService
     ----------------------
     این اینترفیس قرارداد سرویس "کاربر فعلی" در لایه Application است.
     هدف آن فراهم کردن یک API عمومی برای دسترسی به اطلاعات کاربر لاگین‌شده
     بدون وابستگی مستقیم به جزئیات پیاده‌سازی (مثلاً ASP.NET Identity یا JWT).

     ✅ نکات کلیدی:
     - UserId → شناسه کاربر فعلی (می‌تواند Guid یا string باشد؛ در اینجا Guid انتخاب شده).
     - UserName → نام کاربری فعلی.
     - IsAuthenticated → وضعیت احراز هویت (آیا کاربر وارد سیستم شده است یا خیر).
     - Roles → لیست نقش‌های کاربر فعلی (برای کنترل دسترسی و Authorization).

     🛠 جریان کار:
     1. کاربر وارد سیستم می‌شود (Login).
     2. سرویس امنیتی (مثلاً JWT یا Identity) اطلاعات کاربر را استخراج می‌کند.
     3. پیاده‌سازی ICurrentUserService این اطلاعات را در اختیار لایه Application قرار می‌دهد.
     4. سرویس‌ها و Handlerها می‌توانند از این اینترفیس برای گرفتن اطلاعات کاربر فعلی استفاده کنند
        (مثلاً برای ثبت لاگ، کنترل دسترسی، یا ذخیره اطلاعات مالکیت موجودیت‌ها).

     📌 نتیجه:
     این اینترفیس پایه‌ی مکانیزم **Security Context** در معماری ماژولار است و تضمین می‌کند
     که لایه Application فقط قرارداد را بشناسد، نه جزئیات پیاده‌سازی.
     پیاده‌سازی آن در لایه Infrastructure خواهد بود (مثلاً CurrentUserService مبتنی بر HttpContext).
    */

    public interface ICurrentUserService
    {
        Guid? UserId { get; }          // 📌 شناسه کاربر فعلی
        string? UserName { get; }      // 📌 نام کاربری فعلی
        bool IsAuthenticated { get; }  // 📌 وضعیت احراز هویت
        IEnumerable<string> Roles { get; } // 📌 نقش‌های کاربر فعلی
    }
}
