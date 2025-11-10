using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace Core.Domain.Interfaces
{
    /*
     📌 IEntity<TKey>
     ----------------
     این اینترفیس قرارداد پایه برای همه‌ی موجودیت‌های دامنه (Domain Entities) است.
     هدف آن استانداردسازی ساختار موجودیت‌ها و تضمین اینکه هر موجودیت دارای شناسه یکتا (Id) باشد.

     ✅ نکات کلیدی:
     - Generic Interface:
       • TKey → نوع شناسه موجودیت (مثلاً Guid, int, string).
       • این طراحی انعطاف‌پذیری ایجاد می‌کند تا بتوان از انواع مختلف شناسه استفاده کرد.

     - Id → شناسه یکتا موجودیت:
       • هر موجودیت باید یک شناسه یکتا داشته باشد.
       • این شناسه برای شناسایی و مقایسه موجودیت‌ها استفاده می‌شود.
       • معمولاً در کلاس BaseEntity پیاده‌سازی می‌شود.

     🛠 جریان کار:
     1. موجودیت‌های دامنه (مثل User, Order, Product) از IEntity<TKey> ارث‌بری می‌کنند.
     2. هر موجودیت یک Id دارد که نوع آن توسط TKey مشخص می‌شود.
     3. Repositoryها و سرویس‌ها می‌توانند با استفاده از این اینترفیس به صورت عمومی با موجودیت‌ها کار کنند.
     4. این کار باعث رعایت اصل **Consistency & Abstraction** در معماری DDD می‌شود.

     📌 نتیجه:
     این اینترفیس پایه‌ی مکانیزم **Entity Identification** در معماری DDD است
     و تضمین می‌کند که همه‌ی موجودیت‌ها دارای شناسه یکتا و قابل مدیریت باشند.
    */

    public interface IEntity<TKey>
    {
        TKey Id { get; } // 📌 شناسه یکتا موجودیت
    }
}
