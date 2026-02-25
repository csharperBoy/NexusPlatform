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
    public class Position : BaseEntity , IAuditableEntity, IAggregateRoot
    {
        #region IAuditableEntity Impelement
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow; // 📌 زمان ایجاد
        public string? CreatedBy { get; set; }                      // 📌 کاربر ایجادکننده
        public DateTime? ModifiedAt { get; set; }                   // 📌 زمان آخرین تغییر
        public string? ModifiedBy { get; set; }                     // 📌 کاربر آخرین تغییر
        #endregion
        public string Title { get; private set; } = string.Empty;
        public string Code { get; private set; } = string.Empty;

        public Guid FkOrganizationUnitId { get; private set; }
        public Guid? ReportsToPositionId { get; private set; }

        public bool IsManagerial { get; private set; }

        // Navigation
        public virtual OrganizationUnit OrganizationUnit { get; private set; } = null!;
        public virtual Position? ReportsTo { get; private set; }
        public virtual ICollection<Assignment> Assignments { get; private set; } = new List<Assignment>();


        protected Position() { }

        public Position(string title, string code, Guid orgUnitId, string createdBy, bool isManagerial = false, Guid? reportsToPositionId = null)
        {
            Title = title;
            Code = code;
            FkOrganizationUnitId = orgUnitId;
            CreatedBy = createdBy;
            IsManagerial = isManagerial;
            ReportsToPositionId = reportsToPositionId;

        }
    }

}
