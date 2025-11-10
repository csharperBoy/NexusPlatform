using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
namespace Core.Domain.ValueObjects
{
    /*
     📌 FullName (Value Object)
     --------------------------
     این کلاس یک **Value Object** در معماری DDD است که مفهوم "نام کامل" (FirstName + LastName)
     را مدل‌سازی می‌کند. هدف آن تضمین اعتبار، برابری و نمایش استاندارد نام‌ها در سیستم است.

     ✅ نکات کلیدی:
     - از ValueObject ارث‌بری می‌کند:
       • مقایسه FullNameها فقط بر اساس مقادیر FirstName و LastName انجام می‌شود.
       • اگر دو FullName مقادیر یکسان داشته باشند، برابر هستند.

     - FirstName / LastName:
       • هر دو به صورت Immutable تعریف شده‌اند (فقط getter).
       • فقط از طریق متد Create ساخته می‌شوند.

     - متد Create(string firstName, string lastName):
       • اعتبارسنجی اولیه:
         1. FirstName نباید خالی باشد.
         2. LastName نباید خالی باشد.
       • در صورت نامعتبر بودن، ArgumentException پرتاب می‌شود.
       • در صورت معتبر بودن، نمونه جدید ساخته می‌شود (Trim شده).

     - GetEqualityComponents():
       • مقایسه نام‌ها به صورت Case-Insensitive انجام می‌شود (ToLowerInvariant).
       • این کار باعث می‌شود "Ali" و "ALI" برابر در نظر گرفته شوند.

     - ToString():
       • نمایش استاندارد نام کامل به صورت "FirstName LastName".

     🛠 جریان کار:
     1. هنگام ساخت یک کاربر یا مشتری، FullName از طریق FullName.Create ساخته می‌شود.
     2. اگر نام نامعتبر باشد، Exception پرتاب می‌شود و موجودیت ساخته نمی‌شود.
     3. اگر معتبر باشد، به صورت Immutable در موجودیت ذخیره می‌شود.
     4. مقایسه FullNameها همیشه بر اساس مقادیر استاندارد (Trim + LowerCase) انجام می‌شود.

     📌 نتیجه:
     این کلاس پایه‌ی مکانیزم **FullName Value Object** در معماری DDD است
     و تضمین می‌کند که نام‌ها همیشه معتبر، استاندارد و قابل مقایسه باشند.
    */

    public sealed class FullName : Core.Domain.Common.ValueObject
    {
        public string FirstName { get; }
        public string LastName { get; }

        private FullName(string firstName, string lastName)
        {
            FirstName = firstName;
            LastName = lastName;
        }

        // 📌 سازنده‌ی امن برای ایجاد FullName معتبر
        public static FullName Create(string firstName, string lastName)
        {
            if (string.IsNullOrWhiteSpace(firstName))
                throw new ArgumentException("First name cannot be empty", nameof(firstName));
            if (string.IsNullOrWhiteSpace(lastName))
                throw new ArgumentException("Last name cannot be empty", nameof(lastName));

            return new FullName(firstName.Trim(), lastName.Trim());
        }

        // 📌 مقایسه FullNameها فقط بر اساس مقادیر (Case-Insensitive)
        protected override IEnumerable<object?> GetEqualityComponents()
        {
            yield return FirstName.ToLowerInvariant();
            yield return LastName.ToLowerInvariant();
        }

        public override string ToString() => $"{FirstName} {LastName}";
    }
}
