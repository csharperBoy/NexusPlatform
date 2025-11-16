using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Authorization.Application.Interfaces
{
    public interface IResourceService
    {
        Task<List<object>> GetResourceTreeAsync(CancellationToken ct);
    }
}
