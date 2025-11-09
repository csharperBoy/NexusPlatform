using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace Core.Application.Abstractions.Auditing
{
    /*
     📌 IAuditService
     ----------------
     این اینترفیس قرارداد سرویس لاگ‌گذاری (Auditing) در لایه Application است.
     هدف آن ثبت عملیات‌های مهم روی موجودیت‌ها (Entities) برای اهداف امنیتی،
     مانیتورینگ و ردیابی تغییرات می‌باشد.

     ✅ نکات کلیدی:
     - متد اصلی: LogAsync
       → عملیات لاگ‌گذاری را به صورت غیرهمزمان انجام می‌دهد.
       → پارامترها:
         1. action → نوع عملیات (مثلاً "Create", "Update", "Delete").
         2. entityName → نام موجودیت (مثلاً "SampleEntity").
         3. entityId → شناسه موجودیت (مثلاً GUID یا کلید اصلی).
         4. userId → شناسه کاربری که عملیات را انجام داده (اختیاری).
         5. changes → جزئیات تغییرات انجام‌شده (اختیاری، می‌تواند یک آبجکت یا دیکشنری باشد).

     🛠 جریان کار:
     1. هر زمان عملیاتی روی موجودیت‌ها انجام شود (ایجاد، ویرایش، حذف).
     2. سرویس‌های مربوطه متد LogAsync را فراخوانی می‌کنند.
     3. اطلاعات عملیات در دیتابیس یا سیستم لاگ ذخیره می‌شود.
     4. این داده‌ها بعداً برای گزارش‌گیری، مانیتورینگ یا بررسی امنیتی استفاده می‌شوند.

     📌 نتیجه:
     این اینترفیس یک قرارداد عمومی برای لاگ‌گذاری است و پیاده‌سازی آن می‌تواند
     در لایه Infrastructure انجام شود (مثلاً ذخیره در دیتابیس، ارسال به ELK، یا نوشتن در فایل).
     با این طراحی، لایه Application فقط قرارداد را می‌شناسد و وابسته به جزئیات پیاده‌سازی نیست.
    */

    public interface IAuditService
    {
        Task LogAsync(
            string action,       // نوع عملیات (Create, Update, Delete, ...)
            string entityName,   // نام موجودیت
            string entityId,     // شناسه موجودیت
            Guid? userId,        // شناسه کاربر (اختیاری)
            object? changes = null // جزئیات تغییرات (اختیاری)
        );
    }
}
