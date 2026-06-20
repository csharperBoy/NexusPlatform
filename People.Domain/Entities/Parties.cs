using Core.Domain.Common.EntityProperties;
using Core.Domain.Interfaces;
using Core.Domain.ValueObjects;
using Core.Shared.Enums.HR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        public virtual ICollection<naturalPerson>? NaturalPersons { get; private set; }
        public virtual ICollection<legalPersons>? legalPersons { get; private set; }

        // Constructor for EF
        public Parties() { }

       
        private void Touch() => ModifiedAt = DateTime.UtcNow;
        
    }

}
