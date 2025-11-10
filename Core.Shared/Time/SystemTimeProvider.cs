using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace Core.Shared.Time
{
    /*
     📌 SystemTimeProvider
     ---------------------
     این کلاس پیاده‌سازی پیش‌فرض ITimeProvider است که زمان جاری سیستم را
     به صورت UTC برمی‌گرداند.

     ✅ نکات کلیدی:
     - UtcNow:
       • مقدار DateTime.UtcNow را برمی‌گرداند.
       • همیشه زمان جهانی (Coordinated Universal Time) را ارائه می‌دهد.
       • استفاده از UTC باعث می‌شود وابستگی به TimeZone محلی حذف شود و
         داده‌ها در سیستم‌های توزیع‌شده سازگار باقی بمانند.

     - طراحی:
       • این کلاس ساده‌ترین پیاده‌سازی ITimeProvider است.
       • در محیط Production استفاده می‌شود تا زمان واقعی سیستم را ارائه دهد.
       • در تست‌ها می‌توان از پیاده‌سازی‌های جایگزین (مثل FakeTimeProvider) استفاده کرد
         تا زمان قابل کنترل و پیش‌بینی باشد.

     🛠 جریان کار:
     1. در DI Container، SystemTimeProvider به عنوان پیاده‌سازی ITimeProvider ثبت می‌شود.
     2. سرویس‌ها و کلاس‌های Domain یا Application به جای استفاده مستقیم از DateTime.UtcNow،
        از ITimeProvider استفاده می‌کنند.
     3. این طراحی باعث می‌شود کدها تست‌پذیرتر و مستقل از زمان واقعی باشند.

     📌 نتیجه:
     این کلاس پایه‌ی مکانیزم **System Time Provider** در معماری ماژولار است
     و تضمین می‌کند که زمان جاری به صورت استاندارد و یکپارچه در کل سیستم در دسترس باشد.
    */

    public class SystemTimeProvider : ITimeProvider
    {
        public DateTime UtcNow => DateTime.UtcNow;
    }
}
