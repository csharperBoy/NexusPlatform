using Core.Domain.Common;
using Core.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;

namespace Authorization.Domain.Entities
{
    public class Resource : AuditableEntity, IAggregateRoot
    {
        public string Name { get; private set; }
        public string Code { get; private set; }
        public ResourceType Type { get; private set; }
        public string Description { get; private set; }
        // سلسله مراتب منابع
        public Guid? ParentId { get; private set; }
        public Resource Parent { get; private set; }
        public ICollection<Resource> Children { get; private set; } = new List<Resource>();

        // دسترسی‌های مرتبط
        public ICollection<Permission> Permissions { get; private set; } = new List<Permission>();

        private Resource() { } // برای EF Core

        public Resource(string name, string code, ResourceType type, string description = null, Guid? parentId = null)
        {
            Name = name;
            Code = code;
            Type = type;
            Description = description;
            ParentId = parentId;

            AddDomainEvent(new ResourceCreatedEvent(Id, name, code, type));
        }

        public void Update(string name, string description)
        {
            Name = name;
            Description = description;
        }

        public void AddChild(Resource child)
        {
            Children.Add(child);
        }
    }
}
}
