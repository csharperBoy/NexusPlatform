using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace Core.Domain.Common
{
    /*
     📌 BaseEntity
     -------------
     این کلاس پایه (Abstract Base Class) برای همه‌ی موجودیت‌های دامنه است.
     هدف آن فراهم کردن یک اسکلت استاندارد برای:
     1. شناسه یکتا (Id)
     2. مدیریت رویدادهای دامنه (Domain Events)

     ✅ نکات کلیدی:
     - Id → شناسه یکتا برای هر موجودیت (Guid).
       → مقدار پیش‌فرض: Guid.NewGuid() در زمان ایجاد.

     - Domain Events:
       • _domainEvents → لیست داخلی از رویدادهای دامنه.
       • DomainEvents → فقط خواندنی (ReadOnlyCollection) برای دسترسی بیرونی.
       • AddDomainEvent(IDomainEvent eventItem) → افزودن رویداد جدید به موجودیت.
       • ClearDomainEvents() → پاک کردن همه رویدادهای موجودیت (بعد از انتشار).

     🛠 جریان کار:
     1. موجودیت جدید ایجاد می‌شود و Id به صورت خودکار مقداردهی می‌شود.
     2. در طول عملیات دامنه (مثلاً ایجاد سفارش یا تغییر وضعیت)، رویدادهای دامنه
        با متد AddDomainEvent اضافه می‌شوند.
     3. پس از ذخیره تغییرات در UnitOfWork، رویدادها منتشر می‌شوند (Event Dispatcher).
     4. بعد از انتشار، متد ClearDomainEvents فراخوانی می‌شود تا لیست خالی شود.

     📌 نتیجه:
     این کلاس پایه‌ی مکانیزم **Entity Identity + Domain Events** در معماری DDD است
     و تضمین می‌کند که همه‌ی موجودیت‌ها دارای شناسه یکتا و قابلیت تولید رویداد باشند.
    */

    public abstract class BaseEntity
    {
        public Guid Id { get; protected set; } = Guid.NewGuid(); // 📌 شناسه یکتا موجودیت

        private readonly List<IDomainEvent> _domainEvents = new(); // 📌 لیست داخلی رویدادها
        public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly(); // 📌 دسترسی فقط خواندنی

        // 📌 افزودن رویداد دامنه به موجودیت
        protected void AddDomainEvent(IDomainEvent eventItem)
        {
            _domainEvents.Add(eventItem);
        }

        // 📌 پاک کردن همه رویدادهای موجودیت (بعد از انتشار)
        public void ClearDomainEvents()
        {
            _domainEvents.Clear();
        }
    }
}
