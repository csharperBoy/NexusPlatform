using Core.Application.Abstractions;
using Core.Application.Abstractions.HR;
using Core.Shared.Enums.Authorization;
using HR.Application.Interfaces;
using HR.Domain.Entities;
using HR.Domain.Enums;
using HR.Domain.Specifications;
using HR.Infrastructure.Data;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
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
        private readonly IRepository<HRDbContext, Assignment, Guid> _assignmentRepository;
        private readonly ISpecificationRepository<Assignment, Guid> _assignmentSpecRepository;
        private readonly ILogger<OrgChartService> _logger;
        private readonly IUnitOfWork<HRDbContext> _uow;

        public OrgChartService(
            ISpecificationRepository<Post, Guid> postSpecRepository,
        IRepository<HRDbContext, Post, Guid> postRepository, ISpecificationRepository<Assignment, Guid> assignmentSpecRepository,
        IRepository<HRDbContext, Assignment, Guid> assignmentRepository,
        IUnitOfWork<HRDbContext> uow,
        ILogger<OrgChartService> logger)
        {
            _postRepository = postRepository;
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
                if(employeeId == null) {return null;}
                var post = await GetEmployeePostAsync((Guid)employeeId);
                return post.Select(p=>p.Id).ToList();
            }
            catch (Exception ex)
            {
                
                throw;
            }
        }

      
        public async Task<List<Guid>?> GetEmployeeOrganizeId(Guid? employeeId)
        {
            try
            {

                if (employeeId == null) { return null; }
                var posts = await GetEmployeePostAsync((Guid) employeeId);
                return posts.Select(p => p.OrganizationUnitId).ToList();
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public async Task<Guid> AssignToEmployeeAsync(Guid postId, Guid employeeId , PostAssignmentType? assigneType = null , DateOnly? EffectiveFrom = null , DateOnly? EffectiveTo  = null   )
        {
            Assignment assign=new Assignment(postId, employeeId,assigneType,EffectiveFrom,EffectiveTo);
            await _assignmentRepository.AddAsync(assign);
            return assign.Id;
        }

        public async Task<Guid> CreatePostAsync(string code, Guid organizationUnitId, Guid jobTitleId, Guid? jobLevelId = null, Guid? gradeId = null, Guid? costCenterId = null, Guid? reportsToPositionId = null, bool isActive = true)
        {
            Post post = new Post(code, organizationUnitId, jobTitleId, jobLevelId, gradeId,costCenterId, reportsToPositionId);
            await _postRepository.AddAsync(post);
            return post.Id;
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

       

        public async Task SaveAsync()
        {
            await _uow.SaveChangesAsync();
        }
    }
}
