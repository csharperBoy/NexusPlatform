using Audit.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Audit.Application.Interfaces
{
    public interface IAuditQueryService
    {
        Task<IReadOnlyList<AuditLog>> GetRecentLogsAsync(int count = 100);
    }
}
