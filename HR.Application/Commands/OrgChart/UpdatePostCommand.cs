using Core.Shared.Enums.HR;
using Core.Shared.Results;
using HR.Application.Interfaces;
 
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HR.Application.Commands.OrgChart
{
    public record UpdatePostCommand(
        Guid Id,
  string? Code,
  Guid? OrganizationUnitId,
  Guid? JobTitleId,
  Guid? JobLevelId,
  Guid? GradeId,
  Guid? CostCenterId,
  Guid? ReportsToPostId,
  bool? IsActive,

  Guid? EmploymentId,
  PostAssignmentType? AssignType,

     List<Guid>? locationsId,
  List<string>? OfficePhone,
  List<string>? OrgEmail,
  List<string>? OrgMobile

) : IRequest<Result<Guid>>;


    public class UpdatePostCommandHandler : IRequestHandler<UpdatePostCommand, Result<Guid>>
    {
        private readonly IPostInternalService _orgChartService;
        private readonly ILogger<UpdatePostCommandHandler> _logger;

        public UpdatePostCommandHandler(
            IPostInternalService orgChartService,
            ILogger<UpdatePostCommandHandler> logger)
        {
            _orgChartService = orgChartService;
            _logger = logger;
        }

        public async Task<Result<Guid>> Handle(UpdatePostCommand request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation(
                    "Creating resource: {postCode}",
                    request.Code);

                Guid postId = await _orgChartService.UpdatePostAsync(
                    request.Id,
                       request.Code,
                       request.OrganizationUnitId,
                       request.JobTitleId,
                       request.JobLevelId,
                       request.GradeId,
                       request.CostCenterId,
                       request.ReportsToPostId,
                       request.IsActive, request.OfficePhone, request.OrgEmail, request.OrgMobile
                    );
                if (request.EmploymentId != Guid.Empty && request.EmploymentId != null)
                {
                    Guid assignId = await _orgChartService.AssignToEmploymentAsync(new List<Guid> { postId }, (Guid)request.EmploymentId, request.AssignType);
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
