using Core.Shared.Results;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Sample.Application.Commands;
using Sample.Application.Interfaces;

namespace Sample.Presentation.Controllers
{
    /*
     📌 SampleController
     -------------------
     این کلاس یک API Controller در لایه Presentation است که وظیفه‌اش
     دریافت درخواست‌های HTTP و ارسال آن‌ها به لایه Application (از طریق MediatR) می‌باشد.

     ✅ نکات کلیدی:
     - از [ApiController] استفاده شده → ویژگی‌های پیش‌فرض مثل اعتبارسنجی مدل و پاسخ‌دهی استاندارد فعال می‌شوند.
     - [Route("api/sample/[controller]")] → مسیر پایه برای این کنترلر "api/sample/sample" خواهد بود.
     - از IMediator استفاده می‌کند → الگوی MediatR برای جداسازی Controller از منطق تجاری.
       این کار باعث می‌شود Controller فقط نقش "ورودی/خروجی" داشته باشد و منطق اصلی در Command Handlerها اجرا شود.

     🛠 جریان کار:
     1. کلاینت یک درخواست POST به مسیر `api/sample/sample/SampleApi` ارسال می‌کند.
     2. بدنه درخواست شامل SampleApiCommand است.
     3. Controller این Command را به MediatR ارسال می‌کند.
     4. MediatR آن را به Handler مربوطه می‌فرستد (مثلاً SampleApiCommandHandler).
     5. Handler منطق تجاری را اجرا می‌کند (ایجاد موجودیت، ذخیره در دیتابیس، انتشار رویداد).
     6. نتیجه در قالب Result برمی‌گردد.
     7. Controller بررسی می‌کند:
        - اگر موفق نبود → BadRequest با پیام خطا.
        - اگر موفق بود → Ok با داده‌ی خروجی.

     📌 نتیجه:
     این کلاس نشان می‌دهد که Controller فقط یک لایه‌ی نازک برای مدیریت درخواست‌هاست،
     بدون منطق تجاری. این کار باعث رعایت اصل **CQRS + Clean Architecture** می‌شود.
    */

    [ApiController]
    [Route("api/sample/[controller]")]
    public class SampleController : ControllerBase
    {
        private readonly IMediator _mediator;

        public SampleController(IMediator mediator)
        {
            _mediator = mediator;
        }

        // 📌 نمونه‌ی Endpoint برای اجرای یک Command
        [HttpPost("SampleApi")]
        public async Task<IActionResult> SampleApi([FromBody] SampleApiCommand command)
        {
            var res = await _mediator.Send(command);

            if (!res.Succeeded)
                return BadRequest(res.Error); // 📌 اگر خطا رخ داد → پاسخ 400

            return Ok(res.Data); // 📌 اگر موفق بود → پاسخ 200 همراه با داده
        }
    }
}
