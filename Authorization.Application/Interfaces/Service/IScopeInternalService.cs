using Core.Application.Abstractions.Authorization.PublicService;
using Core.Shared.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Authorization.Application.Interfaces.Service
{
    public interface IScopeInternalService : IScopePublicService
    {
        Task UpdateScopeOfPermission(Guid permissionId, List<ScopeType>? scopes);
        Task AddScopesToPermission(Guid permissionId, List<ScopeType>? scopes);
    }
}
