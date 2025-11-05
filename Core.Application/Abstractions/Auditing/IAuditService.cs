using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Application.Abstractions.Auditing
{
    public interface IAuditService
    {
        Task LogAsync(string action, string entityName, string entityId, Guid? userId, object? changes = null);
    }
}
