using Core.Shared.Results;
using MediatR;
using Sample.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sample.Application.Commands
{ 
    /*
     📌 SampleApiCommand
     -------------------
     این کلاس یک Command در الگوی CQRS است.
     Command ها برای عملیات "نوشتن" یا تغییر وضعیت سیستم استفاده می‌شوند.
     
     ✅ نکات کلیدی:
     - اینجا از record استفاده شده چون Command فقط داده‌های ورودی را نگه می‌دارد و نیازی به منطق ندارد.
     - این Command از MediatR استفاده می‌کند و به Handler مربوطه ارسال می‌شود.
     - خروجی Command همیشه یک Result<T> است تا موفقیت یا شکست عملیات مشخص شود.
     
     🛠 کاربرد:
     وقتی کاربر بخواهد یک Sample جدید ایجاد کند، این Command ساخته می‌شود
     و به Handler (SampleApiCommandHandler) ارسال می‌شود تا منطق تجاری اجرا شود.
    */
    public record SampleApiCommand(string property1, string property2)
      : IRequest<Result<SampleApiResponse>>;

}
