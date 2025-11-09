using Azure;
using Core.Application.Abstractions;
using Core.Application.Abstractions.Auditing;
using Core.Application.Abstractions.Caching;
using Core.Application.Abstractions.Events;
using Core.Application.Abstractions.Security;
using Core.Infrastructure.Repositories;
using Core.Shared.Results;
using Microsoft.Extensions.Logging;
using Sample.Application.DTOs;
using Sample.Application.Interfaces;
using Sample.Domain.Entities;
using Sample.Domain.Events;
using Sample.Domain.Specifications;
using Sample.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
namespace Sample.Infrastructure.Services
{
    /*
     📌 SampleService
     ----------------
     این کلاس سرویس اصلی (Command Service) ماژول Sample است.
     وظیفه‌اش مدیریت عملیات نوشتن (Write Operations) روی دیتابیس و انتشار رویدادهای دامنه است.

     ✅ نکات کلیدی:
     - از IRepository استفاده می‌کند تا عملیات CRUD روی SampleEntity انجام دهد.
     - از IUnitOfWork استفاده می‌کند تا تغییرات ذخیره شوند و رویدادهای دامنه به Outbox اضافه شوند.
     - از ILogger برای ثبت لاگ استفاده می‌کند.
     - از ICurrentUserService برای دسترسی به اطلاعات کاربر فعلی استفاده می‌کند.
     - از ICacheService برای مدیریت کش و invalidation بعد از عملیات نوشتن استفاده می‌کند.

     🛠 متدها:
     1. SampleApiMethodAsync:
        - یک SampleEntity جدید می‌سازد.
        - متد MarkSample را صدا می‌زند تا property1 تغییر کند و رویداد دامنه تولید شود.
        - موجودیت را در Repository ذخیره می‌کند.
        - تغییرات را با UnitOfWork ذخیره می‌کند (رویدادها وارد Outbox می‌شوند).
        - نتیجه موفقیت‌آمیز برمی‌گرداند.

     2. SampleApiMethodWithCacheAsync:
        - مشابه متد قبلی است، اما بعد از ذخیره تغییرات کش مرتبط با property1 را پاک می‌کند.
        - این کار تضمین می‌کند که داده‌های کش شده قدیمی نباشند.

     3. InvalidateSampleCachesAsync:
        - کش مرتبط با property1 را با استفاده از RemoveByPatternAsync پاک می‌کند.
        - این الگو باعث می‌شود همه‌ی کلیدهای کش مرتبط با property1 حذف شوند.

     📌 نتیجه:
     این کلاس نشان می‌دهد چطور باید عملیات نوشتن در معماری DDD انجام شود:
     - موجودیت تغییر می‌کند و رویداد دامنه تولید می‌شود.
     - تغییرات با UnitOfWork ذخیره می‌شوند و Outbox پر می‌شود.
     - کش بعد از عملیات نوشتن پاک می‌شود تا داده‌های جدید در Query Service دوباره خوانده شوند.
    */

    public class SampleService : ISampleService
    {
        private readonly IRepository<SampleDbContext, SampleEntity, Guid> _repository;
        private readonly IUnitOfWork<SampleDbContext> _uow;
        private readonly ILogger<SampleService> _logger;
        private readonly ICurrentUserService _currentUser;
        private readonly ICacheService _cache;

        public SampleService(
            IRepository<SampleDbContext, SampleEntity, Guid> repository,
            IUnitOfWork<SampleDbContext> uow,
            ILogger<SampleService> logger,
            ICurrentUserService currentUser,
            ICacheService cache)
        {
            _repository = repository;
            _uow = uow;
            _logger = logger;
            _currentUser = currentUser;
            _cache = cache; // ⚠️ در نسخه‌ی اولیه فراموش شده بود مقداردهی شود
        }

        // 📌 عملیات نوشتن ساده بدون کش
        public async Task<Result<SampleApiResponse>> SampleApiMethodAsync(SampleApiRequest request)
        {
            var entity = new SampleEntity();
            entity.MarkSample(request.property1);

            await _repository.AddAsync(entity);

            // ذخیره تغییرات؛ UoW رویدادهای دامنه را به outbox اضافه می‌کند
            await _uow.SaveChangesAsync();

            return Result<SampleApiResponse>.Ok(new SampleApiResponse("ok"));
        }

        // 📌 عملیات نوشتن همراه با پاک کردن کش
        public async Task<Result<SampleApiResponse>> SampleApiMethodWithCacheAsync(SampleApiRequest request)
        {
            var entity = new SampleEntity();
            entity.MarkSample(request.property1);

            await _repository.AddAsync(entity);
            await _uow.SaveChangesAsync();

            // پاک کردن کش مرتبط با property1
            await InvalidateSampleCachesAsync(request.property1);

            return Result<SampleApiResponse>.Ok(new SampleApiResponse("ok"));
        }

        // 📌 متد کمکی برای پاک کردن کش بعد از نوشتن
        private async Task InvalidateSampleCachesAsync(string property1)
        {
            await _cache.RemoveByPatternAsync($"sample:list:{property1}");
        }
    }
}
