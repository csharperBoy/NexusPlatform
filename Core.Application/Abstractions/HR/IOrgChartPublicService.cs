using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Application.Abstractions.HR
{
    public interface IOrgChartPublicService
    {
        Task<List<Guid>?> GetEmployeeOrganizeId(Guid? employeeId);
        Task<List<Guid>?> GetEmployeePostsId(Guid? employeeId);
    }
}
