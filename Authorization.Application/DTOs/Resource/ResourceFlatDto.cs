using Authorization.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Authorization.Application.DTOs.Resource
{
    /*
      📌 ResourceFlatDto
      -------------------
      نسخه سبک Resource برای مواردی مثل:
      - PermissionEvaluator
      - DataScopeEvaluator
      - ResourceTreeBuilder

      شامل ParentId جهت ساختن درخت.
     */

    public class ResourceFlatDto
    {
        public Guid Id { get; init; }
        public string Key { get; init; } = string.Empty;
        public string Name { get; init; } = string.Empty;

        public Guid? ParentId { get; init; }

        public ResourceType Type { get; init; }
        public ResourceCategory Category { get; init; }
    }
}
