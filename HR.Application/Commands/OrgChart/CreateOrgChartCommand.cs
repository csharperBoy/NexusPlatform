using Core.Application.Abstractions;
using Core.Shared.Enums.Authorization;
using Core.Shared.Results;
using HR.Application.Interfaces;
using HR.Domain.Entities;
using HR.Domain.Enums;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HR.Application.Commands.OrgChart
{
    public record CreateOrgChartCommand(
   string Code,
   Guid OrganizationUnitId,
   Guid JobTitleId,
   Guid? JobLevelId,
   Guid? GradeId,
   Guid? CostCenterId,
   Guid? ReportsToPositionId,
   bool IsActive,

   Guid? EmployeeId,
   PostAssignmentType? AssignType

) : IRequest<Result<Guid>>;


    public class CreateOrgChartCommandHandler : IRequestHandler<CreateOrgChartCommand, Result<Guid>>
    {
        private readonly IOrgChartInternalService _orgChartService;
        private readonly ILogger<CreateOrgChartCommandHandler> _logger;

        public CreateOrgChartCommandHandler(
            IOrgChartInternalService orgChartService,
            ILogger<CreateOrgChartCommandHandler> logger)
        {
            _orgChartService = orgChartService;
            _logger = logger;
        }

        public async Task<Result<Guid>> Handle(CreateOrgChartCommand request, CancellationToken cancellationToken)
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
                       request.ReportsToPositionId,
                       request.IsActive
                    );
                if (request.EmployeeId != Guid.Empty && request.EmployeeId != null)
                {
                    Guid assignId = await _orgChartService.AssignToEmployeeAsync(postId,(Guid) request.EmployeeId , request.AssignType);
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
