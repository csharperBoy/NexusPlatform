using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Authorization.Application.Security
{
    public record PermissionDescriptor(string Code, string Name, string ResourceCode, string? Scope = null);

}
