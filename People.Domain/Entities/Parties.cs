using Core.Domain.Common.EntityProperties;
using Core.Domain.Interfaces;

namespace People.Domain.Entities
{
    public class Parties : BaseEntity, IAuditableEntity, IAggregateRoot
    {
        #region IAuditableEntity Impelement
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow; // 📌 زمان ایجاد
        public string? CreatedBy { get; set; }                      // 📌 کاربر ایجادکننده
        public DateTime? ModifiedAt { get; set; }                   // 📌 زمان آخرین تغییر
        public string? ModifiedBy { get; set; }                     // 📌 کاربر آخرین تغییر
        #endregion

        //navigation
        public virtual ICollection<naturalPersons>? NaturalPersons { get; private set; }
        public virtual ICollection<legalPersons>? legalPersons { get; private set; }

        public virtual ICollection<PartiesRelations>? sourceRealations { get; private set; }
        public virtual ICollection<PartiesRelations>? destinationRealations { get; private set; }

        public virtual ICollection<PersonContact>? contacts { get; private set; }
        // Constructor for EF
        public Parties() { }

       
        private void Touch() => ModifiedAt = DateTime.UtcNow;
        
    }

}
