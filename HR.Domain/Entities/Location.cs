using Core.Domain.Common;
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
    public class Location : BaseEntity, IAuditableEntity,ISoftRemovable , IHierarchicalStructureEntity<Location,Guid?>
    {
        #region IAuditableEntity Impelement
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow; // 📌 زمان ایجاد
        public string? CreatedBy { get; set; }                      // 📌 کاربر ایجادکننده
        public DateTime? ModifiedAt { get; set; }                   // 📌 زمان آخرین تغییر
        public string? ModifiedBy { get; set; }                     // 📌 کاربر آخرین تغییر
        #endregion
        #region IHierarchicalStructureEntity Impelement
        public Guid? ParentId { get; private set; }
        public virtual Location? Parent { get; private set; }
        public virtual ICollection<Location> Children { get; private set; } = new List<Location>();
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
        #region ISoftRemovable Impelement
        public bool IsRemove { get; private set; } = false;

        public async Task SetIsRemove(bool value)
        {
            IsRemove = value;
            Touch();
            await Task.CompletedTask;
        }
        #endregion
        public string Title { get;private set; }

        public Guid FkContactProfileId { get; private set; }
        public Location(string _Title, Guid _FkContactProfileId)
        {
         Title = _Title;
         FkContactProfileId = _FkContactProfileId;
        }
        protected Location()
        {
            
        }
        public bool ApplyChange(
          Optional<string?> _title)
        {

            bool hasChange = false;

            if (_title.IsSet && _title.Value?.Trim() != Title.Trim())
            {
                Title = _title.Value?.Trim();
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
        //public virtual ICollection<LocationContact> LocationContacts { get; set; } = new List<LocationContact>();



    }
}
