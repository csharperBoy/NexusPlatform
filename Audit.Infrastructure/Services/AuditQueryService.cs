using Audit.Application.Interfaces;
using Audit.Domain.Entities;
using Audit.Domain.Specifications;
using Audit.Infrastructure.Data;
using Core.Application.Abstractions;
using Microsoft.EntityFrameworkCore;
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

        public AuditQueryService(ISpecificationRepository<AuditLog, Guid> repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<AuditLog>> GetRecentLogsAsync(int count = 100)
        {
            var spec = new RecentAuditLogsSpec(count);
            return await _repository.ListBySpecAsync(spec);
        }
    }


}
