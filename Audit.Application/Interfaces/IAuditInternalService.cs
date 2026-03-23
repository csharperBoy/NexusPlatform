using Audit.Domain.Entities;
using Core.Application.Abstractions.Auditing.PublicService;
using Core.Shared.Results;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Audit.Application.Interfaces
{
    public interface IAuditInternalService : IAuditPublicService
    {
        Task<Result<IReadOnlyList<AuditLog>>> GetRecentLogsAsync(int page = 1, int pageSize = 100);
    }
}
