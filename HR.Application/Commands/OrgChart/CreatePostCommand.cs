using Core.Application.Abstractions;
using Core.Shared.Enums.Authorization;
using Core.Shared.Enums.HR;
using Core.Shared.Results;
using HR.Application.Interfaces;
using HR.Domain.Entities;
 
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace HR.Application.Commands.OrgChart
{
    public record CreatePostCommand(
    string Code,
    Guid OrganizationUnitId,
    Guid JobTitleId,
    Guid? JobLevelId,
    Guid? GradeId,
    Guid? CostCenterId,
    Guid? ReportsToPostId,
    bool IsActive,

    Guid? EmploymentId,
    PostAssignmentType? AssignType,

     List<Guid>? locationsId,
     List<string>? OfficePhone ,
            List<string>? OrgEmail ,
            List<string>? OrgMobile 

) : IRequest<Result<Guid>>;


    public class CreatePostCommandHandler : IRequestHandler<CreatePostCommand, Result<Guid>>
    {
        private readonly IPostInternalService _orgChartService;
        private readonly ILogger<CreatePostCommandHandler> _logger;

        public CreatePostCommandHandler(
            IPostInternalService orgChartService,
            ILogger<CreatePostCommandHandler> logger)
        {
            _orgChartService = orgChartService;
            _logger = logger;
        }

        public async Task<Result<Guid>> Handle(CreatePostCommand request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation(
                    "Creating resource: {postCode}",
                    request.Code);

                Guid postId = await _orgChartService.CreatePostAsync(
                       request.Code,
                       request.OrganizationUnitId,
                       request.JobTitleId,
                       request.JobLevelId,
                       request.GradeId,
                       request.CostCenterId,
                       request.ReportsToPostId,
                       request.IsActive , request.OfficePhone,request.OrgEmail , request.OrgMobile
                    );
                if (request.EmploymentId != Guid.Empty && request.EmploymentId != null)
                {
                    Guid assignId = await _orgChartService.AssignToEmploymentAsync(postId, (Guid)request.EmploymentId, request.AssignType);
                }
                if (request.locationsId != null)
                {
                    await _orgChartService.AssignLocationsToPost(postId, request.locationsId);
                }
                await _orgChartService.SaveAsync();
                _logger.LogInformation(
                    "Post created successfully: {postId} ({Code})",
                    postId, request.Code);

                return Result<Guid>.Ok(postId);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to create Post: {Code}",
                     request.Code);

                return Result<Guid>.Fail(ex.Message);
            }
        }
    }

}
