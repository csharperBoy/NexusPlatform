using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Application.Abstractions.Identity
{
    public interface IRolePublicService
    {
        Task<Guid> GetAdminRoleIdAsync(CancellationToken cancellationToken = default);
    }
}
