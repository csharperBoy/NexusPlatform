using Core.Shared.Results;
using MediatR;
using Sample.Application.Interfaces;
using Sample.Application.Queries;
using Sample.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace Sample.Application.Handlers.Queries
{
    /*
     📌 GetSamplesByPropertyQueryHandler
     -----------------------------------
     این کلاس یک QueryHandler در الگوی CQRS است.
     وظیفه‌اش پردازش Query مربوط به خواندن داده‌های Sample بر اساس property1 است.

     ✅ نکات کلیدی:
     - از MediatR و اینترفیس IRequestHandler استفاده می‌کنیم تا Queryها به Handler متصل شوند.
     - این Handler وابسته به ISampleQueryService است که منطق خواندن داده‌ها را پیاده‌سازی می‌کند.
     - خروجی همیشه یک Result<T> است تا موفقیت یا شکست عملیات مشخص شود.

     🛠 جریان کار:
     1. دریافت Query (GetSamplesByPropertyQuery) از MediatR.
     2. فراخوانی سرویس Query (`ISampleQueryService`) برای اجرای Specification و صفحه‌بندی.
     3. برگرداندن لیست داده‌ها در قالب Result<IReadOnlyList<SampleEntity>>.

     📌 نتیجه:
     این کلاس نشان می‌دهد چطور باید Queryها را هندل کنیم،
     چطور داده‌ها را با Specification بخوانیم و چطور نتیجه را به صورت استاندارد برگردانیم.
    */

    public class GetSamplesByPropertyQueryHandler
       : IRequestHandler<GetSamplesByPropertyQuery, Result<IReadOnlyList<SampleEntity>>>
    {
        private readonly ISampleQueryService _sampleService;

        public GetSamplesByPropertyQueryHandler(ISampleQueryService sampleService)
        {
            _sampleService = sampleService;
        }

        public async Task<Result<IReadOnlyList<SampleEntity>>> Handle(GetSamplesByPropertyQuery request, CancellationToken ct)
        {
            // اجرای عملیات خواندن داده‌ها با Specification و صفحه‌بندی
            return await _sampleService.GetBySpecAsync(request.property1, request.Page, request.PageSize);
        }
    }
}
