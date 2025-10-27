using Audit.Domain.Entities;
using Audit.Infrastructure.Data;
using Core.Application.Abstractions;
using Core.Application.Abstractions.Auditing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Audit.Infrastructure.Services
{
    public class AuditService : IAuditService
    {

        private readonly IRepository<AuditDbContext, AuditLog, Guid> _repository;
        private readonly IUnitOfWork<AuditDbContext> _uow;

        public AuditService(IRepository<AuditDbContext, AuditLog, Guid> repository, IUnitOfWork<AuditDbContext> uow)
        {
            _repository = repository;
            _uow = uow;
        }

        public async Task LogAsync(string action, string entityName, string entityId, string? userId, object? changes = null)
        {
            var log = new AuditLog(action, entityName, entityId, userId, changes?.ToString());
            await _repository.AddAsync(log);
            await _uow.SaveChangesAsync();
        }
    }
}