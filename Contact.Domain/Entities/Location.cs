using Core.Domain.Common.EntityProperties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HR.Domain.Entities
{
    /// <summary>
    /// مکان ها
    /// </summary>
    public class Location : BaseEntity, IAuditableEntity , IHierarchicalStructureEntity<Location,Guid?>
    {
        #region IAuditableEntity Impelement
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow; // 📌 زمان ایجاد
        public string? CreatedBy { get; set; }                      // 📌 کاربر ایجادکننده
        public DateTime? ModifiedAt { get; set; }                   // 📌 زمان آخرین تغییر
        public string? ModifiedBy { get; set; }                     // 📌 کاربر آخرین تغییر
        #endregion
        #region IHierarchicalStructureEntity Impelement
        public Guid? FkParentId { get; private set; }
        public virtual Location? Parent { get; private set; }
        public virtual ICollection<Location> Children { get; private set; } = new List<Location>();
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
        public string Title { get;private set; }
        public Location(string _Title)
        {
         Title = _Title;   
        }
        protected Location()
        {
            
        }
        public bool ApplyChange(
           string? _title = null)
        {

            bool hasChange = false;

            if (_title != null && _title?.Trim() != Title.Trim())
            {
                Title = _title;
                hasChange = true;
            }

            if (hasChange)
            {
                Touch();
            }
            return hasChange;
        }

    
        public void Touch() => ModifiedAt = DateTime.UtcNow;

        //navigate
        public virtual ICollection<EmploymentLocation> EmploymentLocations { get; set; } = new List<EmploymentLocation>();
        public virtual ICollection<PostLocation> PostLocations { get; set; } = new List<PostLocation>();
        public virtual ICollection<LocationContact> LocationContacts { get; set; } = new List<LocationContact>();



    }
}
