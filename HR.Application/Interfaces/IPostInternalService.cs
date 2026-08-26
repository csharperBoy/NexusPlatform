using Core.Shared.DTOs.HR;
using Core.Shared.Enums.HR;
using HR.Application.DTOs;
using HR.Domain.Entities;
 
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

       
        Task<Guid> AssignToEmploymentAsync(Guid postId, Guid employmentId, PostAssignmentType? assigneType = null,
            DateTime? EffectiveFrom = null, DateTime? EffectiveTo = null);
        Task<Guid> AssignToPostAsync(Guid postId, Guid employmentId, PostAssignmentType? assigneType = null,
            DateTime? EffectiveFrom = null, DateTime? EffectiveTo = null);
        Task<Guid> CreatePostAsync(string code, Guid organizationUnitId, Guid jobTitleId, Guid? jobLevelId = null,
            Guid? gradeId = null, Guid? costCenterId = null, Guid? reportsToPositionId = null, bool isActive = true
             

    , List<string>? OfficePhone = null,
            List<string>? OrgEmail = null,
            List<string>? OrgMobile = null
            );
        Task DeleteAsync(Guid id);
        Task<IEnumerable<CostCenter>> GetCostCenterListAsync();
        Task<List<Post>?> GetEmploymentPostAsync(Guid employmentId);
        Task<IEnumerable<Grade>> GetGradeListAsync();
        Task<IEnumerable<JobLevel>> GetJobLevelListAsync();
        Task<IEnumerable<JobTitle>> GetJobTitleListAsync();
        Task<IEnumerable<OrganizationUnit>> GetOrganizationUnitListAsync();
        Task<IReadOnlyList<PostInfoDto>> GetPostListAsync();
        Task SaveAsync();
        Task<Guid> UpdatePostAsync(Guid id, string? code, Guid? organizationUnitId, Guid? jobTitleId, Guid? jobLevelId, Guid? gradeId, Guid? costCenterId, Guid? reportsToPostId, bool? isActive, List<string>? officePhone, List<string>? orgEmail, List<string>? orgMobile);
    }
}
