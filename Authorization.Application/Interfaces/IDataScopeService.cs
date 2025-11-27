using Authorization.Application.Commands.DataScopes;
using Authorization.Application.DTOs.DataScopes;
using Core.Shared.Results;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Authorization.Application.Interfaces
{
    public interface IDataScopeService
    {
        // عملیات Write با منطق پیچیده
        Task<Guid> AssignDataScopeAsync(AssignDataScopeCommand command);
        Task UpdateDataScopeAsync(UpdateDataScopeCommand command);

        // عملیات Read
        Task<DataScopeDto> GetDataScopeAsync(Guid dataScopeId);
        Task<IReadOnlyList<DataScopeDto>> GetUserDataScopesAsync(Guid userId);

        // منطق کسب‌وکار پیچیده
        Task<string> BuildDataFilterAsync(Guid userId, string resourceKey);
        Task ValidateDataScopeHierarchyAsync(AssignDataScopeCommand command);
    }
}
