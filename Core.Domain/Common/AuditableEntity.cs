using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace Core.Domain.Common
{
    /*
     📌 AuditableEntity
     ------------------
     این کلاس یک **Base Class** برای موجودیت‌های دامنه است که نیاز به ثبت اطلاعات
     مربوط به ایجاد و ویرایش دارند. هدف آن فراهم کردن یک اسکلت استاندارد برای
     مدیریت **Audit Fields** در همه‌ی موجودیت‌ها می‌باشد.

     ✅ نکات کلیدی:
     - از BaseEntity ارث‌بری می‌کند → یعنی علاوه بر شناسه و ویژگی‌های پایه، قابلیت‌های Audit هم دارد.
     - فیلدها:
       • CreatedAt → زمان ایجاد موجودیت (به صورت پیش‌فرض زمان فعلی UTC).
       • CreatedBy → شناسه یا نام کاربری فردی که موجودیت را ایجاد کرده.
       • ModifiedAt → زمان آخرین تغییر موجودیت.
       • ModifiedBy → شناسه یا نام کاربری فردی که آخرین تغییر را انجام داده.

     🛠 جریان کار:
     1. هنگام ایجاد موجودیت جدید:
        - CreatedAt به زمان فعلی تنظیم می‌شود.
        - CreatedBy از سرویس ICurrentUserService گرفته می‌شود.
     2. هنگام ویرایش موجودیت:
        - ModifiedAt به زمان فعلی تنظیم می‌شود.
        - ModifiedBy از سرویس ICurrentUserService گرفته می‌شود.
     3. این مقادیر معمولاً توسط **AuditBehavior** یا **DbContext SaveChanges** به صورت خودکار مقداردهی می‌شوند.

     📌 نتیجه:
     این کلاس پایه‌ی مکانیزم **Auditing** در معماری ماژولار است و تضمین می‌کند
     که همه‌ی موجودیت‌ها اطلاعات ایجاد و ویرایش را به صورت استاندارد نگه‌داری کنند.
     این کار باعث افزایش قابلیت ردیابی (Traceability) و شفافیت در سیستم می‌شود.
    */

    public abstract class AuditableEntity : BaseEntity
    {
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow; // 📌 زمان ایجاد
        public string? CreatedBy { get; set; }                      // 📌 کاربر ایجادکننده
        public DateTime? ModifiedAt { get; set; }                   // 📌 زمان آخرین تغییر
        public string? ModifiedBy { get; set; }                     // 📌 کاربر آخرین تغییر
    }
}
