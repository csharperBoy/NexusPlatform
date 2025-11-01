using Audit.Domain.Entities;
using Audit.Infrastructure.Data;
using Core.Application.Abstractions;
using Core.Application.Abstractions.Auditing;
using Core.Infrastructure.Repositories;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Audit.Infrastructure.Services
{
    public class AuditService : IAuditService
    {

        private readonly IRepository<AuditDbContext, AuditLog, Guid> _repository;
        private readonly IUnitOfWork<AuditDbContext> _uow;
        private readonly ILogger<AuditService> _logger;

        public AuditService(IRepository<AuditDbContext, AuditLog, Guid> repository, IUnitOfWork<AuditDbContext> uow, ILogger<AuditService> logger)
        {
            _repository = repository;
            _uow = uow;
            _logger = logger;
        }


        public async Task LogAsync(string action, string entityName, string entityId, string? userId, object? changes = null)
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