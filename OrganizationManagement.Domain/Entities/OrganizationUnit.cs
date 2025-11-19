using Core.Domain.Common;
using Core.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrganizationManagement.Domain.Entities
{
    public class OrganizationUnit : AuditableEntity, IAggregateRoot//, IEntity<Guid>
    {

        public string Name { get; private set; } = string.Empty;
        public string Code { get; private set; } = string.Empty;

        public Guid? ParentId { get; private set; }
        public string? Description { get; private set; }

        // Navigation
        public virtual OrganizationUnit? Parent { get; private set; }
        public virtual ICollection<OrganizationUnit> Children { get; private set; } = new List<OrganizationUnit>();
        public virtual ICollection<Position> Positions { get; private set; } = new List<Position>();


        protected OrganizationUnit() { }

        public OrganizationUnit(string name, string code, string createdBy, string? description = null, Guid? parentId = null)
        {
            Name = name;
            Code = code;
            CreatedBy = createdBy;
            Description = description;
            ParentId = parentId;
        }
    }

}
