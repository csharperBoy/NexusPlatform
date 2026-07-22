using Core.Domain.Common.EntityProperties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HR.Domain.Entities
{
    /// <summary>
    /// عنوان شغلی
    /// مهندس نرم‌افزار، مدیر بازاریابی، حسابدار ارشد، مشاور حقوقی، اپراتور تولید
    /// </summary>
    public class JobTitle :BaseEntity ,IAuditableEntity//, IHierarchicalStructureEntity<JobTitle,Guid?>
    {
        /*    #region IHierarchicalStructureEntity Impelement
       public Guid? FkParentId { get; private set; }
       public virtual JobTitle? Parent { get; private set; }
       public virtual ICollection<JobTitle> Children { get; private set; } = new List<JobTitle>();
       public void ChangeParent(Guid? newParentId)
       {
           if (newParentId == Id)
               throw new InvalidOperationException("JobTitle cannot be its own parent.");

           FkParentId = newParentId;
           Touch();

           // ارسال ایونت وقتی ساختار سلسله مراتب تغییر می‌کند
           //AddDomainEvent(new MenuHierarchyChangedEvent(Id));
       }
       #endregion*/
        #region IAuditableEntity Impelement
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow; // 📌 زمان ایجاد
        public string? CreatedBy { get; set; }                      // 📌 کاربر ایجادکننده
        public DateTime? ModifiedAt { get; set; }                   // 📌 زمان آخرین تغییر
        public string? ModifiedBy { get; set; }                     // 📌 کاربر آخرین تغییر

        public void Touch() => ModifiedAt = DateTime.UtcNow;
        #endregion

        public string Code { get; private set; }
        public string Name { get; private set; }
        public bool IsActive { get; private set; }

        public virtual ICollection<Post> Posts { get; set; } = new List<Post>();
   
        public bool ApplyChange(
                  string? _Code ,
            string? _Name ,
                bool? _IsActive
           )
        {
            bool hasChange = false;

            if (_Code.Trim() != null && _Code.Trim() != this.Code.Trim())
            {
                this.Code = _Code;
                hasChange = true;
            }
            if (_Name.Trim() != null && _Name.Trim() != this.Name.Trim())
            {
                this.Name = _Name;
                hasChange = true;
            }
            if (_IsActive != null && _IsActive != this.IsActive)
            {
                this.IsActive = (bool)_IsActive;
                hasChange = true;
            }

            if (hasChange)
            {
                Touch();
            }
            return hasChange;
        }
        protected JobTitle()
        {
            
        }
        public JobTitle(
             string _Code,
             string _Name

            )
        {
            Code= _Code;
            Name= _Name;
            IsActive = true;
        }
        public void SetName(string _Name)
        {
            Name = _Name;
        }

    }
}
