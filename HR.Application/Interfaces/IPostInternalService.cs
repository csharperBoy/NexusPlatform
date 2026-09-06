using Core.Domain.Common;
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
        Task<bool> AssignLocationsToPost(Guid postId, List<Guid> locationsId);

       
        Task<bool> AssignToEmploymentAsync(List<Guid?> postId, Guid employmentId, PostAssignmentType? assigneType = null,
            DateTime? EffectiveFrom = null, DateTime? EffectiveTo = null);
        Task<bool> AssignToPostAsync(Guid postId, List<Guid?> employmentIds, PostAssignmentType? assigneType = null,
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
        Task<(bool,string)> UpdatePostAsync(Guid id,
              Optional<string?> code,
            Optional<Guid?> organizationUnitId,
            Optional<Guid> jobTitleId,
            Optional<Guid?> jobLevelId,
            Optional<Guid?> gradeId,
            Optional<Guid?> costCenterId,
            Optional<Guid?> reportsToPostId,
            Optional<bool?> isActive,
            Optional<List<string>?> officePhone,
            Optional<List<string>?> orgEmail,
            Optional<List<string>?> orgMobile);
    }
}
