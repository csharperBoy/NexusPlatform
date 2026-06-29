using Core.Domain.Common.EntityProperties;
using Core.Domain.Interfaces;
using People.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace People.Domain.Entities
{
    public class PartiesRelation : BaseEntity, IAuditableEntity, IAggregateRoot
    {
        #region IAuditableEntity Impelement
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow; // 📌 زمان ایجاد
        public string? CreatedBy { get; set; }                      // 📌 کاربر ایجادکننده
        public DateTime? ModifiedAt { get; set; }                   // 📌 زمان آخرین تغییر
        public string? ModifiedBy { get; set; }                     // 📌 کاربر آخرین تغییر
        #endregion
       
        public Guid FkSourcePartyId { get; set; }
        public Guid FkDestinationPartyId { get; set; }
        public PartiesRelationsType RelationType { get; set; }
        //navigation
       
        public virtual Party FkDestinationParty { get; set; } = null!;

        public virtual Party FkSourceParty { get; set; } = null!;

        // Constructor for EF
        public PartiesRelation() { }


        private void Touch() => ModifiedAt = DateTime.UtcNow;
    }
}
