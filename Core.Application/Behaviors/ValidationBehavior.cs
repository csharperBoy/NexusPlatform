using FluentValidation;
using MediatR;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ValidationException = FluentValidation.ValidationException;

namespace Core.Application.Behaviors
{
    /*
     📌 ValidationBehavior<TRequest, TResponse>
     ------------------------------------------
     این کلاس یک **Pipeline Behavior** در MediatR است که وظیفه‌اش اعتبارسنجی (Validation)
     همه‌ی Requestها (Command/Query) قبل از رسیدن به Handler می‌باشد.

     ✅ نکات کلیدی:
     - از IPipelineBehavior<TRequest, TResponse> ارث‌بری می‌کند:
       → این الگو به ما اجازه می‌دهد منطق مشترک (Cross-Cutting Concerns) را
         قبل یا بعد از اجرای Handler اضافه کنیم.
       → در اینجا اعتبارسنجی قبل از اجرای Handler انجام می‌شود.

     - وابستگی:
       → IEnumerable<IValidator<TRequest>> → مجموعه‌ای از Validatorها برای Request مشخص.
         این Validatorها معمولاً با استفاده از FluentValidation تعریف می‌شوند.

     - متد Handle:
       1. بررسی می‌کند که آیا Validator برای Request وجود دارد یا خیر.
       2. اگر وجود داشته باشد:
          - یک ValidationContext ساخته می‌شود.
          - همه Validatorها به صورت موازی اجرا می‌شوند (Task.WhenAll).
          - خطاهای اعتبارسنجی جمع‌آوری می‌شوند.
          - اگر خطا وجود داشته باشد، یک ValidationException پرتاب می‌شود.
       3. اگر خطا وجود نداشته باشد، Handler اصلی اجرا می‌شود.

     🛠 جریان کار:
     1. کاربر یک Command یا Query ارسال می‌کند.
     2. MediatR آن را به Pipeline Behaviors می‌فرستد.
     3. ValidationBehavior قبل از اجرای Handler اعتبارسنجی را انجام می‌دهد.
     4. اگر داده‌ها معتبر باشند → Handler اجرا می‌شود.
     5. اگر داده‌ها نامعتبر باشند → ValidationException پرتاب می‌شود و Handler اجرا نمی‌شود.

     📌 نتیجه:
     این کلاس تضمین می‌کند که همه‌ی درخواست‌ها قبل از رسیدن به Handler اعتبارسنجی شوند،
     بدون اینکه نیاز باشد در هر Handler به صورت دستی کد اعتبارسنجی نوشته شود.
     این کار باعث رعایت اصل **Cross-Cutting Concerns** و ساده‌سازی Handlerها می‌شود.
    */

    public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        private readonly IEnumerable<IValidator<TRequest>> _validators;

        public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
        {
            _validators = validators;
        }

        public async Task<TResponse> Handle(
            TRequest request,
            RequestHandlerDelegate<TResponse> next,
            CancellationToken cancellationToken)
        {
            if (_validators.Any())
            {
                var context = new ValidationContext<TRequest>(request);

                var validationResults = await Task.WhenAll(
                    _validators.Select(v => v.ValidateAsync(context, cancellationToken)));

                var failures = validationResults
                    .SelectMany(r => r.Errors)
                    .Where(f => f != null)
                    .ToList();

                if (failures.Count != 0)
                    throw new ValidationException(failures);
            }

            return await next();
        }
    }
}
