using Core.Application.Abstractions.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Authorization.Infrastructure.Services
{
    public class AuthorizationResourceDefinitionProvider : IResourceDefinitionProvider
    {
        public string ModuleKey => "authorization";
        public string ModuleName => "Authorization Management";

        public IEnumerable<ResourceDefinition> GetResourceDefinitions()
        {
            return new[]
            {
                new ResourceDefinition
                {
                    Key = "authorization",
                    Name = "Authorization",
                    Description = "Authorization management module",
                    Type = "Ui",
                    Category = "System",
                    ParentKey = null,
                    DisplayOrder = 1000,
                    Icon = "shield",
                    Route = "/authorization",
                    Permissions = new[] { "authorization.access" }
                },
            };
        }
    }
}
