using Core.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Application.Abstractions.Authorization
{
    public class PermissionDefinition
    {
        public string ResourceKey { get; set; }
        public string Action { get; set; }
        public string Scope { get; set; } = "All";
        public string Type { get; set; } = "allow";
        public string Description { get; set; }

    }
}
