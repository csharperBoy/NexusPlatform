using Audit.Application.Interfaces;
using Audit.Domain.Entities;
using Audit.Domain.Specifications;
using Audit.Infrastructure.Data;
using Core.Application.Abstractions;
using Core.Application.Abstractions.Auditing;
using Core.Application.Abstractions.Caching.PublicService;
using Core.Infrastructure.Repositories;
using Core.Shared.Results;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Audit.Infrastructure.Services
{
    public class AuditService : IAuditInternalService
    {

        private readonly IRepository<AuditDbContext, AuditLog, Guid> _repository;
        private readonly IUnitOfWork<AuditDbContext> _uow;
        private readonly ILogger<AuditService> _logger;

        private readonly ISpecificationRepository<AuditLog, Guid> _specRepository;
        private readonly ICachePublicService _cache;

        public AuditService(IRepository<AuditDbContext, AuditLog, Guid> repository,
            ISpecificationRepository<AuditLog, Guid> specRepository, IUnitOfWork<AuditDbContext> uow, ILogger<AuditService> logger)
        {
            _repository = repository;
            _specRepository = specRepository;
            _uow = uow;
            _logger = logger;
        }
        public async Task<Result<IReadOnlyList<AuditLog>>> GetRecentLogsAsync(int page = 1, int pageSize = 100)
        {
            var spec = new RecentAuditLogsSpec(page, pageSize);
            var (items, totalCount) = await _specRepository.FindBySpecAsync(spec);

            // ثبت لاگ
            _logger.LogInformation("Spec results: Total={Total}, Page={Page}, PageSize={PageSize}, Returned={Returned}",
                totalCount, page, pageSize, 0);

            return Result<IReadOnlyList<AuditLog>>.Ok(items.ToList());
        }

        public async Task LogAsync(string action, string entityName, string entityId, Guid? userId, object? changes = null)
        {
            string? serializedChanges = null;

            if (changes != null)
            {
                try
                {
                    serializedChanges = JsonSerializer.Serialize(changes, new JsonSerializerOptions
                    {
                        WriteIndented = false,
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                    });
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to serialize audit changes for {Entity}", entityName);
                    serializedChanges = $"[SerializationError] {changes.GetType().Name}";
                }
            }

            var log = new AuditLog(action, entityName, entityId, userId, serializedChanges);
            await _repository.AddAsync(log);
            await _uow.SaveChangesAsync();
        }

    }
}