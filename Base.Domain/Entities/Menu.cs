using Core.Domain.Attributes;
using Core.Domain.Common.EntityProperties;
using Core.Domain.Interfaces;
using Core.Shared.Enums.Base;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Base.Domain.Entities
{

    [SecuredResource("Base.Menu")]
    public class Menu : BaseEntity, IHierarchicalStructureEntity<Menu, Guid?>, IAuditableEntity, IOwnerableEntity, IAggregateRoot
    {
        #region IAuditableEntity Impelement
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow; // 📌 زمان ایجاد
        public string? CreatedBy { get; set; }                      // 📌 کاربر ایجادکننده
        public DateTime? ModifiedAt { get; set; }                   // 📌 زمان آخرین تغییر
        public string? ModifiedBy { get; set; }                     // 📌 کاربر آخرین تغییر
        private void Touch() => ModifiedAt = DateTime.UtcNow;
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
        public void ChangeParent(Guid? newParentId)
        {
            if (newParentId == Id)
                throw new InvalidOperationException("Menu cannot be its own parent.");

            ParentId = newParentId;
            Touch();

            // ارسال ایونت وقتی ساختار سلسله مراتب تغییر می‌کند
            //AddDomainEvent(new MenuHierarchyChangedEvent(Id));
        }
        #endregion
        public string Title { get; set; }
        public string Key { get; set; }
        public string? Description { get; set; }
        public string Path { get; set; }
        /// <summary>
        ///  به صورت "fa-solid:folder" یا "md-folder" ذخیره می‌شود.
        ///  مثال: "fa-solid:folder" (Font Awesome) یا "md-folder" (Material Design).
        /// </summary>
        public Icon? Icon { get; set; }
        public int? Order { get; set; }
        public Menu(string _Title, string _Key, string? _Description, string _Path, Icon? _Icon, int? _Order,Guid? _ParentId)
        {
            if (string.IsNullOrWhiteSpace(_Key)) throw new ArgumentException("Menu key cannot be empty.");
            if (string.IsNullOrWhiteSpace(_Title)) throw new ArgumentException("Menu Title cannot be empty.");

            Key = _Key.Trim().ToLowerInvariant();
            Title = _Title.Trim();
            Description = _Description;
            Path = _Path;
            Icon = _Icon;
            Order = _Order;
            ParentId = _ParentId;
        }

        public void Update(string _title, string? _description, Icon? _icon, int? _order, string _key)
        {
            Title = _title; Description = _description; Icon = _icon; Order = _order; Key = _key;
        }
        public Menu()
        {
            
        }
    }




}
