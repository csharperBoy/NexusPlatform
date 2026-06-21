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
    public class PartiesRelations : BaseEntity, IAuditableEntity, IAggregateRoot
    {
        #region IAuditableEntity Impelement
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow; // 📌 زمان ایجاد
        public string? CreatedBy { get; set; }                      // 📌 کاربر ایجادکننده
        public DateTime? ModifiedAt { get; set; }                   // 📌 زمان آخرین تغییر
        public string? ModifiedBy { get; set; }                     // 📌 کاربر آخرین تغییر
        #endregion

        public Guid sourcePartyId { get; set; }
        public Guid destinationPartyId { get; set; }
        public PartiesRelationsType relationType { get; set; }
        //navigation
        public virtual Parties? sourceParty { get; private set; }
        public virtual Parties? destinationParty { get; private set; }

        // Constructor for EF
        public PartiesRelations() { }


        private void Touch() => ModifiedAt = DateTime.UtcNow;
    }
}
