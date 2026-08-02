using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Application.Abstractions.HR
{
    public interface IOrgChartPublicService
    {
        Task<List<Guid?>?> GetEmploymentOrganizeId(Guid? employmentId);
        Task<List<Guid>?> GetEmploymentPostsId(Guid? employmentId);
        Task<List<Guid>?> GetEmploymentPostsPermissionAssigneeId(Guid? employmentId);
    }
}
