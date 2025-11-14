using Core.Domain.Common;
using Core.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Authorization.Domain.Entities
{
    public class RolePermission : AuditableEntity, IEntity<Guid>
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// AspNetRoles.Id
        /// </summary>
        public Guid RoleId { get; set; }

        public Guid PermissionId { get; set; }
        public Permission Permission { get; set; } = default!;

        /// <summary>
        /// Data access scope: Own | Org | All | or JSON
        /// </summary>
        public string? Scope { get; set; }
    }
}
