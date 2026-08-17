using HR.Application.DTOs;
using HR.Domain.Entities;
using HR.Domain.Enums;
using HR.Domain.Specifications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HR.Application.Interfaces
{
    public interface IPostInternalService
    {
        Task AssignLocationsToPost(Guid postId, List<Guid> locationsId);

       
        Task<Guid> AssignToEmploymentAsync(Guid postId, Guid employmentId, PostAssignmentType? assigneType = null, DateTime? EffectiveFrom = null, DateTime? EffectiveTo = null);
        Task<Guid> AssignToPostAsync(Guid postId, Guid employmentId, PostAssignmentType? assigneType = null, DateTime? EffectiveFrom = null, DateTime? EffectiveTo = null);
        Task<Guid> CreatePostAsync(string code, Guid organizationUnitId, Guid jobTitleId, Guid? jobLevelId = null, Guid? gradeId = null, Guid? costCenterId = null, Guid? reportsToPositionId = null, bool isActive = true
             

    , string? OfficePhone = null,
            string? OrgEmail = null,
            string? OrgMobile = null
            );
        Task<IEnumerable<CostCenter>> GetCostCenterListAsync();
        Task<List<Post>?> GetEmploymentPostAsync(Guid employmentId);
        Task<IEnumerable<Grade>> GetGradeListAsync();
        Task<IEnumerable<JobLevel>> GetJobLevelListAsync();
        Task<IEnumerable<JobTitle>> GetJobTitleListAsync();
        Task<IEnumerable<OrganizationUnit>> GetOrganizationUnitListAsync();
        Task<IReadOnlyList<PostInfoDto>> GetPostListAsync();
        Task SaveAsync();
        Task<Guid> UpdatePostAsync(Guid id, string? code, Guid? organizationUnitId, Guid? jobTitleId, Guid? jobLevelId, Guid? gradeId, Guid? costCenterId, Guid? reportsToPostId, bool? isActive, string? officePhone, string? orgEmail, string? orgMobile);
    }
}
