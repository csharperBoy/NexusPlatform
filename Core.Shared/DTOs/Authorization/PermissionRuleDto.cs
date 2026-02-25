using Core.Shared.Enums.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Shared.DTOs.Authorization
{
    public class PermissionRuleDto
    {
        public Guid PermissionId { get; private set; }

        public RuleType Type { get; private set; }
      
        public string? FieldName { get; private set; }

        public string? JoinEntity { get; private set; }
        public string? JoinLocalKey { get; private set; }
        public string? JoinForeignKey { get; private set; }
        public string? JoinField { get; private set; }

        public ComparisonOperator Operator { get; private set; }
        public string? Value { get; private set; }

        public LogicalOperator LogicalOperator { get; private set; } 
        public int GroupOrder { get; private set; }
    }
}
