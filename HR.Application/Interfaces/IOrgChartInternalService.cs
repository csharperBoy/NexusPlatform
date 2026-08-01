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
        Task<Guid> AssignToEmployeeAsync(Guid postId, Guid employeeId, PostAssignmentType? assigneType = null, DateTime? EffectiveFrom = null, DateTime? EffectiveTo = null);
        Task<Guid> AssignToPostAsync(Guid postId, Guid employeeId, PostAssignmentType? assigneType = null, DateTime? EffectiveFrom = null, DateTime? EffectiveTo = null);
        Task<Guid> CreatePostAsync(string code, Guid organizationUnitId, Guid jobTitleId, Guid? jobLevelId = null, Guid? gradeId = null, Guid? costCenterId = null, Guid? reportsToPositionId = null, bool isActive = true
             

    , string? OfficePhone = null,
            string? OrgEmail = null,
            string? OrgMobile = null
            );
        Task<List<Post>?> GetEmployeePostAsync(Guid employeeId);
        Task<IReadOnlyList<PostInfoView>> GetPostListAsync();
        Task SaveAsync();
        Task<Guid> UpdatePostAsync(Guid id, string? code, Guid? organizationUnitId, Guid? jobTitleId, Guid? jobLevelId, Guid? gradeId, Guid? costCenterId, Guid? reportsToPostId, bool? isActive, string? officePhone, string? orgEmail, string? orgMobile);
    }
}
