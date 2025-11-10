using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace Core.Shared.Time
{
    /*
     📌 ITimeProvider
     ----------------
     این اینترفیس یک abstraction برای دسترسی به زمان جاری (UTC) فراهم می‌کند.
     هدف آن جداسازی وابستگی مستقیم به DateTime.UtcNow از لایه‌های بالاتر و افزایش تست‌پذیری است.

     ✅ نکات کلیدی:
     - ویژگی UtcNow:
       • زمان جاری به صورت UTC.
       • به جای استفاده مستقیم از DateTime.UtcNow در کدها، از این اینترفیس استفاده می‌شود.

     - طراحی:
       • اینترفیس ساده است اما نقش مهمی در معماری دارد.
       • با استفاده از ITimeProvider می‌توان زمان را در تست‌ها Mock کرد.
       • این کار باعث می‌شود تست‌ها قابل پیش‌بینی باشند (بدون وابستگی به زمان واقعی سیستم).

     🛠 جریان کار:
     1. در لایه‌ی Infrastructure یک پیاده‌سازی (مثلاً SystemTimeProvider) ساخته می‌شود که DateTime.UtcNow را برمی‌گرداند.
     2. در تست‌ها می‌توان پیاده‌سازی دیگری (مثلاً FakeTimeProvider) ساخت که زمان ثابت یا کنترل‌شده برمی‌گرداند.
     3. سرویس‌ها و کلاس‌های Domain یا Application به جای وابستگی به DateTime، از ITimeProvider استفاده می‌کنند.
     4. این طراحی باعث می‌شود کدها قابل تست، قابل نگهداری و مستقل از زمان واقعی باشند.

     📌 نتیجه:
     این اینترفیس پایه‌ی مکانیزم **Time Abstraction** در معماری ماژولار است
     و تضمین می‌کند که مدیریت زمان به صورت استاندارد، قابل تست و قابل توسعه انجام شود.
    */

    public interface ITimeProvider
    {
        DateTime UtcNow { get; }
    }
}
