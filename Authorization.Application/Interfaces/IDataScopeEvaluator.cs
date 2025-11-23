using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Authorization.Application.Interfaces
{
    public interface IDataScopeEvaluator
    {
        Task AddOverrideAsync(Guid userId, Guid permissionId, bool granted, string? scope, CancellationToken ct);
        Task RemoveOverrideAsync(Guid userId, Guid overrideId, CancellationToken ct);
    }
}
