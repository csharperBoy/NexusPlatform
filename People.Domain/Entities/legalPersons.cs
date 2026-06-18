using Core.Domain.Common.EntityProperties;
using Core.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace People.Domain.Entities
{
    public class legalPersons : BaseEntity, IAuditableEntity
    {
        #region IAuditableEntity Impelement
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow; // 📌 زمان ایجاد
        public string? CreatedBy { get; set; }                      // 📌 کاربر ایجادکننده
        public DateTime? ModifiedAt { get; set; }                   // 📌 زمان آخرین تغییر
        public string? ModifiedBy { get; set; }                     // 📌 کاربر آخرین تغییر
        #endregion

        public Guid fkPartyId { get; private set; }
        public string Title { get; private set; }
        public string Description { get; private set; }
        public string? RegisterCode { get; private set; }
        public virtual Parties party { get; private set; }
        protected legalPersons() { }
        public legalPersons(
            Guid _fkPartyId,
            string _Title,
            string _Description,
            string? _RegisterCode
            )
        {
            fkPartyId = _fkPartyId;
            Title = _Title;
            Description = _Description;
            RegisterCode = _RegisterCode;
        }
    }
}
