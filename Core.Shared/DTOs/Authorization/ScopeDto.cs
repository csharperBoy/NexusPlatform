using Core.Shared.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Shared.DTOs.Authorization
{
    public class ScopeDto
    {
        public Guid PermissionId { get;  set; }
        public ScopeType scope { get; set; }

    }
}
