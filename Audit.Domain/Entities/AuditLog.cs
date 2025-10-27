using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Audit.Domain.Entities
{
    public class AuditLog
    {
        public Guid Id { get; private set; } = Guid.NewGuid();
        public DateTime Timestamp { get; private set; } = DateTime.UtcNow;

        public string Action { get; private set; } = string.Empty;
        public string EntityName { get; private set; } = string.Empty;
        public string EntityId { get; private set; } = string.Empty;
        public string? UserId { get; private set; }
        public string? Changes { get; private set; }

        protected AuditLog() { }

        public AuditLog(string action, string entityName, string entityId, string? userId, string? changes = null)
        {
            Action = action;
            EntityName = entityName;
            EntityId = entityId;
            UserId = userId;
            Changes = changes;
        }
    }
}

