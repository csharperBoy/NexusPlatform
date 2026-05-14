using Core.Shared.Enums.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Authorization.Application.DTOs.Permissions
{
    public class PermissionRuleCreateDto
    {
        //public Guid Id { get; set; }
        //public Guid PermissionId { get; set; }


        public string? FieldName { get; set; }

        public string? JoinLocalKey { get; set; }
        public string? JoinForeignKey { get; set; }
        public string? JoinEntity { get; set; }

        public ComparisonOperator? Operator { get; set; }
        public string? Value { get; set; }

        public LogicalOperator? LogicalOperator { get; set; }
        public int? GroupOrder { get; set; }
    }
}
