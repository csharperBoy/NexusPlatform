using Core.Application.Abstractions;
using Core.Application.Abstractions.Contact;
using Core.Application.Abstractions.HR;
using Core.Domain.Common.EntityProperties;
using Core.Shared.DTOs.HR;
using Core.Shared.Enums;
using Core.Shared.Enums.Authorization;
using Core.Shared.Enums.Contact;
using Core.Shared.Enums.HR;

using HR.Application.DTOs;
using HR.Application.Interfaces;
using HR.Domain.Entities;
 
using HR.Domain.Events.Employment;
using HR.Domain.Events.Post;
using HR.Domain.Specifications;
using HR.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
namespace HR.Infrastructure.Services
{
    public class PostService :
        IPostInternalService,
        IPostPublicService
    {
        //private readonly ISpecificationRepository<PostContact, Guid> _postContactSpecRepository;
        private readonly ISpecificationRepository<PostInfoView, Guid> _postInfoViewSpecRepository;
        private readonly ISpecificationRepository<Post, Guid> _postSpecRepository;
        private readonly IRepository<HRDbContext, Post, Guid> _postRepository;
        //private readonly IRepository<HRDbContext, PostContact, Guid> _postContactRepository;
        private readonly IRepository<HRDbContext, Assignment, Guid> _assignmentRepository;
        private readonly ISpecificationRepository<Assignment, Guid> _assignmentSpecRepository;

        private readonly IRepository<HRDbContext, PostLocation, Guid> _postLocationsRepository;
        private readonly ISpecificationRepository<PostLocation, Guid> _postLocationSpecRepository;

        private readonly IContactPublicService _contactService;
        private readonly ILogger<PostService> _logger;
        private readonly IUnitOfWork<HRDbContext> _uow;
        private readonly IHRUnitOfWork<HRDbContext> _hrUow;


        //private readonly HRDbContext _contex;
        public PostService(
             //HRDbContext contex,
             IRepository<HRDbContext, PostLocation, Guid> postLocationsRepository,
             ISpecificationRepository<PostLocation, Guid> postLocationSpecRepository,
            ISpecificationRepository<PostInfoView, Guid> postInfoViewSpecRepository,
            ISpecificationRepository<Post, Guid> postSpecRepository,
        //ISpecificationRepository<PostContact, Guid> postContactSpecRepository,
        IRepository<HRDbContext, Post, Guid> postRepository,
        //IRepository<HRDbContext, PostContact, Guid> postContactRepository,
        ISpecificationRepository<Assignment, Guid> assignmentSpecRepository,
        IRepository<HRDbContext, Assignment, Guid> assignmentRepository,
        IContactPublicService contactService,
        IUnitOfWork<HRDbContext> uow, IHRUnitOfWork<HRDbContext> hrUow,
        ILogger<PostService> logger)
        {
            //_contex= contex;
            _postInfoViewSpecRepository = postInfoViewSpecRepository;
            _hrUow = hrUow;
            _postRepository = postRepository;
            //_postContactRepository = postContactRepository;
            _postSpecRepository = postSpecRepository;
            //_postContactSpecRepository = postContactSpecRepository;
            _logger = logger;
            _assignmentSpecRepository = assignmentSpecRepository;
            _assignmentRepository = assignmentRepository;
            _contactService = contactService;
            _uow = uow;
            _postLocationSpecRepository = postLocationSpecRepository;
            _postLocationsRepository = postLocationsRepository;
        }
        public async Task AssignLocationsToPost(Guid postId, List<Guid> locationsId)
        {
            // ۱. دریافت مکان‌های فعال فعلی کارمند (فرض بر این است که اسپک فقط Activeها را برمی‌گرداند)
            var spec = new GetPostLocationsSpec(postId);
            var existingActive = await _postLocationSpecRepository.ListBySpecAsync(spec);

            // ۲. مجموعه‌های شناسه‌ها برای مقایسه (حذف تکراری‌های ورودی)
            var existingIds = existingActive.Select(e => e.FkLocationId).ToHashSet();
            var newIds = locationsId.Distinct().ToHashSet();

            // ۳. مکان‌هایی که باید منقضی شوند (موجود اما در لیست جدید نیستند)
            var toExpire = existingActive.Where(e => !newIds.Contains(e.FkLocationId)).ToList();
            foreach (var item in toExpire)
            {
                item.DoExpire();
            }

            // ۴. مکان‌هایی که باید اضافه شوند (در لیست جدید هستند اما قبلاً وجود نداشتند)
            var toAdd = newIds
                .Where(id => !existingIds.Contains(id))
                .Select(id => new PostLocation(id, postId))
                .ToList();

            if (toAdd.Any())
            {
                await _postLocationsRepository.AddRangeAsync(toAdd);
            }

        }
        public async Task<List<Guid>?> GetEmploymentPostsId(Guid? employmentId)
        {
            try
            {
                if (employmentId == null) { return null; }
                var post = await GetEmploymentPostAsync((Guid)employmentId);
                return post.Select(p => p.Id).ToList();
            }
            catch (Exception ex)
            {

                throw;
            }
        }


        public async Task<List<Guid?>?> GetEmploymentOrganizeId(Guid? employmentId)
        {
            try
            {

                if (employmentId == null) { return null; }
                var posts = await GetEmploymentPostAsync((Guid)employmentId);
                return posts.Select(p => p.FkOrganizationUnitId).ToList();
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public async Task<Guid> AssignToEmploymentAsync(Guid postId, Guid employmentId, PostAssignmentType? assigneType = null, DateTime? EffectiveFrom = null, DateTime? EffectiveTo = null)
        {
            Assignment assign = new Assignment(postId, employmentId, assigneType, EffectiveFrom, EffectiveTo);
            var assignments = await GetPostAssignmentAsync(postId);
            if (assignments?.Count > 0)
            {
                foreach (var item in assignments)
                {
                    item.DoExpire();
                    await _assignmentRepository.UpdateAsync(item);

                }
            }
            await _assignmentRepository.AddAsync(assign);
            return assign.Id;
        }
        public async Task<Guid> AssignToPostAsync(Guid postId, Guid employmentId, PostAssignmentType? assigneType = null, DateTime? EffectiveFrom = null, DateTime? EffectiveTo = null)
        {
            Assignment assign = new Assignment(postId, employmentId, assigneType, EffectiveFrom, EffectiveTo);
            var assignments = await GetEmploymentAssignmentAsync(employmentId);
            if (assignments?.Count > 0)
            {
                foreach (var item in assignments)
                {
                    item.DoExpire();
                    await _assignmentRepository.UpdateAsync(item);

                }
            }
            await _assignmentRepository.AddAsync(assign);
            return assign.Id;
        }


        public async Task<Guid> CreatePostAsync(string code, Guid organizationUnitId, Guid jobTitleId, Guid? jobLevelId = null, Guid? gradeId = null, Guid? costCenterId = null, Guid? reportsToPostId = null, bool isActive = true
            , List<string>? OfficePhone = null,
            List<string>? OrgEmail = null,
            List<string>? OrgMobile = null
            )
        {

            Guid contactProfileId = await _contactService.CreateContactProfileAsync($"Post - {code}", ContactProfileTypeEnum.Post);
            Post post = new Post(code, jobTitleId, contactProfileId, organizationUnitId, jobLevelId, gradeId, costCenterId, reportsToPostId);
            await _postRepository.AddAsync(post);
            await _contactService.SyncProfileContacts(ContactTypeEnum.OrganizationMobile, OrgMobile, post.FkContactProfileId);
            await _contactService.SyncProfileContacts(ContactTypeEnum.Email, OrgEmail, post.FkContactProfileId);
            await _contactService.SyncProfileContacts(ContactTypeEnum.OfficePhone, OfficePhone, post.FkContactProfileId);
            return post.Id;
        }

        public async Task<List<Post>?> GetEmploymentPostAsync(Guid employmentId)
        {
            try
            {
                _logger.LogDebug("Getting post for employment {employmentId}", employmentId);

                // استفاده از Specification شیک
                var assignmentSpec = new ActiveAssignmentsByEmploymentSpec(employmentId);
                var assignments = await _assignmentSpecRepository.ListBySpecAsync(assignmentSpec);

                var assignment = assignments.ToList();
                if (assignment == null)
                {
                    _logger.LogWarning("No active assignment found for employment {employmentId}", employmentId);
                    return null;
                }

                var post = assignment.Select(a => a.Post);
                if (post == null)
                {
                    _logger.LogError("post not found for assignment ");
                    return null;
                }

                return post.ToList();

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting post for employment {employmentId}", employmentId);
                throw;
            }
        }
        public async Task<List<Assignment>?> GetEmploymentAssignmentAsync(Guid employmentId)
        {
            try
            {
                _logger.LogDebug("Getting employment for employmentId {employmentId}", employmentId);

                // استفاده از Specification شیک
                var assignmentSpec = new ActiveAssignmentsByEmploymentSpec(employmentId);
                var assignments = await _assignmentSpecRepository.ListBySpecAsync(assignmentSpec);

                return assignments.ToList();


            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting post for employment {employmentId}", employmentId);
                throw;
            }
        }
        public async Task<List<Assignment>?> GetPostAssignmentAsync(Guid postId)
        {
            try
            {
                _logger.LogDebug("Getting Assignment for post {postId}", postId);

                var assignmentSpec = new ActiveAssignmentsByPostSpec(postId);
                var assignments = await _assignmentSpecRepository.ListBySpecAsync(assignmentSpec);

                return assignments.ToList();

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting post for employment {postId}", postId);
                throw;
            }
        }

        public async Task SaveAsync()
        {
            await _uow.SaveChangesAsync();
            await _contactService.SaveAsync();
        }

        public async Task<List<Guid>?> GetEmploymentPostsPermissionAssigneeId(Guid? employmentId)
        {
            if (employmentId == null) { return null; }
            var post = await GetEmploymentPostAsync((Guid)employmentId);
            return post.Select(p => p.FkPermissionAssigneeId).ToList();
        }

        public async Task<Guid> UpdatePostAsync(
            Guid id, string? code, Guid? organizationUnitId, Guid? jobTitleId, Guid? jobLevelId, Guid? gradeId, Guid? costCenterId, Guid? reportsToPostId, bool? isActive,
            List<string>? officePhone, List<string>? orgEmail, List<string>? orgMobile)
        {
            Post? post = await _postRepository.GetByIdAsync(id);
            if (post == null)
                throw new Exception("can not found post!!!");

            bool hasChange = post.ApplyChange(code, jobTitleId, organizationUnitId, jobLevelId, gradeId, costCenterId, isActive, reportsToPostId);
            if (hasChange)
            {
                await _postRepository.UpdateAsync(post);
            }
            if (officePhone != null)
            {
                await _contactService.SyncProfileContacts(ContactTypeEnum.OfficePhone, officePhone, post.FkContactProfileId);
            }
            if (orgEmail != null)
            {
                await _contactService.SyncProfileContacts(ContactTypeEnum.Email, orgEmail, post.FkContactProfileId);
            }
            if (orgMobile != null)
            {
                await _contactService.SyncProfileContacts(ContactTypeEnum.OrganizationMobile, orgMobile, post.FkContactProfileId);
            }
            return post.Id;
        }
        public async Task<IReadOnlyList<PostInfoDto>> GetPostListAsync()
        {
            //var list =await _hrUow.PostInfoViewRepository.GetAllAsync();
            var postList = await _postInfoViewSpecRepository.ListBySpecAsync(new GetAllPostInfoViewSpec());
            var postIds = postList.Select(p => p.Id).ToList();

            var locList = await _postLocationsRepository.GetAllAsync(q =>
                q.Where(a => postIds.Contains(a.FkPostId) && a.IsCurrent)
                 .Include(a => a.Location)
            );


            var result = postList.Select(s => new PostInfoDto
            {
                EmploymentCode = s.EmploymentCode,
                ProfileId = s.FkContactProfileId,
                EmploymentId = s.EmploymentId,
                FirstName = s.FirstName,
                LastName = s.LastName,
                FkCostCenterId = s.FkCostCenterId,
                CostCenterName = s.CostCenterName,
                Id = s.Id,
                FkGradeId = s.FkGradeId,
                GradeTitle = s.GradeTitle,
                FkJobLevelId = s.FkJobLevelId,
                JobLevelTitle = s.JobLevelTitle,
                FkJobTitleId = s.FkJobTitleId,
                JobTitleName = s.JobTitleName,
                AssigneeType = s.AssignmentsAssigneeType?.ToString().ToEnumOrDefault<PostAssignmentType>(PostAssignmentType.Permanent),
                FkOrganizationUnitId = s.FkOrganizationUnitId,
                OrganizationUnitsName = s.OrganizationUnitsName,
                FkParentId = s.FkParentId,
                Gender = s.Gender,
                PostCode = s.PostCode,
                locations = locList.Where(l => l.FkPostId == s.Id).Select(s => new LocationInfoDto { Id = s.Location.Id, Title = s.Location.Title }).ToList(),


            }).ToList();
            return result;
        }

        public async Task<IEnumerable<CostCenter>> GetCostCenterListAsync()
        {
            var list = await _hrUow.CostCenterRepository.GetAllAsync();
            return list;
        }

        public async Task<IEnumerable<Grade>> GetGradeListAsync()
        {
            var list = await _hrUow.GradeRepository.GetAllAsync();
            return list;
        }

        public async Task<IEnumerable<JobLevel>> GetJobLevelListAsync()
        {
            var list = await _hrUow.JobLevelRepository.GetAllAsync();
            return list;
        }

        public async Task<IEnumerable<JobTitle>> GetJobTitleListAsync()
        {
            var list = await _hrUow.JobTitleRepository.GetAllAsync();
            return list;
        }

        public async Task<IEnumerable<OrganizationUnit>> GetOrganizationUnitListAsync()
        {
            var list = await _hrUow.OrganizationUnitRepository.GetAllAsync();
            return list;
        }

        public async Task DeleteAsync(Guid id)
        {
            Post? model = await _postRepository.GetByIdAsync(id);
            if (model == null)
                throw new Exception("can not found post!!!");

            await model.SoftRemove();
            model.AddDomainEvent(new RemovePostEvent(model.Id, model.Code,model.IsActive,model.FkPermissionAssigneeId, model.FkContactProfileId));

            await ExpirePostLocationsAsync(id);


            await ExpirePostEmploymentsAsync(id);

        }

        private async Task ExpirePostLocationsAsync(Guid id)
        {
            var locList = await _postLocationsRepository.GetAllAsync(queryOptions: q => q.Where(a => a.FkPostId == id && a.IsCurrent));
            foreach (var item in locList)
            {
                item.DoExpire();
            }
        }
        private async Task ExpirePostEmploymentsAsync(Guid id)
        {
            var empList = await _assignmentRepository.GetAllAsync(queryOptions: q => q.Where(a => a.FkPostId == id && a.IsCurrent));
            foreach (var item in empList)
            {
                item.DoExpire();
            }
        }
    }
}
