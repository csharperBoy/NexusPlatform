using FluentValidation;
using Sample.Application.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace Sample.Application.Validators
{
    /*
     📌 SampleApiCommandValidator
     ----------------------------
     این کلاس یک Validator برای Command در الگوی CQRS است.
     وظیفه‌اش اعتبارسنجی داده‌های ورودی قبل از اجرای منطق تجاری است.

     ✅ نکات کلیدی:
     - از کتابخانه FluentValidation استفاده می‌کنیم تا قوانین اعتبارسنجی را تعریف کنیم.
     - این Validator روی SampleApiCommand اعمال می‌شود.
     - اگر داده‌ها معتبر نباشند، Pipeline Behavior مربوط به Validation جلوی اجرای Handler را می‌گیرد
       و خطاهای اعتبارسنجی به کلاینت برگردانده می‌شوند.

     🛠 قوانین اعتبارسنجی:
     1. property1
        - نباید خالی باشد (NotEmpty).
        - حداکثر طول 200 کاراکتر.
     2. property2
        - نباید خالی باشد (NotEmpty).
        - حداکثر طول 200 کاراکتر.

     📌 نتیجه:
     این کلاس نشان می‌دهد چطور باید قوانین اعتبارسنجی را برای Commandها تعریف کنیم
     تا مطمئن شویم داده‌های ورودی قبل از رسیدن به Handler معتبر هستند.
    */

    public class SampleApiCommandValidator : AbstractValidator<SampleApiCommand>
    {
        public SampleApiCommandValidator()
        {
            // اعتبارسنجی property1
            RuleFor(x => x.property1)
                .NotEmpty().WithMessage("property1 نباید خالی باشد.")
                .MaximumLength(200);

            // اعتبارسنجی property2
            RuleFor(x => x.property2)
                .NotEmpty().WithMessage("property2 نباید خالی باشد.")
                .MaximumLength(200);
        }
    }
}
