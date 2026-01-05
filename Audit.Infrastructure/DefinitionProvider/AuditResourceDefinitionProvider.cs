using Core.Application.Abstractions.Authorization;
using Core.Application.Abstractions.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Audit.Infrastructure.DefinitionProvider
{
   /* public class AuditResourceDefinitionProvider : IResourceDefinitionProvider
    {
        public string ModuleKey => "audit";
        public string ModuleName => "audit Management";

        public IEnumerable<ResourceDefinition> GetResourceDefinitions()
        {
            return new[]
            {
                new ResourceDefinition
                {
                    Key = "audit",
                    Name = "Audit",
                    Description = "audit management module",
                    Type = null,
                    Category = "System",
                    ParentKey = null,
                    DisplayOrder = 2000,
                    Icon = "shield",
                    Route = "/audit",
                    Permissions = new[] { "audit.access" }
                },

                 new ResourceDefinition
                {
                    Key = "Audit.AuditLog",
                    Name = "auditLog",
                    Description = "audit Log Table",
                    Type = "Data",
                    Category = "System",
                    ParentKey ="audit",
                    DisplayOrder = 1,
                    Icon = "shield",
                    Route = "/audit/auditLog",
                    Permissions = new[] { "auditLog.access" }
                },
            };
        }
    }*/
}
