using HR.Domain.Entities;
using HR.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HR.Application.Interfaces
{
    public interface IOrgChartInternalService
    {
        Task<Guid> AssignToEmployeeAsync(Guid postId, Guid employeeId, PostAssignmentType? assigneType = null, DateOnly? EffectiveFrom = null, DateOnly? EffectiveTo = null);

        Task<Guid> CreatePostAsync(string code, Guid organizationUnitId, Guid jobTitleId, Guid? jobLevelId = null, Guid? gradeId = null, Guid? costCenterId = null, Guid? reportsToPositionId = null, bool isActive = true);
        Task<List<Post>?> GetEmployeePostAsync(Guid employeeId);

        Task SaveAsync();
    }
}
