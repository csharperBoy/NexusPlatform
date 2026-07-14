using Core.Application.Abstractions;
using Core.Application.Abstractions.HR;
using Core.Shared.Enums.Authorization;
using Core.Shared.Enums.HR;
using Core.Shared.Enums.People;
using HR.Application.Interfaces;
using HR.Domain.Entities;
using HR.Domain.Enums;
using HR.Domain.Specifications;
using HR.Infrastructure.Data;
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
        private readonly ISpecificationRepository<Post, Guid> _postSpecRepository;
        private readonly IRepository<HRDbContext, Post, Guid> _postRepository;
        private readonly IRepository<HRDbContext, PostContact, Guid> _postContactRepository;
        private readonly IRepository<HRDbContext, Assignment, Guid> _assignmentRepository;
        private readonly ISpecificationRepository<Assignment, Guid> _assignmentSpecRepository;
        private readonly ILogger<OrgChartService> _logger;
        private readonly IUnitOfWork<HRDbContext> _uow;

        public OrgChartService(
            ISpecificationRepository<Post, Guid> postSpecRepository,
        IRepository<HRDbContext, Post, Guid> postRepository,
        IRepository<HRDbContext, PostContact, Guid> postContactRepository,
        ISpecificationRepository<Assignment, Guid> assignmentSpecRepository,
        IRepository<HRDbContext, Assignment, Guid> assignmentRepository,
        IUnitOfWork<HRDbContext> uow,
        ILogger<OrgChartService> logger)
        {
            _postRepository = postRepository;
            _postContactRepository = postContactRepository;
            _postSpecRepository = postSpecRepository;
            _logger = logger;
            _assignmentSpecRepository = assignmentSpecRepository;
            _assignmentRepository = assignmentRepository;
            _uow = uow;
        }

        public async Task<List<Guid>?> GetEmployeePostsId(Guid? employeeId)
        {
            try
            {
                if (employeeId == null) { return null; }
                var post = await GetEmployeePostAsync((Guid)employeeId);
                return post.Select(p => p.Id).ToList();
            }
            catch (Exception ex)
            {

                throw;
            }
        }


        public async Task<List<Guid?>?> GetEmployeeOrganizeId(Guid? employeeId)
        {
            try
            {

                if (employeeId == null) { return null; }
                var posts = await GetEmployeePostAsync((Guid)employeeId);
                return posts.Select(p => p.FkOrganizationUnitId).ToList();
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public async Task<Guid> AssignToEmployeeAsync(Guid postId, Guid employeeId, PostAssignmentType? assigneType = null, DateOnly? EffectiveFrom = null, DateOnly? EffectiveTo = null)
        {
            Assignment assign = new Assignment(postId, employeeId, assigneType, EffectiveFrom, EffectiveTo);
            var assignments = await GetPostAssignmentAsync(postId);
            if (assignments.Count > 0)
            {
                foreach (var item in assignments)
                {
                    await item.TerminationEmployeeOnPost();

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
                await CreatePostContact(PostContactType.OrgMobile, OrgMobile, post.Id);
            }
            if (OrgEmail != null)
            {
                await CreatePostContact(PostContactType.OrgEmail, OrgEmail, post.Id);
            }
            if (OfficePhone != null)
            {
                await CreatePostContact(PostContactType.OfficePhone, OfficePhone, post.Id);
            }
            return post.Id;
        }
        private async Task CreatePostContact(PostContactType type, string? value, Guid postId)
        {
            if (value != null)
            {
                PostContact contact = new PostContact(type, value, postId);
                await _postContactRepository.AddAsync(contact);
            }
        }
        public async Task<List<Post>?> GetEmployeePostAsync(Guid employeeId)
        {
            try
            {
                _logger.LogDebug("Getting post for employee {employeeId}", employeeId);

                // استفاده از Specification شیک
                var assignmentSpec = new ActiveAssignmentsByEmployeeSpec(employeeId);
                var assignments = await _assignmentSpecRepository.ListBySpecAsync(assignmentSpec);

                var assignment = assignments.ToList();
                if (assignment == null)
                {
                    _logger.LogWarning("No active assignment found for employee {employeeId}", employeeId);
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
                _logger.LogError(ex, "Error getting post for employee {employeeId}", employeeId);
                throw;
            }
        }
        public async Task<List<Employment>?> GetPostEmployeeAsync(Guid postId)
        {
            try
            {
                _logger.LogDebug("Getting employee for post {postId}", postId);

                // استفاده از Specification شیک
                var assignmentSpec = new ActiveAssignmentsByPostSpec(postId);
                var assignments = await _assignmentSpecRepository.ListBySpecAsync(assignmentSpec);

                var assignment = assignments.ToList();
                if (assignment == null)
                {
                    _logger.LogWarning("No active assignment found for postId {postId}", postId);
                    return null;
                }

                var employee = assignment.Select(a => a.Employment);
                if (employee == null)
                {
                    _logger.LogError("employee not found for assignment ");
                    return null;
                }

                return employee.ToList();

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting post for employee {postId}", postId);
                throw;
            }
        }
        public async Task<List<Assignment>?> GetPostAssignmentAsync(Guid postId)
        {
            try
            {
                _logger.LogDebug("Getting Assignment for post {postId}", postId);

                // استفاده از Specification شیک
                var assignmentSpec = new ActiveAssignmentsByPostSpec(postId);
                var assignments = await _assignmentSpecRepository.ListBySpecAsync(assignmentSpec);

                return assignments.ToList();

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting post for employee {postId}", postId);
                throw;
            }
        }

        public async Task SaveAsync()
        {
            await _uow.SaveChangesAsync();
        }

        public async Task<List<Guid>?> GetEmployeePostsPermissionAssigneeId(Guid? employeeId)
        {
            if (employeeId == null) { return null; }
            var post = await GetEmployeePostAsync((Guid)employeeId);
            return post.Select(p => p.FkPermissionAssigneeId).ToList();
        }
    }
}
