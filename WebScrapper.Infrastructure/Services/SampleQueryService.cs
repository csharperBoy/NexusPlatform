using Core.Application.Abstractions;
using Core.Application.Abstractions.Caching;
using Core.Application.Abstractions.Security;
using Core.Infrastructure.Repositories;
using Core.Shared.Results;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebScrapper.Application.Interfaces;

namespace WebScrapper.Infrastructure.Services
{
    /*
     📌 SampleQueryService
     ---------------------
     این کلاس یک سرویس Query در لایه Infrastructure است که وظیفه‌اش
     خواندن داده‌ها از دیتابیس با استفاده از Specification و Repository می‌باشد.

     ✅ نکات کلیدی:
     - از ISpecificationRepository استفاده می‌کند تا Queryها را بر اساس Specification اجرا کند.
     - از ILogger برای ثبت لاگ استفاده می‌کند.
     - از ICacheService برای کش کردن نتایج Query استفاده می‌کند تا کارایی افزایش یابد.

     🛠 متدها:
     1. GetBySpecAsync:
        - داده‌ها را بر اساس Specification (SampleGetSpec) می‌خواند.
        - نتایج را مرتب‌سازی (OrderBy) و صفحه‌بندی (Paging) می‌کند.
        - لاگ ثبت می‌کند که چند رکورد کل و چند رکورد در این صفحه برگردانده شده است.
        - خروجی را در قالب Result<IReadOnlyList<SampleEntity>> برمی‌گرداند.

     2. GetCachedSamplesAsync:
        - ابتدا بررسی می‌کند که آیا داده‌ها در Cache موجود هستند یا نه.
        - اگر داده در Cache موجود بود → همان را برمی‌گرداند (Cache Hit).
        - اگر نبود → داده‌ها را از Repository می‌خواند، در Cache ذخیره می‌کند و سپس برمی‌گرداند.
        - Cache با کلید "sample:list:{property1}" و مدت زمان ۵ دقیقه ذخیره می‌شود.

     📌 نتیجه:
     این کلاس نشان می‌دهد چطور می‌توان:
     - از Specification برای Queryهای پیچیده استفاده کرد.
     - از Repository برای جداسازی لایه دسترسی به داده استفاده کرد.
     - از Cache برای بهبود کارایی و کاهش بار روی دیتابیس استفاده کرد.
     - از لاگ برای مانیتورینگ و عیب‌یابی استفاده کرد.
    */

    public class SampleQueryService : ISampleQueryService
    {
        private readonly ISpecificationRepository<SampleEntity, Guid> _repository;
        private readonly ILogger<PlayWrightScrapperService> _logger;
        private readonly ICacheService _cache;

        public SampleQueryService(
           ILogger<PlayWrightScrapperService> logger,
           ISpecificationRepository<SampleEntity, Guid> repository,
           ICacheService cache)
        {
            _logger = logger;
            _repository = repository;
            _cache = cache;
        }

        // 📌 نمونه‌ی خواندن با Specification شامل صفحه‌بندی و ترتیب
        public async Task<Result<IReadOnlyList<SampleEntity>>> GetBySpecAsync(string property1, int page = 1, int pageSize = 10)
        {
            var spec = new SampleGetSpec(property1);

            // اجرای Specification
            var (items, totalCount) = await _repository.FindBySpecAsync(spec);

            // مرتب‌سازی و صفحه‌بندی
            var ordered = items.OrderBy(x => x.CreatedAt).ToList();
            var paged = ordered.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            // ثبت لاگ
            _logger.LogInformation("Spec results: Total={Total}, Page={Page}, PageSize={PageSize}, Returned={Returned}",
                totalCount, page, pageSize, paged.Count);

            return Result<IReadOnlyList<SampleEntity>>.Ok(paged);
        }

        // 📌 نمونه‌ی خواندن با Cache
        public async Task<Result<IReadOnlyList<SampleEntity>>> GetCachedSamplesAsync(string property1)
        {
            var cacheKey = $"sample:list:{property1}";

            // بررسی Cache
            var cached = await _cache.GetAsync<IReadOnlyList<SampleEntity>>(cacheKey);
            if (cached != null)
            {
                _logger.LogInformation("Cache hit for {Key}", cacheKey);
                return Result<IReadOnlyList<SampleEntity>>.Ok(cached);
            }

            // اگر Cache خالی بود → خواندن از Repository
            var spec = new SampleGetSpec(property1);
            var list = (await _repository.ListBySpecAsync(spec)).ToList();

            // ذخیره در Cache
            await _cache.SetAsync(cacheKey, list, TimeSpan.FromMinutes(5));
            _logger.LogInformation("Cache set for {Key}", cacheKey);

            return Result<IReadOnlyList<SampleEntity>>.Ok(list);
        }
    }
}
