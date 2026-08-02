using Core.Application.Abstractions;
using Core.Application.Abstractions.HR;
using Core.Domain.Common.EntityProperties;
using Core.Shared.Enums.Authorization;
using Core.Shared.Enums.HR;
using Core.Shared.Enums.People;
using HR.Application.Interfaces;
using HR.Domain.Entities;
using HR.Domain.Enums;
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
    public class OrgChartService :
        IOrgChartInternalService,
        IOrgChartPublicService
    {
        private readonly ISpecificationRepository<PostContact, Guid> _postContactSpecRepository;
        private readonly ISpecificationRepository<PostInfoView, Guid> _postInfoViewSpecRepository;
        private readonly ISpecificationRepository<Post, Guid> _postSpecRepository;
        private readonly IRepository<HRDbContext, Post, Guid> _postRepository;
        private readonly IRepository<HRDbContext, PostContact, Guid> _postContactRepository;
        private readonly IRepository<HRDbContext, Assignment, Guid> _assignmentRepository;
        private readonly ISpecificationRepository<Assignment, Guid> _assignmentSpecRepository;
        private readonly ILogger<OrgChartService> _logger;
        private readonly IUnitOfWork<HRDbContext> _uow;
        private readonly IHRUnitOfWork<HRDbContext> _hrUow;


        //private readonly HRDbContext _contex;
        public OrgChartService(
            //HRDbContext contex,
            ISpecificationRepository<PostInfoView, Guid> postInfoViewSpecRepository,
            ISpecificationRepository<Post, Guid> postSpecRepository,
            ISpecificationRepository<PostContact, Guid> postContactSpecRepository,
        IRepository<HRDbContext, Post, Guid> postRepository,
        IRepository<HRDbContext, PostContact, Guid> postContactRepository,
        ISpecificationRepository<Assignment, Guid> assignmentSpecRepository,
        IRepository<HRDbContext, Assignment, Guid> assignmentRepository,
        IUnitOfWork<HRDbContext> uow, IHRUnitOfWork<HRDbContext> hrUow,
        ILogger<OrgChartService> logger)
        {
            //_contex= contex;
            _postInfoViewSpecRepository = postInfoViewSpecRepository;
            _hrUow = hrUow; 
            _postRepository = postRepository;
            _postContactRepository = postContactRepository;
            _postSpecRepository = postSpecRepository;
            _postContactSpecRepository = postContactSpecRepository;
            _logger = logger;
            _assignmentSpecRepository = assignmentSpecRepository;
            _assignmentRepository = assignmentRepository;
            _uow = uow;
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
            if (assignments.Count > 0)
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
            if (assignments.Count > 0)
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
            , string? OfficePhone = null,
            string? OrgEmail = null,
            string? OrgMobile = null
            )
        {
            Post post = new Post(code, organizationUnitId, jobTitleId, jobLevelId, gradeId, costCenterId, reportsToPostId);
            await _postRepository.AddAsync(post);
            if (OrgMobile != null)
            {
                await CreatePostContact(HrContactType.OrgMobile, OrgMobile, post.Id);
            }
            if (OrgEmail != null)
            {
                await CreatePostContact(HrContactType.OrgEmail, OrgEmail, post.Id);
            }
            if (OfficePhone != null)
            {
                await CreatePostContact(HrContactType.OfficePhone, OfficePhone, post.Id);
            }
            return post.Id;
        }
        private async Task CreatePostContact(HrContactType type, string? value, Guid postId)
        {
            if (value != null)
            {
                GetPostContactSpec spec = new GetPostContactSpec(type, postId, value);
                PostContact? existContact = await _postContactSpecRepository.GetBySpecAsync(spec);
                if (existContact?.Value.Trim() != value.Trim())
                {
                    if (existContact != null)
                    {
                        existContact.DoExpire();
                        await _postContactRepository.UpdateAsync(existContact);

                    }
                    PostContact contact = new PostContact(type, value, postId, DateTime.UtcNow);
                    await _postContactRepository.AddAsync(contact);
                }

            }
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
        public async Task<List<Assignment>?> GetPostAssignmentAsync(Guid postId )
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
        }

        public async Task<List<Guid>?> GetEmploymentPostsPermissionAssigneeId(Guid? employmentId)
        {
            if (employmentId == null) { return null; }
            var post = await GetEmploymentPostAsync((Guid)employmentId);
            return post.Select(p => p.FkPermissionAssigneeId).ToList();
        }

        public async Task<Guid> UpdatePostAsync(
            Guid id, string? code, Guid? organizationUnitId, Guid? jobTitleId, Guid? jobLevelId, Guid? gradeId, Guid? costCenterId, Guid? reportsToPostId, bool? isActive,
            string? officePhone, string? orgEmail, string? orgMobile)
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
                await CreatePostContact(HrContactType.OfficePhone, officePhone, post.Id);
            }
            if (orgEmail != null)
            {
                await CreatePostContact(HrContactType.OrgEmail, orgEmail, post.Id);
            }
            if (orgMobile != null)
            {
                await CreatePostContact(HrContactType.OrgMobile, orgMobile, post.Id);
            }
            return post.Id;
        }

        public async Task<IReadOnlyList<PostInfoView>> GetPostListAsync()
        {
            //var list =await _hrUow.PostInfoViewRepository.GetAllAsync();
            var list = await  _postInfoViewSpecRepository.ListBySpecAsync(new GetAllPostInfoViewSpec());
            var result = list.ToList();
            return result;
        }
    }
}
