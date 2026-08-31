using Core.Domain.ValueObjects;
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
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace HR.Application.Commands.Employment
{

    public record UpdateEmploymentCommand(
           Guid Id,
    #region party
        List<string>? Phone,
        List<string>? Address,
        List<string>? Email,
        List<string>? Mobile,
    #endregion
    #region Person

     string? FirstName,
     string? LastName,
     DateTime? BirthDate,
     string? BirthPlace,
     string? FatherName,
     string? nationalCode,
    #endregion
    #region employment

     string? EmploymentCode,
     Guid? EmploymentTypeId,
     Guid? EmploymentStatusId,
     DateOnly? StartDate,
     DateOnly? EndDate,

     List<Guid>? locationsId,

     List<string>? OfficePhone,
            List<string>? OrgEmail,
            List<string>? OrgMobile,
    #endregion

    #region post assign

     Guid PostId,
     PostAssignmentType AssigneeType,
     DateTime? EffectiveFrom,
     DateTime? EffectiveTo
    #endregion

) : IRequest<Result<Guid>>;


    public class UpdateEmploymentCommandHandler : IRequestHandler<UpdateEmploymentCommand, Result<Guid>>
    {
        private readonly IEmploymentInternalService _employmentService;
        private readonly IPostInternalService _orgChartService;
        private readonly ILogger<UpdateEmploymentCommandHandler> _logger;

        public UpdateEmploymentCommandHandler(
            IEmploymentInternalService employmentService,
            ILogger<UpdateEmploymentCommandHandler> logger,
            IPostInternalService orgChartService)
        {
            _employmentService = employmentService;
            _logger = logger;
            _orgChartService = orgChartService;
        }

        public async Task<Result<Guid>> Handle(UpdateEmploymentCommand request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation(
                    "Creating resource: {EmploymentCode}",
                    request.EmploymentCode);

                Guid EmploymentId = await _employmentService.UpdateEmploymentAsync(
                    request.Id,
                    request.Phone,
                    request.Address,
                    request.Email,
                    request.Mobile,
                    request.FirstName,
                    request.LastName,
                    request.BirthDate,
                     request.BirthPlace,
                     request.FatherName,
                     request.nationalCode,
                     request.EmploymentCode,
                    request.EmploymentTypeId,
                    request.EmploymentStatusId,
                    request.StartDate,
                    request.EndDate,
                    request.locationsId,
                    request.OfficePhone,
                    request.OrgEmail,
                    request.OrgMobile
                    );
                if (request.PostId != Guid.Empty && request.PostId != null)
                {
                    Guid assignId = await _orgChartService.AssignToEmploymentAsync(new List<Guid> { request.PostId }, EmploymentId, request.AssigneeType, request.EffectiveFrom, request.EffectiveTo);
                }
                if (request.locationsId != null)
                {
                    await _employmentService.AssignLocationsToEmployment(EmploymentId, request.locationsId);
                }
                await _employmentService.SaveAsync();
                _logger.LogInformation(
                    "Employment created successfully: {EmploymentId} ({Code})",
                    EmploymentId, request.EmploymentCode);

                return Result<Guid>.Ok(EmploymentId);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to create Employment: {Code}",
                     request.EmploymentCode);

                return Result<Guid>.Fail(ex.Message);
            }
        }
    }


}
