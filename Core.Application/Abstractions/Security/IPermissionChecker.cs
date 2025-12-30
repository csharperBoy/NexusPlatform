using Core.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace Core.Application.Abstractions.Security
{
    /*
     📌 IPermissionChecker
     ---------------------
     این اینترفیس قرارداد سرویس بررسی مجوزها (Permission Checking) در لایه Application است.
     هدف آن فراهم کردن یک API عمومی برای کنترل دسترسی (Authorization) بر اساس مجوزهای تعریف‌شده می‌باشد.

     ✅ نکات کلیدی:
     - متدها:
       1. HasPermissionAsync(string permission)
          → بررسی می‌کند که آیا کاربر فعلی دارای یک مجوز خاص هست یا نه.
          → برای سناریوهایی مثل "آیا کاربر می‌تواند موجودیت Sample را ویرایش کند؟".

       2. HasAnyPermissionAsync(params string[] permissions)
          → بررسی می‌کند که آیا کاربر فعلی حداقل یکی از مجوزهای داده‌شده را دارد یا نه.
          → برای سناریوهایی مثل "آیا کاربر دارای یکی از نقش‌های Admin یا Manager هست؟".

     🛠 جریان کار:
     1. کاربر وارد سیستم می‌شود و نقش‌ها/مجوزهای او استخراج می‌شوند (مثلاً از JWT یا Identity).
     2. سرویس‌های Application یا Handlerها برای اجرای عملیات حساس، متدهای IPermissionChecker را فراخوانی می‌کنند.
     3. اگر کاربر مجوز لازم را داشته باشد → عملیات ادامه پیدا می‌کند.
     4. اگر نداشته باشد → خطای Authorization یا Forbidden برگردانده می‌شود.

     📌 نتیجه:
     این اینترفیس پایه‌ی مکانیزم **Authorization** در معماری ماژولار است و تضمین می‌کند
     که کنترل دسترسی به صورت استاندارد و مستقل از جزئیات پیاده‌سازی انجام شود.
     پیاده‌سازی آن در لایه Infrastructure خواهد بود (مثلاً PermissionChecker مبتنی بر ClaimsPrincipal).
    */

    public interface IPermissionChecker
    {
        /// <summary>
        /// 📌 بررسی می‌کند که آیا کاربر فعلی دارای مجوز مشخص‌شده هست یا نه.
        /// </summary>
        Task<bool> HasPermissionAsync(string permission);

        /// <summary>
        /// 📌 بررسی می‌کند که آیا کاربر فعلی حداقل یکی از مجوزهای داده‌شده را دارد یا نه.
        /// </summary>
        Task<bool> HasAnyPermissionAsync(params string[] permissions);

    }
}
