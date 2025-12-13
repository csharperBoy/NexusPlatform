using Core.Application.Abstractions.Security;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
namespace Core.Infrastructure.Security
{
    /*
     📌 CurrentUserService
     ---------------------
     این کلاس پیاده‌سازی اینترفیس ICurrentUserService است و وظیفه‌ی استخراج اطلاعات کاربر جاری
     از HttpContext را بر عهده دارد. هدف آن فراهم کردن یک abstraction ساده برای دسترسی به
     شناسه، نام، وضعیت احراز هویت و نقش‌های کاربر است.

     ✅ نکات کلیدی:
     - وابستگی‌ها:
       • IHttpContextAccessor → برای دسترسی به HttpContext در ASP.NET Core.
       • این وابستگی از طریق DI تزریق می‌شود.

     - ویژگی‌ها:
       • UserId → شناسه‌ی کاربر جاری (Claim "sub" یا JwtRegisteredClaimNames.Sub).
         - اگر مقدار موجود نباشد → null.
         - اگر مقدار موجود باشد → Guid.Parse.
       • UserName → نام کاربر جاری (Identity.Name).
       • IsAuthenticated → وضعیت احراز هویت کاربر (Identity.IsAuthenticated).
       • Roles → لیست نقش‌های کاربر جاری (Claim "role").

     - طراحی:
       • این کلاس یک Wrapper ساده روی HttpContext.User است.
       • هدف: جداسازی وابستگی مستقیم به HttpContext از لایه‌های بالاتر.
       • این کار باعث می‌شود تست‌پذیری و انعطاف‌پذیری افزایش یابد.

     🛠 جریان کار:
     1. در زمان اجرای درخواست، HttpContext شامل Claims کاربر است.
     2. CurrentUserService این Claims را استخراج می‌کند.
     3. سرویس‌های Application یا Domain می‌توانند از ICurrentUserService استفاده کنند
        بدون نیاز به وابستگی مستقیم به HttpContext.
     4. این طراحی باعث می‌شود کدها تمیزتر و قابل تست‌تر باشند.

     📌 نتیجه:
     این کلاس پایه‌ی مکانیزم **Current User Context Abstraction** در معماری ماژولار است
     و تضمین می‌کند که اطلاعات کاربر جاری به صورت استاندارد و قابل استفاده مجدد در کل سیستم در دسترس باشد.
    */

    public class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CurrentUserService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public Guid? UserId
        {
            get
            {
                var userId = _httpContextAccessor.HttpContext?.User?.FindFirstValue(JwtRegisteredClaimNames.Sub);
                return string.IsNullOrEmpty(userId) ? null : Guid.Parse(userId);
            }
        }


        public string? UserName => _httpContextAccessor.HttpContext?.User?.Identity?.Name;

        public bool IsAuthenticated => _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;

      //  public IEnumerable<string> Roles =>
        //    _httpContextAccessor.HttpContext?.User?.FindAll("role").Select(r => r.Value) ?? Enumerable.Empty<string>();

        public IEnumerable<string> Roles =>
    _httpContextAccessor.HttpContext?.User?.FindAll(ClaimTypes.Role).Select(r => r.Value)
    ?? Enumerable.Empty<string>();
    }
}
