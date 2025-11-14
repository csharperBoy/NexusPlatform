using Core.Domain.Common;
using Core.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Authorization.Domain.Entities
{
    public class Resource : AuditableEntity, IAggregateRoot
    {
        public Resource() { }

        public Resource(string code, string name, string type, Guid? parentId = null)
        {
            Code = code;
            Name = name;
            Type = type;
            ParentId = parentId;
        }

        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// A canonical unique code like: Identity.Users.Create
        /// </summary>
        public string Code { get; set; } = default!;

        public string Name { get; set; } = default!;

        /// <summary>
        /// Module | Page | Action | Table
        /// </summary>
        public string Type { get; set; } = default!;

        public string? Metadata { get; set; }

        public Guid? ParentId { get; set; }
        public Resource? Parent { get; set; }

        public ICollection<Resource> Children { get; set; } = new List<Resource>();
        public ICollection<Permission> Permissions { get; set; } = new List<Permission>();
    }
}
