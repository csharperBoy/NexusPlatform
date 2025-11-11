using Audit.Domain.Entities;
using Core.Shared.Results;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Audit.Application.Interfaces
{
    public interface IAuditQueryService
    {
        Task<Result<IReadOnlyList<AuditLog>>> GetRecentLogsAsync(int page = 1, int pageSize = 100);
    }
}
