using Core.Application.Abstractions.Security;
using Core.Shared.Results;
using MediatR;
using Sample.Application.Commands;
using Sample.Application.DTOs;
using Sample.Application.Interfaces;
namespace Sample.Application.Handlers.Commands
{
    /*
     📌 SampleApiCommandHandler
     --------------------------
     این کلاس یک CommandHandler در الگوی CQRS است.
     وظیفه‌اش پردازش Command مربوط به ایجاد Sample جدید است.

     ✅ نکات کلیدی:
     - از MediatR و اینترفیس IRequestHandler استفاده می‌کنیم تا Commandها به Handler متصل شوند.
     - این Handler وابسته به دو سرویس است:
       1. ISampleService → برای اجرای منطق تجاری (ایجاد Sample).
       2. IPermissionChecker → برای بررسی مجوز کاربر قبل از اجرای عملیات.
     - خروجی همیشه یک Result<T> است تا موفقیت یا شکست عملیات مشخص شود.

     🛠 جریان کار:
     1. دریافت Command (SampleApiCommand) از MediatR.
     2. بررسی مجوز کاربر با PermissionChecker.
        - اگر مجوز نداشت → خطا برمی‌گردد.
     3. اجرای عملیات با SampleService.
        - اگر عملیات شکست خورد → خطا برمی‌گردد.
     4. اگر همه چیز موفق بود → یک SampleApiResponse با پیام موفقیت برمی‌گردد.

     📌 نتیجه:
     این کلاس نشان می‌دهد چطور باید Commandها را هندل کنیم،
     چطور مجوزها را بررسی کنیم و چطور نتیجه عملیات را به صورت استاندارد برگردانیم.
    */

    public class SampleApiCommandHandler : IRequestHandler<SampleApiCommand, Result<SampleApiResponse>>
    {
        private readonly ISampleService _sampleService;
        private readonly IPermissionChecker _permissionChecker;

        public SampleApiCommandHandler(ISampleService sampleService, IPermissionChecker permissionChecker)
        {
            _sampleService = sampleService;
            _permissionChecker = permissionChecker;
        }

        public async Task<Result<SampleApiResponse>> Handle(SampleApiCommand request, CancellationToken cancellationToken)
        {
            // مرحله 1: بررسی مجوز
            var hasPermission = await _permissionChecker.HasPermissionAsync("Sample.Execute");
            if (!hasPermission)
                return Result<SampleApiResponse>.Fail("مجوز لازم وجود ندارد.");

            // مرحله 2: اجرای عملیات با سرویس
            var result = await _sampleService.SampleApiMethodAsync(new SampleApiRequest(request.property1, request.property2));
            if (!result.Succeeded)
                return Result<SampleApiResponse>.Fail("عملیات با خطا مواجه شد.");

            // مرحله 3: برگرداندن نتیجه موفقیت
            return Result<SampleApiResponse>.Ok(new SampleApiResponse("عملیات موفق بود"));
        }
    }
}

