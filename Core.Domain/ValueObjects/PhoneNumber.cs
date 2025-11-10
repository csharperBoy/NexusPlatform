using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace Core.Domain.ValueObjects
{
    /*
     📌 PhoneNumber (Value Object)
     -----------------------------
     این کلاس یک **Value Object** در معماری DDD است که مفهوم "شماره تلفن" را مدل‌سازی می‌کند.
     هدف آن تضمین اعتبار، برابری و نمایش استاندارد شماره تلفن‌ها در سیستم است.

     ✅ نکات کلیدی:
     - از ValueObject ارث‌بری می‌کند:
       • مقایسه PhoneNumberها فقط بر اساس مقدار (Value) انجام می‌شود.
       • اگر دو PhoneNumber مقدار یکسان داشته باشند، برابر هستند.

     - Value → رشته‌ی شماره تلفن معتبر.
       • فقط از طریق متد Create ساخته می‌شود (Immutable).
       • به صورت مستقیم قابل تغییر نیست.

     - متد Create(string value):
       • اعتبارسنجی اولیه:
         1. مقدار نباید خالی باشد.
         2. همه‌ی کاراکترهای غیرعددی حذف می‌شوند (فقط رقم‌ها باقی می‌مانند).
         3. طول شماره باید بین ۷ تا ۱۵ رقم باشد (مطابق استانداردهای بین‌المللی).
       • در صورت نامعتبر بودن، ArgumentException پرتاب می‌شود.
       • در صورت معتبر بودن، نمونه جدید ساخته می‌شود.

     - GetEqualityComponents():
       • مقایسه PhoneNumberها فقط بر اساس مقدار رشته‌ای انجام می‌شود.

     - ToString():
       • مقدار شماره تلفن را برمی‌گرداند (برای نمایش یا لاگ‌گذاری).

     🛠 جریان کار:
     1. هنگام ساخت یک کاربر یا مشتری، PhoneNumber از طریق PhoneNumber.Create ساخته می‌شود.
     2. اگر شماره تلفن نامعتبر باشد، Exception پرتاب می‌شود و موجودیت ساخته نمی‌شود.
     3. اگر معتبر باشد، به صورت Immutable در موجودیت ذخیره می‌شود.
     4. مقایسه PhoneNumberها همیشه بر اساس مقدار استاندارد (فقط رقم‌ها) انجام می‌شود.

     📌 نتیجه:
     این کلاس پایه‌ی مکانیزم **PhoneNumber Value Object** در معماری DDD است
     و تضمین می‌کند که شماره تلفن‌ها همیشه معتبر، استاندارد و قابل مقایسه باشند.
    */

    public sealed class PhoneNumber : Core.Domain.Common.ValueObject
    {
        public string Value { get; }

        private PhoneNumber(string value) => Value = value;

        // 📌 سازنده‌ی امن برای ایجاد PhoneNumber معتبر
        public static PhoneNumber Create(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Phone number cannot be empty", nameof(value));

            var digits = new string(value.Where(char.IsDigit).ToArray());
            if (digits.Length < 7 || digits.Length > 15)
                throw new ArgumentException("Invalid phone number length", nameof(value));

            return new PhoneNumber(digits);
        }

        // 📌 مقایسه PhoneNumberها فقط بر اساس مقدار
        protected override IEnumerable<object?> GetEqualityComponents()
        {
            yield return Value;
        }

        public override string ToString() => Value;
    }
}
