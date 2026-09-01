using Core.Shared.Enums.HR;
using Core.Shared.Results;
using HR.Application.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using Core.Domain.Common;
namespace HR.Application.Commands.Employment
{

    public record UpdateEmploymentCommand(
           Guid Id,
    #region party
     Optional<List<string>?> Phone = default,
     Optional<List<string>?> Address = default,
     Optional<List<string>?> Email = default,
     Optional<List<string>?> Mobile = default,
    #endregion
    #region Person

     Optional<string?> FirstName = default,
     Optional<string?> LastName = default,
     Optional<DateTime?> BirthDate = default,
     Optional<string?> BirthPlace = default,
     Optional<string?> FatherName = default,
     Optional<string?> nationalCode = default,
    #endregion
    #region employment

     Optional<string?> EmploymentCode = default,
     Optional<Guid?> EmploymentTypeId = default,
     Optional<Guid?> EmploymentStatusId = default,
     Optional<DateOnly?> StartDate = default,
     Optional<DateOnly?> EndDate = default,

     Optional<List<Guid>?> locationsId = default,

     Optional<List<string>?> OfficePhone = default,
     Optional<List<string>?> OrgEmail = default,
     Optional<List<string>?> OrgMobile = default,
    #endregion

    #region post assign

     Optional<Guid?> PostId = default,
     Optional<PostAssignmentType?> AssigneeType = default,
     Optional<DateTime?> EffectiveFrom = default,
     Optional<DateTime?> EffectiveTo = default
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
                if (request.PostId.IsSet)
                {
                    Guid assignId = await _orgChartService.AssignToEmploymentAsync(new List<Guid?> { request.PostId.Value }, EmploymentId, request.AssigneeType.Value, request.EffectiveFrom.Value, request.EffectiveTo.Value);
                }
                if (request.locationsId.IsSet)
                {
                    await _employmentService.AssignLocationsToEmployment(EmploymentId, request.locationsId.Value);
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
