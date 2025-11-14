using Core.Domain.Common;
using Core.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Authorization.Domain.Entities
{
    public class UserPermission : AuditableEntity, IEntity<Guid>
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// AspNetUsers.Id
        /// </summary>
        public Guid UserId { get; set; }

        public Guid PermissionId { get; set; }
        public Permission Permission { get; set; } = default!;

        /// <summary>
        /// true = allow (granted), false = deny (override)
        /// </summary>
        public bool Granted { get; set; }

        /// <summary>
        /// Data access scope override
        /// </summary>
        public string? Scope { get; set; }
    }
}
