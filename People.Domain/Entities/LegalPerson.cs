using Core.Domain.Common.EntityProperties;
using Core.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace People.Domain.Entities
{
    public class LegalPerson : BaseEntity, IAuditableEntity
    {
        #region IAuditableEntity Impelement
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow; // 📌 زمان ایجاد
        public string? CreatedBy { get; set; }                      // 📌 کاربر ایجادکننده
        public DateTime? ModifiedAt { get; set; }                   // 📌 زمان آخرین تغییر
        public string? ModifiedBy { get; set; }                     // 📌 کاربر آخرین تغییر
        #endregion

        public Guid FkPartyId { get; private set; }

        public string Title { get; private set; } = null!;

        public string Description { get; private set; } = null!;

        public string? RegisterCode { get; private set; }

        public virtual Party Party { get; private set; } = null!;

        protected LegalPerson() { }
        public LegalPerson(
            Guid _FkPartyId,
            string _Title,
            string _Description,
            string? _RegisterCode
            )
        {
            FkPartyId = _FkPartyId;
            Title = _Title;
            Description = _Description;
            RegisterCode = _RegisterCode;
        }
    }
}
