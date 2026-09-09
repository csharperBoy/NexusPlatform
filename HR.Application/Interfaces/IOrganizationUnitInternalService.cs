using Core.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HR.Application.Interfaces
{
    public interface IOrganizationUnitInternalService
    {
        Task<Guid> CreateAsync(string code, string name, Guid? parentId, string createBy);
        Task DeleteAsync(Guid id);
        Task SaveAsync();
        Task<bool> UpdateAsync(Guid id, Optional<string?> name, Optional<string?> code, Optional<Guid?> parentId);
    }
}
