using OrganizationManagement.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrganizationManagement.Application.Interfaces
{
    public interface IPositionInternalService
    {
        Task<List<Position>?> GetUserPositionAsync(Guid userId);
    }
}
