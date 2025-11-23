using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Authorization.Application.Interfaces
{
    public interface IPermissionEvaluator
    {
        Task UpdateAsync(Guid roleId, List<Guid> permissionIds, CancellationToken ct);
        Task<List<Guid>> GetIdsAsync(Guid roleId, CancellationToken ct);
    }
}
