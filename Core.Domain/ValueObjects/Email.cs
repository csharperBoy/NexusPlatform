using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace Core.Domain.ValueObjects
{
    /*
     📌 Email (Value Object)
     -----------------------
     این کلاس یک **Value Object** در معماری DDD است که مفهوم "ایمیل" را مدل‌سازی می‌کند.
     هدف آن تضمین اعتبار (Validation) و برابری (Equality) ایمیل‌ها به صورت استاندارد است.

     ✅ نکات کلیدی:
     - از ValueObject ارث‌بری می‌کند:
       • مقایسه ایمیل‌ها فقط بر اساس مقدار (Value) انجام می‌شود.
       • اگر دو ایمیل مقدار یکسان داشته باشند، برابر هستند.

     - Value → رشته‌ی ایمیل معتبر.
       • همیشه Trim و ToLowerInvariant می‌شود تا مقایسه‌ها Case-Insensitive باشند.
       • فقط از طریق متد Create ساخته می‌شود (Immutable).

     - متد Create(string value):
       • اعتبارسنجی اولیه:
         1. مقدار نباید خالی باشد.
         2. باید شامل "@" باشد.
         3. طول باید حداقل ۵ کاراکتر باشد.
       • در صورت نامعتبر بودن، ArgumentException پرتاب می‌شود.
       • در صورت معتبر بودن، نمونه جدید ساخته می‌شود.

     - GetEqualityComponents():
       • تنها مقدار ایمیل (Value) برای مقایسه استفاده می‌شود.

     - ToString():
       • مقدار ایمیل را برمی‌گرداند (برای نمایش یا لاگ‌گذاری).

     🛠 جریان کار:
     1. هنگام ساخت یک کاربر جدید، ایمیل از طریق Email.Create ساخته می‌شود.
     2. اگر ایمیل نامعتبر باشد، Exception پرتاب می‌شود و موجودیت ساخته نمی‌شود.
     3. اگر ایمیل معتبر باشد، به صورت Immutable در موجودیت ذخیره می‌شود.
     4. مقایسه ایمیل‌ها همیشه بر اساس مقدار استاندارد (Trim + LowerCase) انجام می‌شود.

     📌 نتیجه:
     این کلاس پایه‌ی مکانیزم **Email Value Object** در معماری DDD است
     و تضمین می‌کند که ایمیل‌ها همیشه معتبر، استاندارد و قابل مقایسه باشند.
    */

    public sealed class Email : Core.Domain.Common.ValueObject
    {
        public string Value { get; }

        private Email(string value) => Value = value;

        // 📌 سازنده‌ی امن برای ایجاد ایمیل معتبر
        public static Email Create(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Email cannot be empty", nameof(value));

            if (!value.Contains("@") || value.Length < 5)
                throw new ArgumentException("Invalid email format", nameof(value));

            return new Email(value.Trim().ToLowerInvariant());
        }

        // 📌 مقایسه ایمیل‌ها فقط بر اساس مقدار
        protected override IEnumerable<object?> GetEqualityComponents()
        {
            yield return Value;
        }

        public override string ToString() => Value;
    }
}
