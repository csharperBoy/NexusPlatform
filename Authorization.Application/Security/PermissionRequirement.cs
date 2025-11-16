using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Authorization.Application.Security
{
    public class PermissionRequirement : IAuthorizationRequirement
    {
        public string PermissionCode { get; }

        public PermissionRequirement(string code)
        {
            PermissionCode = code;
        }
    }
}
