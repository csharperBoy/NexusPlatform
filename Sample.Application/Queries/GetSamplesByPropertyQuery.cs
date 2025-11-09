using Core.Shared.Results;
using MediatR;
using Sample.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace Sample.Application.Queries
{
    /*
     📌 GetSamplesByPropertyQuery
     ----------------------------
     این کلاس یک Query در الگوی CQRS است.
     Queryها فقط برای عملیات **خواندن داده‌ها (Read)** استفاده می‌شوند و هیچ تغییر وضعیتی در سیستم ایجاد نمی‌کنند.

     ✅ نکات کلیدی:
     - از MediatR و اینترفیس IRequest<Result<T>> استفاده می‌کنیم تا Queryها به Handler مربوطه متصل شوند.
     - این Query سه پارامتر دارد:
       1. property1 → شرط اصلی برای فیلتر کردن داده‌ها.
       2. Page → شماره صفحه برای صفحه‌بندی (پیش‌فرض 1).
       3. PageSize → تعداد رکوردها در هر صفحه (پیش‌فرض 10).
     - خروجی همیشه یک Result<IReadOnlyList<SampleEntity>> است تا موفقیت یا شکست عملیات مشخص شود.

     🛠 جریان کار:
     1. این Query توسط کلاینت یا لایه بالاتر ساخته می‌شود.
     2. به MediatR ارسال می‌شود.
     3. Handler مربوطه (GetSamplesByPropertyQueryHandler) آن را دریافت و پردازش می‌کند.
     4. داده‌ها بر اساس Specification خوانده می‌شوند و در قالب Result برگردانده می‌شوند.

     📌 نتیجه:
     این کلاس نشان می‌دهد چطور باید Queryها را تعریف کنیم،
     چطور پارامترهای صفحه‌بندی را مدیریت کنیم و چطور داده‌ها را به صورت استاندارد برگردانیم.
    */

    public record GetSamplesByPropertyQuery(string property1, int Page = 1, int PageSize = 10)
        : IRequest<Result<IReadOnlyList<SampleEntity>>>;
}
