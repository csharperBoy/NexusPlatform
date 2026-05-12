using Core.Application.Abstractions.Authorization.PublicService;
using Core.Shared.DTOs.Authorization;
using Core.Shared.Enums.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Authorization.Application.Interfaces.Service
{
    public interface IPermissionRuleInternalService : IPermissionRulePublicService
    {
        Task<Guid> CreateRuleAsync(Guid permissionId, string fieldName, ComparisonOperator comparisonOperator, string? value, LogicalOperator logicalOperator, int groupOrder);
        Task DeletePermissionRuleAsync(Guid id);
        Task<PermissionRuleDto?> GetById(Guid id);
        Task<IReadOnlyList<PermissionRuleDto>> GetPermissionRules(Guid? permissionId);
        Task UpdatePermissionRuleAsync(Guid id, Guid? permissionId, string? fieldName, ComparisonOperator? comparisonOperator, string? value, LogicalOperator? logicalOperator, int? groupOrder);
    }
}
