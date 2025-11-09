using Core.Application.Abstractions;
using Core.Application.Abstractions.Caching;
using Core.Application.Abstractions.Security;
using Core.Infrastructure.Repositories;
using Core.Shared.Results;
using Microsoft.Extensions.Logging;
using Sample.Application.Interfaces;
using Sample.Domain.Entities;
using Sample.Domain.Specifications;
using Sample.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sample.Infrastructure.Services
{
    public class SampleQueryService: ISampleQueryService
    {

        private readonly ISpecificationRepository<SampleEntity, Guid> _repository;
        private readonly ILogger<SampleService> _logger; 
        private readonly ICacheService _cache;
        public SampleQueryService(
           ILogger<SampleService> logger,
           ISpecificationRepository<SampleEntity, Guid> repository,
           ICacheService cache)
        {
            _logger = logger;
            _repository = repository;
            _cache = cache;
        }
        // نمونه‌ی خواندن با Specification شامل صفحه‌بندی و ترتیب
        public async Task<Result<IReadOnlyList<SampleEntity>>> GetBySpecAsync(string property1, int page = 1, int pageSize = 10)
        {
            var spec = new SampleGetSpec(property1);
            // ترتیب و صفحه‌بندی (اختیاری)
            // اگر BaseSpecification را اصلاح کنید تا متدهای protected را بیرون بدهد،
            // می‌توانید در خود Spec این‌ها را اعمال کنید؛ فعلاً اینجا نشان می‌دهیم که معیار تعریف شده و Count/Items را برمی‌گردانیم.

            var (items, totalCount) = await _repository.FindBySpecAsync(spec);
            var ordered = items.OrderBy(x => x.CreatedAt).ToList();
            var paged = ordered.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            _logger.LogInformation("Spec results: Total={Total}, Page={Page}, PageSize={PageSize}, Returned={Returned}",
                totalCount, page, pageSize, paged.Count);

            return Result<IReadOnlyList<SampleEntity>>.Ok(paged);
        }

        public async Task<Result<IReadOnlyList<SampleEntity>>> GetCachedSamplesAsync(string property1)
        {
            var cacheKey = $"sample:list:{property1}";
            var cached = await _cache.GetAsync<IReadOnlyList<SampleEntity>>(cacheKey);
            if (cached != null)
            {
                _logger.LogInformation("Cache hit for {Key}", cacheKey);
                return Result<IReadOnlyList<SampleEntity>>.Ok(cached);
            }

            var spec = new SampleGetSpec(property1);
            var list = (await _repository.ListBySpecAsync(spec)).ToList();
            await _cache.SetAsync(cacheKey, list, TimeSpan.FromMinutes(5));
            _logger.LogInformation("Cache set for {Key}", cacheKey);

            return Result<IReadOnlyList<SampleEntity>>.Ok(list);
        }

        
    }
}
