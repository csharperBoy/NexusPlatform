using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace Core.Domain.ValueObjects
{
    /*
     📌 Money (Value Object)
     -----------------------
     این کلاس یک **Value Object** در معماری DDD است که مفهوم "پول" (Amount + Currency)
     را مدل‌سازی می‌کند. هدف آن تضمین اعتبار، برابری و نمایش استاندارد مقادیر پولی در سیستم است.

     ✅ نکات کلیدی:
     - از ValueObject ارث‌بری می‌کند:
       • مقایسه Moneyها فقط بر اساس مقدار (Amount) و واحد پولی (Currency) انجام می‌شود.
       • اگر دو Money مقادیر یکسان داشته باشند، برابر هستند.

     - Amount → مقدار پولی:
       • همیشه با دقت دو رقم اعشار ذخیره می‌شود (Round با MidpointRounding.AwayFromZero).
       • این کار باعث سازگاری در محاسبات مالی می‌شود.

     - Currency → واحد پولی:
       • باید یک کد سه‌حرفی ISO باشد (مثلاً "USD", "EUR", "IRR").
       • همیشه به صورت UpperCase ذخیره می‌شود تا مقایسه‌ها استاندارد باشند.

     - متد Of(decimal amount, string currency):
       • سازنده‌ی امن برای ایجاد Money معتبر.
       • اعتبارسنجی واحد پولی (سه حرفی و غیرخالی).
       • مقداردهی Amount با Round به دو رقم اعشار.
       • در صورت نامعتبر بودن، ArgumentException پرتاب می‌شود.

     - GetEqualityComponents():
       • مقایسه Moneyها بر اساس Amount و Currency.

     - ToString():
       • نمایش استاندارد مقدار پولی به صورت "Amount Currency"
         مثال: "100.50 USD"

     🛠 جریان کار:
     1. هنگام ساخت یک سفارش یا تراکنش، Money از طریق Money.Of ساخته می‌شود.
     2. اگر واحد پولی نامعتبر باشد، Exception پرتاب می‌شود و موجودیت ساخته نمی‌شود.
     3. اگر معتبر باشد، به صورت Immutable در موجودیت ذخیره می‌شود.
     4. مقایسه Moneyها همیشه بر اساس مقدار و واحد پولی استاندارد انجام می‌شود.

     📌 نتیجه:
     این کلاس پایه‌ی مکانیزم **Money Value Object** در معماری DDD است
     و تضمین می‌کند که مقادیر پولی همیشه معتبر، استاندارد و قابل مقایسه باشند.
    */

    public sealed class Money : Core.Domain.Common.ValueObject
    {
        public decimal Amount { get; }
        public string Currency { get; }

        private Money(decimal amount, string currency)
        {
            Amount = amount;
            Currency = currency;
        }

        // 📌 سازنده‌ی امن برای ایجاد Money معتبر
        public static Money Of(decimal amount, string currency)
        {
            if (string.IsNullOrWhiteSpace(currency) || currency.Length != 3)
                throw new ArgumentException("Currency must be a 3-letter ISO code", nameof(currency));

            return new Money(decimal.Round(amount, 2, MidpointRounding.AwayFromZero), currency.ToUpperInvariant());
        }

        // 📌 مقایسه Moneyها فقط بر اساس Amount و Currency
        protected override IEnumerable<object?> GetEqualityComponents()
        {
            yield return Amount;
            yield return Currency;
        }

        public override string ToString() => $"{Amount} {Currency}";
    }
}
