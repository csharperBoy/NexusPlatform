using OrganizationManagement.Domain.Entities;
using OrganizationManagement.Domain.Specifications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrganizationManagement.Application.Interfaces
{
    public interface IOrganizationService
    {
         Task<Position?> GetUserPositionAsync(Guid userId);

         Task<IReadOnlyList<Guid>> GetUserUnitsAsync(Guid userId);
         //Task<Person> GetUserPersonAsync(Guid userId);

         Task<IReadOnlyList<Guid>> GetAvailableUnitsAsync(Guid userId);

         Task<IReadOnlyList<Guid>> GetSubtreeUnitsAsync(Guid unitId, int depth);

         Task<string?> GetUnitPathAsync(Guid unitId);

         Task<bool> IsUnitInHierarchyAsync(Guid parentUnitId, Guid childUnitId);

         Task<OrganizationUnit?> GetUnitAsync(Guid unitId);

         Task<Position?> GetUnitManagerAsync(Guid unitId);

         Task<IReadOnlyList<Guid>> GetUnitUsersAsync(Guid unitId);

         Task<IReadOnlyList<Guid>> GetSiblingUnitsAsync(Guid unitId);

         Task<IReadOnlyList<Guid>> GetChildUnitsAsync(Guid unitId);

    }
}
