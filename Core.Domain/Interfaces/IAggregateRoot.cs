using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace Core.Domain.Interfaces
{
    /*
     📌 IAggregateRoot
     -----------------
     این اینترفیس یک **Marker Interface** در معماری DDD است.
     هدف آن مشخص کردن موجودیت‌هایی است که نقش **Aggregate Root** را دارند.

     ✅ نکات کلیدی:
     - Aggregate Root چیست؟
       • موجودیت اصلی در یک Aggregate که مسئول مدیریت و هماهنگی سایر موجودیت‌ها و Value Objectهای داخلی است.
       • تنها Aggregate Root می‌تواند مستقیماً توسط Repository ذخیره یا بازیابی شود.
       • سایر موجودیت‌های داخلی (Entityها و Value Objectها) فقط از طریق Aggregate Root مدیریت می‌شوند.

     - Marker Interface:
       • اینترفیس بدون متد یا پراپرتی است.
       • فقط برای علامت‌گذاری کلاس‌ها استفاده می‌شود.
       • کلاس‌هایی مثل Order, Customer, Product می‌توانند از IAggregateRoot ارث‌بری کنند تا مشخص شود Aggregate Root هستند.

     🛠 جریان کار:
     1. موجودیت دامنه (مثلاً Order) از BaseEntity و IAggregateRoot ارث‌بری می‌کند.
     2. Repositoryها فقط Aggregate Rootها را مدیریت می‌کنند (مثلاً IOrderRepository).
     3. سایر Entityها و Value Objectها فقط درون Aggregate Root وجود دارند و به صورت مستقل ذخیره نمی‌شوند.
     4. این کار باعث رعایت اصل **Aggregate Consistency** در DDD می‌شود.

     📌 نتیجه:
     این اینترفیس پایه‌ی مکانیزم **Aggregate Root Identification** در معماری DDD است
     و تضمین می‌کند که فقط موجودیت‌های اصلی به صورت مستقل مدیریت شوند.
    */

    public interface IAggregateRoot { }
}
