using Core.Domain.Common;
using Core.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrganizationManagement.Domain.Entities
{
    public class JobFamily : AuditableEntity, IAggregateRoot//, IEntity<Guid> 
    {

        public string Name { get; private set; } = string.Empty;
        public string? Description { get; private set; }

        public virtual ICollection<Position> Positions { get; private set; } = new List<Position>();


        protected JobFamily() { }

        public JobFamily(string name, string createdBy, string? description = null)
        {
            Name = name;
            CreatedBy = createdBy;
            Description = description;
        }
    }

}
