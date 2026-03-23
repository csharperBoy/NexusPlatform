using Authorization.Domain.Enums;
using Authorization.Domain.Events;
using Core.Domain.Attributes;
using Core.Domain.Common;
using Core.Domain.Common.EntityProperties;
using Core.Domain.Interfaces;
using Core.Shared.Enums.Authorization;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Authorization.Domain.Entities
{

    [SecuredResource("Authorization.Resource")]
    public class Resource : BaseEntity , IAuditableEntity, IOwnerableEntity, IAggregateRoot, IHierarchicalStructureEntity<Resource, Guid?>
    {
        #region IAuditableEntity Impelement
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow; // 📌 زمان ایجاد
        public string? CreatedBy { get; set; }                      // 📌 کاربر ایجادکننده
        public DateTime? ModifiedAt { get; set; }                   // 📌 زمان آخرین تغییر
        public string? ModifiedBy { get; set; }                     // 📌 کاربر آخرین تغییر
        #endregion

        #region IHierarchicalStructureEntity Impelement
        public Guid? ParentId { get; private set; }
        public virtual Resource? Parent { get; private set; }
        public virtual ICollection<Resource> Children { get; private set; } = new List<Resource>();
      
        #endregion

        #region IDataScopedEntity Impelement
        public Guid? OwnerOrganizationUnitId { get; protected set; }
        public Guid? OwnerPositionId { get; protected set; }
        public Guid? OwnerPersonId { get; protected set; }
        public Guid? OwnerUserId { get; protected set; }

        public void SetOwners(Guid? userId, Guid? personId, Guid? positiontId, Guid? orgUnitId)
        {
            OwnerUserId = userId;
            OwnerPersonId = personId;
            OwnerPositionId = positiontId;
            OwnerOrganizationUnitId = orgUnitId;
        }
        public void SetPersonOwner(Guid personId)
        {
            OwnerPersonId = personId;
        }
        public void SetUserOwner(Guid userId)
        {
            OwnerUserId = userId;
        }
        public void SetPositionOwner(Guid positiontId)
        {
            OwnerPositionId = positiontId;
        }
        public void SetOrganizationUnitOwner(Guid orgUnitId)
        {
            OwnerOrganizationUnitId = orgUnitId;
        }
        #endregion

        public string Key { get; private set; } 
        public string Name { get; private set; }
        public string Description { get; private set; }
        public ResourceType Type { get; private set; }
        public ResourceCategory Category { get; private set; }
        public bool IsActive { get; private set; } = true;
        public int DisplayOrder { get; private set; }
        public string Icon { get; private set; }
        public string? ResourcePath { get; private set; } // سلسله مراتب ریسورس‌ها (مثل منوهای تودرتو)

        public bool hasScope { get; set; } // آیا شروط مربوط به Scope باید اعمال شود روی این؟
        public bool hasFieldBaseCondition { get; set; }// آیا شروط مربوط به FieldBaseCondition باید اعمال شود روی این؟
        public bool hasRelationBaseCondition { get; set; }// آیا شروط مربوط به RelationBaseCondition باید اعمال شود روی این؟

        // ارتباط مستقیم با پرمیشن‌ها
        public virtual ICollection<Permission> Permissions { get; private set; } = new List<Permission>();

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
            string createdBy = "system"
            )
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
            CreatedBy = createdBy;
            CreatedAt = DateTime.UtcNow;
            GeneratePath();
        }

        public void SetPath(string path) => ResourcePath = path;

        public void Update(
            string name,
            string description,
            ResourceType type,
            ResourceCategory category,
            int displayOrder,
            string icon)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Resource name cannot be empty.");

            Name = name.Trim();
            Description = description;
            Type = type;
            Category = category;
            DisplayOrder = displayOrder;
            Icon = icon;
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
                ResourcePath = $"{Parent.ResourcePath}/{Key}";
            }
            else
            {
                ResourcePath = Key;
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
