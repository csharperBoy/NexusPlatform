using Core.Application.Abstractions.People;
using Core.Application.Context;
using Core.Application.Provider;
using Core.Domain.ValueObjects;
using Core.Shared.Enums.HR;
using Core.Shared.Results;
using HR.Application.Interfaces;
using HR.Domain.Enums;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HR.Application.Commands.Employment
{
    public record CreateEmploymentCommand(
    #region party
        string? Phone,
        string? Address,
        string? Email,
        string? Mobile,
    #endregion
    #region Person

     string NationalCode,
     string FirstlName,
     string LastName,
     DateTime? BirthDate,
     string? BirthPlace,
     string? FatherName,
     Gender? Gender,
    #endregion
    #region employment

     string EmploymentCode,
     Guid? EmploymentTypeId,
     Guid? EmploymentStatusId,
     DateOnly? StartDate,
     DateOnly? EndDate,

     List<Guid> locationsId,

     string? OfficePhone,
            string? OrgEmail,
            string? OrgMobile,
    #endregion

    #region post assign

     Guid PostId,
     PostAssignmentType AssigneeType,
     DateTime? EffectiveFrom,
     DateTime? EffectiveTo
    #endregion

) : IRequest<Result<Guid>>;


    public class CreateEmploymentCommandHandler : IRequestHandler<CreateEmploymentCommand, Result<Guid>>
    {
        private readonly IOrgChartInternalService _orgChartService;
        private readonly IPersonPublicService _personService;
        private readonly IEmploymentInternalService _employmentService;
        private readonly ILogger<CreateEmploymentCommandHandler> _logger;
        private readonly IUserDataContextProvider _userProvider;
        public CreateEmploymentCommandHandler(
            IOrgChartInternalService orgChartService,
            IPersonPublicService personService,
            IEmploymentInternalService employmentService,
           IUserDataContextProvider userProvider,
        ILogger<CreateEmploymentCommandHandler> logger)
        {
            _orgChartService = orgChartService;
            _logger = logger;
            _personService = personService;
            _employmentService = employmentService;
            _userProvider = userProvider;
        }

        public async Task<Result<Guid>> Handle(CreateEmploymentCommand request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation(
                    "Creating employmentCode: {EmploymentCode}",
                    request.EmploymentCode);
                UserDataContext userContext = await _userProvider.GetAsync(new CancellationToken());
                #region ساخت شخصیت حقیقی

                PhoneNumber? phone = null;
                Email? email = null;
                PhoneNumber? mobile = null;
                try { phone = request.Phone != null ? PhoneNumber.Create(request.Phone) : null; } catch { }
                try { email = request.Email != null ? Email.Create(request.Email) : null; } catch { }
                try { mobile = request.Mobile != null ? PhoneNumber.Create(request.Mobile) : null; } catch { }


                Guid personId = await _personService.CreatePersonAsync(
                    request.NationalCode,
                    request.FirstlName,
                    request.LastName,
                    request.BirthDate,
                    request.BirthPlace,
                    request.FatherName,
                    request.Gender,
                    phone, request.Address, email, mobile
                  , userContext.UserName
                    );
                #endregion

                #region ایجاد کارمند

                PhoneNumber? orgPhone = null;
                Email? orgEmail = null;
                PhoneNumber? orgMobile = null;
                try { orgPhone = request.OfficePhone != null ? PhoneNumber.Create(request.OfficePhone) : null; } catch { }
                try { orgEmail = request.OrgEmail != null ? Email.Create(request.OrgEmail) : null; } catch { }
                try { orgMobile = request.OrgMobile != null ? PhoneNumber.Create(request.OrgMobile) : null; } catch { }



                Guid employmentId = await _employmentService.CreateEmploymentAsync(
                    request.EmploymentCode, personId, request.EmploymentTypeId, request.EmploymentStatusId, request.StartDate, request.EndDate);
                #endregion
                

                #region انتصاب مکان ها به شخص
                if (request.locationsId != null && request.locationsId.Count() > 0)
                {
                    await _employmentService.AssignLocationsToEmployment(employmentId, request.locationsId);
                }
                #endregion

                #region انتصاب شخص به پست سازمانی

                Guid AssignId = await _orgChartService.AssignToEmploymentAsync(request.PostId, employmentId, request.AssigneeType, request.EffectiveFrom, request.EffectiveTo);

                #endregion

                #region ذخیره تغییرات
                await _personService.SaveAsync();
                await _employmentService.SaveAsync();
                await _orgChartService.SaveAsync();
                #endregion
                _logger.LogInformation(
                    "Employment created successfully: {employmentId} ({EmploymentCode})",
                    employmentId, request.EmploymentCode);

                return Result<Guid>.Ok(employmentId);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to create Post: {EmploymentCode}",
                     request.EmploymentCode);

                return Result<Guid>.Fail(ex.Message);
            }
        }
    }

}
