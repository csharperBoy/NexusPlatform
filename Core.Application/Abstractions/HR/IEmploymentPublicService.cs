using Core.Shared.DTOs.HR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Application.Abstractions.HR
{
    public interface IEmploymentPublicService
    {
        Task<IEnumerable<EmploymentFullDto>> GetFullInfoAsync();
        Task<Guid?> GetEmploymentId(Guid? personId);
        Task SaveAsync();
    }
}
