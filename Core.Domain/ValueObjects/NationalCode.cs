using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace Core.Domain.ValueObjects
{
    /*
     📌 NationalCode (Value Object)
     ------------------------------
     این کلاس یک **Value Object** در معماری DDD است که مفهوم "کد ملی" را مدل‌سازی می‌کند.
     هدف آن تضمین اعتبار، برابری و نمایش استاندارد کد ملی در سیستم است.

     ✅ نکات کلیدی:
     - از ValueObject ارث‌بری می‌کند:
       • مقایسه NationalCodeها فقط بر اساس مقدار (Value) انجام می‌شود.
       • اگر دو NationalCode مقدار یکسان داشته باشند، برابر هستند.

     - Value → رشته‌ی کد ملی معتبر.
       • فقط از طریق متد Create ساخته می‌شود (Immutable).
       • به صورت مستقیم قابل تغییر نیست.

     - متد Create(string value):
       • اعتبارسنجی اولیه:
         1. مقدار نباید خالی باشد.
         2. طول باید دقیقاً ۱۰ رقم باشد.
         3. همه‌ی کاراکترها باید عددی باشند.
       • در صورت نامعتبر بودن، ArgumentException پرتاب می‌شود.
       • در صورت معتبر بودن، نمونه جدید ساخته می‌شود.

     - GetEqualityComponents():
       • مقایسه NationalCodeها فقط بر اساس مقدار رشته‌ای انجام می‌شود.

     - ToString():
       • مقدار کد ملی را برمی‌گرداند (برای نمایش یا لاگ‌گذاری).

     🛠 جریان کار:
     1. هنگام ساخت یک کاربر یا مشتری، NationalCode از طریق NationalCode.Create ساخته می‌شود.
     2. اگر کد ملی نامعتبر باشد، Exception پرتاب می‌شود و موجودیت ساخته نمی‌شود.
     3. اگر معتبر باشد، به صورت Immutable در موجودیت ذخیره می‌شود.
     4. مقایسه NationalCodeها همیشه بر اساس مقدار استاندارد انجام می‌شود.

     📌 نتیجه:
     این کلاس پایه‌ی مکانیزم **NationalCode Value Object** در معماری DDD است
     و تضمین می‌کند که کد ملی‌ها همیشه معتبر، استاندارد و قابل مقایسه باشند.
    */

    public sealed class NationalCode : Core.Domain.Common.ValueObject
    {
        public string Value { get; }

        private NationalCode(string value) => Value = value;

        // 📌 سازنده‌ی امن برای ایجاد NationalCode معتبر
        public static NationalCode Create(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("National code cannot be empty", nameof(value));

            if (value.Length != 10 || !value.All(char.IsDigit))
                throw new ArgumentException("Invalid national code format", nameof(value));

            return new NationalCode(value);
        }

        // 📌 مقایسه NationalCodeها فقط بر اساس مقدار
        protected override IEnumerable<object?> GetEqualityComponents()
        {
            yield return Value;
        }

        public override string ToString() => Value;
    }
}

