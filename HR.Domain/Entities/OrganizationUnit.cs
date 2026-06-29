using Core.Domain.Common;
using Core.Domain.Common.EntityProperties;
using Core.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HR.Domain.Entities
{
    /// <summary>
    /// واحد سازمانی
    /// </summary>
    public class OrganizationUnit :BaseEntity,IAuditableEntity, IAggregateRoot , IHierarchicalStructureEntity<OrganizationUnit, Guid?>
    {
        #region IAuditableEntity Impelement
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow; // 📌 زمان ایجاد
        public string? CreatedBy { get; set; }                      // 📌 کاربر ایجادکننده
        public DateTime? ModifiedAt { get; set; }                   // 📌 زمان آخرین تغییر
        public string? ModifiedBy { get; set; }                     // 📌 کاربر آخرین تغییر
        #endregion
        #region IHierarchicalStructureEntity Impelement
        public Guid? FkParentId { get; private set; }
        public virtual OrganizationUnit? Parent { get; private set; }
        public virtual ICollection<OrganizationUnit> Children { get; private set; } = new List<OrganizationUnit>();
        public void ChangeParent(Guid? newParentId)
        {
            if (newParentId == Id)
                throw new InvalidOperationException("Menu cannot be its own parent.");

            FkParentId = newParentId;
            Touch();

            // ارسال ایونت وقتی ساختار سلسله مراتب تغییر می‌کند
            //AddDomainEvent(new MenuHierarchyChangedEvent(Id));
        }
        #endregion

        private void Touch() => ModifiedAt = DateTime.UtcNow;

        public string Name { get; private set; }
        public string Code { get; private set; }

        // Materialized Path: مثلا "/RootGuid/ParentGuid/MyGuid/"
        // ایندکس کردن این فیلد در دیتابیس حیاتی است برای Like Query سریع
        public string Path { get; private set; }


        public virtual ICollection<Post> Posts { get; set; } = new List<Post>();



        protected OrganizationUnit() { }

        public OrganizationUnit(string name, string code, Guid? parentId)
        {
            Name = name;
            Code = code;
            FkParentId = parentId;
            // Path در زمان سرویس ایجاد (Create) مقداردهی اولیه می‌شود
            // چون اینجا Id هنوز شاید تولید نشده باشد (اگر از Identity دیتابیس استفاده کنید)
            // اما چون Guid دارید، می‌توانید همینجا بسازید
            Path = string.Empty;
        }

        // متدی که Domain Service بعد از تعیین Parent صدا می‌زند
        public void UpdatePath(string parentPath)
        {
            // فرمت استاندارد: /ParentPath/MyId/
            // این باعث می‌شود با LIKE '/Path/%' تمام فرزندان را بگیرید
            Path = string.IsNullOrEmpty(parentPath)
                   ? $"/{Id}/"
                   : $"{parentPath}{Id}/";
        }
    }
}
