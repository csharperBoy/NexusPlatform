using Core.Domain.Attributes;
using Core.Domain.Common.EntityProperties;
using Core.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Domain.Entities
{

    [SecuredResource("Core.Menu")]
    public class Menu : BaseEntity,IHierarchicalStructureEntity<Menu , Guid?> , IAuditableEntity, IOwnerableEntity, IAggregateRoot
    {
        #region IAuditableEntity Impelement
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow; // 📌 زمان ایجاد
        public string? CreatedBy { get; set; }                      // 📌 کاربر ایجادکننده
        public DateTime? ModifiedAt { get; set; }                   // 📌 زمان آخرین تغییر
        public string? ModifiedBy { get; set; }                     // 📌 کاربر آخرین تغییر
        #endregion

        #region IOwnerableEntity Impelement
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
        #region IHierarchicalStructureEntity Impelement
        public Guid? ParentId { get; private set; }
        public virtual Menu? Parent { get; private set; }
        public virtual ICollection<Menu> Children { get; private set; } = new List<Menu>();

        #endregion
        public string Title { get; set; }
        public string Description { get; set; }
        public string Path { get; set; }
        /// <summary>
        ///  به صورت "fa-solid:folder" یا "md-folder" ذخیره می‌شود.
        ///  مثال: "fa-solid:folder" (Font Awesome) یا "md-folder" (Material Design).
        /// </summary>
        public Icon Icon { get; set; }
        public int Order { get; set; }
      
    }

    


}
