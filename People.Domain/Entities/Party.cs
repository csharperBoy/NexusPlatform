using Core.Domain.Common.EntityProperties;
using Core.Domain.Interfaces;

namespace People.Domain.Entities
{
    public class Party : BaseEntity, IAuditableEntity, IAggregateRoot
    {
        #region IAuditableEntity Impelement
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow; // 📌 زمان ایجاد
        public string? CreatedBy { get; set; }                      // 📌 کاربر ایجادکننده
        public DateTime? ModifiedAt { get; set; }                   // 📌 زمان آخرین تغییر
        public string? ModifiedBy { get; set; }                     // 📌 کاربر آخرین تغییر
        #endregion
        public Guid FkPermissionAssigneeId { get; private set; }

        //navigation
        public virtual ICollection<LegalPerson> LegalPeople { get; set; } = new List<LegalPerson>();

        public virtual ICollection<NaturalPerson> NaturalPeople { get; set; } = new List<NaturalPerson>();

        public virtual ICollection<PartiesRelation> PartiesRelationFkDestinationParties { get; set; } = new List<PartiesRelation>();

        public virtual ICollection<PartiesRelation> PartiesRelationFkSourceParties { get; set; } = new List<PartiesRelation>();

        public virtual ICollection<PartyContact> PartyContacts { get; set; } = new List<PartyContact>();
        
        // Constructor for EF
        public Party() { }

        public Party(Guid _FkPermissionAssigneeId) { 
        FkPermissionAssigneeId = _FkPermissionAssigneeId;
        }


        private void Touch() => ModifiedAt = DateTime.UtcNow;
        
    }

}
