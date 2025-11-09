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
        }
        
        public async Task<Result<SampleApiResponse>> SampleApiMethodAsync(SampleApiRequest request)
        {
            var entity = new SampleEntity();
            entity.MarkSample(request.property1);

            await _repository.AddAsync(entity);

            // ذخیره تغییرات؛ UoW رویدادهای دامنه را به outbox اضافه می‌کند
            await _uow.SaveChangesAsync();

            return Result<SampleApiResponse>.Ok(new SampleApiResponse("ok"));
        }


        // نمونه کاربرد invalidation بعد از نوشتن:
        public async Task<Result<SampleApiResponse>> SampleApiMethodWithCacheAsync(SampleApiRequest request)
        {
            var entity = new SampleEntity();
            entity.MarkSample(request.property1);

            await _repository.AddAsync(entity);
            await _uow.SaveChangesAsync();

            await InvalidateSampleCachesAsync(request.property1);

            return Result<SampleApiResponse>.Ok(new SampleApiResponse("ok"));
        }
        // پس از ایجاد/ویرایش، کش را پاک کنید
        private async Task InvalidateSampleCachesAsync(string property1)
        {
            await _cache.RemoveByPatternAsync($"sample:list:{property1}");
        }
    }
}