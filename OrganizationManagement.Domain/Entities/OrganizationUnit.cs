using Core.Domain.Common;
using Core.Domain.Common.EntityProperties;
using Core.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrganizationManagement.Domain.Entities
{
    public class OrganizationUnit :BaseEntity,IAuditableEntity, IAggregateRoot
    {
        #region IAuditableEntity Impelement
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow; // 📌 زمان ایجاد
        public string? CreatedBy { get; set; }                      // 📌 کاربر ایجادکننده
        public DateTime? ModifiedAt { get; set; }                   // 📌 زمان آخرین تغییر
        public string? ModifiedBy { get; set; }                     // 📌 کاربر آخرین تغییر
        #endregion
        public string Name { get; private set; }
        public string Code { get; private set; }
        public Guid? ParentId { get; private set; }

        // Materialized Path: مثلا "/RootGuid/ParentGuid/MyGuid/"
        // ایندکس کردن این فیلد در دیتابیس حیاتی است برای Like Query سریع
        public string Path { get; private set; }

        public virtual OrganizationUnit? Parent { get; private set; }
        public virtual ICollection<OrganizationUnit> Children { get; private set; } = new List<OrganizationUnit>();
        public virtual ICollection<Position> Positions { get; private set; } = new List<Position>();

        protected OrganizationUnit() { }

        public OrganizationUnit(string name, string code, Guid? parentId)
        {
            Name = name;
            Code = code;
            ParentId = parentId;
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
