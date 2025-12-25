using Authorization.Domain.Enums;
using Authorization.Domain.Events;
using Core.Domain.Common;
using Core.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Authorization.Domain.Entities
{
        public class Resource : AuditableEntity, IAggregateRoot
        {
            public string Key { get; private set; }
            public string Name { get; private set; }
            public string Description { get; private set; }
            public ResourceType Type { get; private set; }
            public ResourceCategory Category { get; private set; }
            public bool IsActive { get; private set; } = true;
            public int DisplayOrder { get; private set; }
            public string Icon { get; private set; }
            public string Route { get; private set; }

            public Guid? ParentId { get; private set; }
            public string Path { get; private set; }

            // Navigation Properties
            public virtual Resource? Parent { get; private set; }
            public virtual ICollection<Resource> Children { get; private set; } = new List<Resource>();
            public virtual ICollection<Permission> Permissions { get; private set; } = new List<Permission>();
            public virtual ICollection<DataScope> DataScopes { get; private set; } = new List<DataScope>();

            protected Resource() { }

            public Resource(
                string key,
                string name,
                ResourceType type,
                ResourceCategory category,
                Guid? parentId = null,
                string description = "",
                int displayOrder = 0,
                string icon = "",
                string route = "",
                string createdBy = "system")
            {
                if (string.IsNullOrWhiteSpace(key)) throw new ArgumentException("Resource key cannot be empty.");
                if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Resource name cannot be empty.");

                Key = key.Trim().ToLowerInvariant();
                Name = name.Trim();
                Type = type;
                Category = category;
                ParentId = parentId;
                Description = description;
                DisplayOrder = displayOrder;
                Icon = icon;
                Route = route;
                CreatedBy = createdBy;
                CreatedAt = DateTime.UtcNow;

                GeneratePath();
            }

            public void Update(
                string name,
                string description,
                ResourceType type,
                ResourceCategory category,
                int displayOrder,
                string icon,
                string route)
            {
                if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Resource name cannot be empty.");

                Name = name.Trim();
                Description = description;
                Type = type;
                Category = category;
                DisplayOrder = displayOrder;
                Icon = icon;
                Route = route;
                ModifiedAt = DateTime.UtcNow;
            }

            public void ChangeParent(Guid? newParentId)
            {
                if (newParentId == Id)
                    throw new InvalidOperationException("Resource cannot be its own parent.");

                ParentId = newParentId;
                GeneratePath();
                ModifiedAt = DateTime.UtcNow;

                // ارسال ایونت وقتی ساختار سلسله مراتب تغییر می‌کند
                AddDomainEvent(new ResourceHierarchyChangedEvent(Id));
            }

            public void Activate()
            {
                if (IsActive) return;
                IsActive = true;
                ModifiedAt = DateTime.UtcNow;
            }

            public void Deactivate()
            {
                if (!IsActive) return;
                IsActive = false;
                ModifiedAt = DateTime.UtcNow;
            }

            public void Reorder(int newDisplayOrder)
            {
                if (DisplayOrder == newDisplayOrder) return;
                DisplayOrder = newDisplayOrder;
                ModifiedAt = DateTime.UtcNow;
            }

            public void GeneratePath()
            {
                if (ParentId.HasValue && Parent != null)
                {
                    Path = $"{Parent.Path}/{Id}";
                }
                else
                {
                    Path = Id.ToString();
                }
            }
        [NotMapped]
        public bool IsUiResource => Type == ResourceType.Ui;
        [NotMapped]
        public bool IsDataResource => Type == ResourceType.Data;
        [NotMapped]
        public bool IsRoot => !ParentId.HasValue;
        }
    
}
