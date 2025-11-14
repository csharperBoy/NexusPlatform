using Core.Domain.Common;
using Core.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Authorization.Domain.Entities
{
    public class Permission : AuditableEntity, IAggregateRoot
    {
        public Permission() { }

        public Permission(string code, string name, Guid resourceId)
        {
            Code = code;
            Name = name;
            ResourceId = resourceId;
        }

        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// Canonical code e.g. Identity.Users.Create
        /// </summary>
        public string Code { get; set; } = default!;

        public string Name { get; set; } = default!;
        public string? Description { get; set; }

        public Guid ResourceId { get; set; }
        public Resource Resource { get; set; } = default!;

        public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
        public ICollection<UserPermission> UserPermissions { get; set; } = new List<UserPermission>();
    }
}
