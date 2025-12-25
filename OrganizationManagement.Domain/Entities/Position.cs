using Core.Domain.Common;
using Core.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrganizationManagement.Domain.Entities
{
    public class Position : AuditableEntity, IAggregateRoot//, IEntity<Guid>
    {

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
