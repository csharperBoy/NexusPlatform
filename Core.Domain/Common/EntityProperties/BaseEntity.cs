using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
namespace Core.Domain.Common.EntityProperties
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
        public void AddDomainEvent(IDomainEvent eventItem)
        {
            _domainEvents.Add(eventItem);
        }

        // 📌 پاک کردن همه رویدادهای موجودیت (بعد از انتشار)
        public void ClearDomainEvents()
        {
            _domainEvents.Clear();
        }
    }

    

    public static class BaseEntityExtensions
    {
        // 📌 کش کردن PropertyInfo ها برای هر نوع موجودیت (برای افزایش سرعت)
        private static readonly ConcurrentDictionary<Type, PropertyInfo[]> _propertiesCache = new();

        /// <summary>
        /// تغییرات یک موجودیت را بر اساس ماسک (لیست رشته‌ای) اعمال می‌کند.
        /// </summary>
        /// <param name="target">موجودیت مقصد (همان موجودیت داخل دیتابیس)</param>
        /// <param name="source">موجودیت مبدأ (حاوی مقادیر جدید)</param>
        /// <param name="updateMask">لیست نام فیلدهایی که باید تغییر کنند. اگر null یا خالی باشد، یعنی همه فیلدها</param>
        /// <returns>برمی‌گرداند که آیا تغییر معنی‌داری اعمال شده است یا نه</returns>
        public static bool ApplyChange(this BaseEntity target, BaseEntity source, List<string>? updateMask)
        {
            if (target == null || source == null)
                return false;

            // 📌 گرفتن نام موجودیت (مثلاً "Employment", "Person", "Contact")
            string entityName = target.GetType().Name;


            // 📌 اگر نوع‌ها یکی نباشند، خطا بده
            if (entityName != source.GetType().Name)
                throw new ArgumentException("نوع موجودیت مبدأ و مقصد باید یکی باشد.");


            // 📌 گرفتن PropertyInfo ها از کش
            var properties = _propertiesCache.GetOrAdd(target.GetType(), type =>
                type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                    .Where(p => p.CanRead && p.CanWrite)
                    .Where(p => p.Name != nameof(BaseEntity.Id)) // امنیت: تغییر Id ممنوع
                    .ToArray()
            );

            bool hasChanged = false;

            foreach (var property in properties)
            {
                // 🔥 کلید اصلی: ساختن نام کامل ماسک به صورت "EntityName.PropertyName"
                string fullMaskKey = $"{entityName}.{property.Name}";

                // اگر ماسک وجود داشته باشد و این کلید در آن نباشد، رد کن
                if (updateMask is { Count: > 0 } &&
                    !updateMask.Contains(fullMaskKey, StringComparer.OrdinalIgnoreCase))
                {
                    continue;
                }

                // خواندن مقادیر فعلی و جدید
                var currentValue = property.GetValue(target);
                var newValue = property.GetValue(source);

                // مقایسه عمیق (برای پشتیبانی از لیست‌ها و مجموعه‌ها)
                if (!AreValuesEqual(currentValue, newValue))
                {
                    property.SetValue(target, newValue);
                    hasChanged = true;
                }
            }

            return hasChanged;
        }

        /// <summary>
        /// مقایسه عمیق دو مقدار (پشتیبانی از لیست‌ها، آرایه‌ها و انواع ارجاعی)
        /// </summary>
        private static bool AreValuesEqual(object? left, object? right)
        {
            // هر دو null هستند => برابر
            if (left == null && right == null) return true;
            // یکی null و دیگری نه => نابرابر
            if (left == null || right == null) return false;

            // اگر هر دو از نوع IEnumerable باشند (لیست، آرایه، و ...)
            if (left is IEnumerable leftEnumerable && right is IEnumerable rightEnumerable)
            {
                // تبدیل به لیست برای مقایسه ترتیبی
                var leftList = leftEnumerable.Cast<object>().ToList();
                var rightList = rightEnumerable.Cast<object>().ToList();

                // اگر تعداد عناصر متفاوت باشد => نابرابر
                if (leftList.Count != rightList.Count) return false;

                // مقایسه تک‌تک عناصر با استفاده از خود این متد (فراخوانی بازگشتی)
                return !leftList.Where((t, i) => !AreValuesEqual(t, rightList[i])).Any();
            }

            // در غیر این صورت، مقایسه معمولی با متد Equals
            return left.Equals(right);
        }
    }
}
