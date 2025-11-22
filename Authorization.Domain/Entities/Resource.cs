using Authorization.Domain.Enums;
using Core.Domain.Common;
using Core.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Authorization.Domain.Entities
{
    public class Resource : AuditableEntity, IAggregateRoot
    {
        public string Key { get; private set; } = string.Empty;
        public string Name { get; private set; } = string.Empty;
        public ResourceType Type { get; private set; }
        public ResourceCategory Category { get; private set; }
        
        public Guid? ParentId { get; private set; }

        // Navigation
        public virtual Resource? Parent { get; private set; }
        public virtual ICollection<Resource> Children { get; private set; } = new List<Resource>();
        public virtual ICollection<Permission> Permissions { get; private set; } = new List<Permission>();
        public virtual ICollection<DataScope> DataScopes { get; private set; } = new List<DataScope>();

        protected Resource() { }  // EF Core

        public Resource(string key, string name, ResourceType type, Guid? parentId = null, string createdBy = "system")
        {
            Key = key ?? throw new ArgumentNullException(nameof(key));
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Type = type;
            ParentId = parentId;
            CreatedBy = createdBy;
        }

        // Domain Behaviors
        public void Update(string name, ResourceType type)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Type = type;
            ModifiedAt = DateTime.UtcNow;  // دستی ست می‌کنیم اگر لازم باشه (یا در SaveChanges)
        }

        public void ChangeParent(Guid? newParentId)
        {
            if (newParentId == Id)
                throw new InvalidOperationException("Resource cannot be its own parent.");

            ParentId = newParentId;
            ModifiedAt = DateTime.UtcNow;
        }
    }
}
