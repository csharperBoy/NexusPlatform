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
        // Properties
        public string Key { get; private set; }
        public string Name { get; private set; }
        public string Description { get; private set; }
        public ResourceType Type { get; private set; }
        public ResourceCategory Category { get; private set; }
        public bool IsActive { get; private set; } = true;
        public int DisplayOrder { get; private set; }
        public string Icon { get; private set; }
        public string Route { get; private set; }

        // Hierarchical Structure
        public Guid? ParentId { get; private set; }
        public string Path { get; private set; } // Materialized Path برای کوئری‌های کارآمد

        // Navigation Properties
        public virtual Resource? Parent { get; private set; }
        public virtual ICollection<Resource> Children { get; private set; } = new List<Resource>();
        public virtual ICollection<Permission> Permissions { get; private set; } = new List<Permission>();
        public virtual ICollection<DataScope> DataScopes { get; private set; } = new List<DataScope>();

        // Constructor for EF Core
        protected Resource() { }

        // Main Constructor
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
            ValidateInputs(key, name);

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

            GeneratePath(); // تولید مسیر سلسله مراتبی

            AddDomainEvent(new ResourceCreatedEvent(Id, Key, Name, Type, Category));
        }

        // Domain Methods
        public void Update(
            string name,
            string description,
            ResourceType type,
            ResourceCategory category,
            int displayOrder,
            string icon,
            string route)
        {
            ValidateInputs(Key, name); // Key ثابت می‌ماند

            Name = name.Trim();
            Description = description;
            Type = type;
            Category = category;
            DisplayOrder = displayOrder;
            Icon = icon;
            Route = route;
            ModifiedAt = DateTime.UtcNow;

            AddDomainEvent(new ResourceUpdatedEvent(Id, Name, Type, Category));
        }

        public void ChangeParent(Guid? newParentId)
        {
            if (newParentId == Id)
                throw new AuthorizationDomainException("Resource cannot be its own parent.");

            if (IsDescendant(newParentId))
                throw new AuthorizationDomainException("Cannot set a descendant as parent.");

            ParentId = newParentId;
            GeneratePath(); // مسیر جدید تولید می‌شود
            ModifiedAt = DateTime.UtcNow;

            AddDomainEvent(new ResourceParentChangedEvent(Id, ParentId, newParentId));
        }

        public void Activate()
        {
            if (IsActive) return;

            IsActive = true;
            ModifiedAt = DateTime.UtcNow;
            AddDomainEvent(new ResourceActivatedEvent(Id));
        }

        public void Deactivate()
        {
            if (!IsActive) return;

            IsActive = false;
            ModifiedAt = DateTime.UtcNow;
            AddDomainEvent(new ResourceDeactivatedEvent(Id));
        }

        public void Reorder(int newDisplayOrder)
        {
            if (DisplayOrder == newDisplayOrder) return;

            DisplayOrder = newDisplayOrder;
            ModifiedAt = DateTime.UtcNow;
        }

        // Validation Methods
        private static void ValidateInputs(string key, string name)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new AuthorizationDomainException("Resource key cannot be empty.");

            if (string.IsNullOrWhiteSpace(name))
                throw new AuthorizationDomainException("Resource name cannot be empty.");

            if (key.Length > 100)
                throw new AuthorizationDomainException("Resource key cannot exceed 100 characters.");

            if (name.Length > 200)
                throw new AuthorizationDomainException("Resource name cannot exceed 200 characters.");
        }

        // Path Generation for Hierarchical Queries
        private void GeneratePath()
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

        // Business Logic
        private bool IsDescendant(Guid? potentialParentId)
        {
            if (!potentialParentId.HasValue) return false;

            // در implementation کامل، باید چک کنیم آیا potentialParentId از نوادگان این انتیتی است
            // برای سادگی فعلا false برمی‌گردانیم
            return false;
        }

        public bool IsUiResource => Type == ResourceType.Ui;
        public bool IsDataResource => Type == ResourceType.Data;
        public bool IsRoot => !ParentId.HasValue;
        public int Level => Path?.Count(c => c == '/') ?? 0;
    }
}
