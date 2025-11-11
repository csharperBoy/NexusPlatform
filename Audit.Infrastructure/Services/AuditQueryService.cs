using Audit.Application.Interfaces;
using Audit.Domain.Entities;
using Audit.Domain.Specifications;
using Audit.Infrastructure.Data;
using Core.Application.Abstractions;
using Core.Application.Abstractions.Caching;
using Core.Shared.Results;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Audit.Infrastructure.Services
{
    public class AuditQueryService : IAuditQueryService
    {
        private readonly ISpecificationRepository<AuditLog, Guid> _repository;
        private readonly ILogger<AuditQueryService> _logger;
        private readonly ICacheService _cache;

        public AuditQueryService(
           ILogger<AuditQueryService> logger,
            ISpecificationRepository<AuditLog, Guid> repository,
           ICacheService cache)
        {
            _logger = logger;
            _repository = repository;
            _cache = cache;
        }


        public async Task<Result<IReadOnlyList<AuditLog>>> GetRecentLogsAsync(int page = 1, int pageSize = 100)
        {
            var spec = new RecentAuditLogsSpec(page,pageSize);
            var (items, totalCount) = await _repository.FindBySpecAsync(spec);

            // ثبت لاگ
            _logger.LogInformation("Spec results: Total={Total}, Page={Page}, PageSize={PageSize}, Returned={Returned}",
                totalCount, page, pageSize,0);

            return Result<IReadOnlyList<AuditLog>>.Ok(items.ToList());
        }
    }


}
