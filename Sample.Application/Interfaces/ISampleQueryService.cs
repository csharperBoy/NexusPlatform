using Core.Shared.Results;
using Sample.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace Sample.Application.Interfaces
{
    /*
     📌 ISampleQueryService
     ----------------------
     اینترفیس سرویس مخصوص **Query** در الگوی CQRS.

     ✅ نکات کلیدی:
     - این سرویس فقط برای عملیات خواندن داده‌ها (Read) استفاده می‌شود.
     - هیچ عملیات نوشتن یا تغییر وضعیت در این سرویس وجود ندارد.
     - خروجی همه متدها در قالب Result<T> است تا موفقیت یا شکست عملیات مشخص شود.

     🛠 متدها:
     1. GetBySpecAsync
        - داده‌ها را بر اساس Specification (شرط‌های تعریف‌شده در Domain) می‌خواند.
        - از پارامترهای page و pageSize برای صفحه‌بندی استفاده می‌کند.
        - مناسب برای کوئری‌های پیچیده و قابل توسعه.

     2. GetCachedSamplesAsync
        - ابتدا داده‌ها را از Cache می‌خواند.
        - اگر داده در Cache نبود، از دیتابیس خوانده می‌شود و سپس در Cache ذخیره می‌شود.
        - مناسب برای بهبود کارایی و کاهش بار روی دیتابیس.

     📌 نتیجه:
     این اینترفیس نشان می‌دهد که در معماری CQRS، سرویس‌های Query باید جدا از سرویس‌های Command باشند
     و فقط مسئول خواندن داده‌ها هستند.
    */

    public interface ISampleQueryService
    {
        Task<Result<IReadOnlyList<SampleEntity>>> GetBySpecAsync(string property1, int page = 1, int pageSize = 10);
        Task<Result<IReadOnlyList<SampleEntity>>> GetCachedSamplesAsync(string property1);
    }
}
