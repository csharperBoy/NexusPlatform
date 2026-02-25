using Core.Domain.Common;
using Core.Domain.Common.EntityProperties;
using Core.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrganizationManagement.Domain.Entities
{
    public class Assignment : BaseEntity,IAuditableEntity, IAggregateRoot
    {
        #region IAuditableEntity Impelement
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow; // 📌 زمان ایجاد
        public string? CreatedBy { get; set; }                      // 📌 کاربر ایجادکننده
        public DateTime? ModifiedAt { get; set; }                   // 📌 زمان آخرین تغییر
        public string? ModifiedBy { get; set; }                     // 📌 کاربر آخرین تغییر
        #endregion
        public Guid FkPersonId { get; private set; }
        public Guid FkPositionId { get; private set; }

        public DateTime StartDate { get; private set; }
        public DateTime? EndDate { get; private set; }
        public bool IsActive { get; private set; }
        public virtual Position Position { get; private set; } = null!;
        protected Assignment() { }
        public Assignment(Guid personId, Guid positionId, DateTime startDate, string createdBy, bool isActive = true, DateTime? endDate = null)
        {
            FkPersonId = personId;
            FkPositionId = positionId;
            StartDate = startDate;
            EndDate = endDate;
            CreatedBy = createdBy;
            IsActive = isActive;
        }
    }
}
