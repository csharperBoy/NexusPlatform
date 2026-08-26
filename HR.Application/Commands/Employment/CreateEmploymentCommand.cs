using Core.Application.Abstractions.People;
using Core.Application.Context;
using Core.Application.Provider;
using Core.Domain.ValueObjects;
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

namespace HR.Application.Commands.Employment
{
    public record CreateEmploymentCommand(
    #region party
        List<string>? Phone,
        List<string>? Address,
        List<string>? Email,
        List<string>? Mobile,
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


    public class CreateEmploymentCommandHandler : IRequestHandler<CreateEmploymentCommand, Result<Guid>>
    {
        private readonly IPostInternalService _orgChartService;
        private readonly IPersonPublicService _personService;
        private readonly IEmploymentInternalService _employmentService;
        private readonly ILogger<CreateEmploymentCommandHandler> _logger;
        private readonly IUserDataContextProvider _userProvider;
        public CreateEmploymentCommandHandler(
            IPostInternalService orgChartService,
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

               List< PhoneNumber>? phone = null;
               List< Email>? email = null;
               List< PhoneNumber>? mobile = null;
                try { phone.AddRange( request.Phone != null ? request.Phone.Select(a=> PhoneNumber.Create(a)).ToList() : null); } catch { }
                try { email.AddRange(  request.Email != null ? request.Email.Select(a => Email.Create(a)).ToList() : null); } catch { }
                try { mobile.AddRange( request.Mobile != null ? request.Mobile.Select(a => PhoneNumber.Create(a)).ToList() : null); } catch { }


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
                List<PhoneNumber>? orgPhone = null;
                List<Email>? orgEmail = null;
                List<PhoneNumber>? orgMobile = null;
                try { orgPhone.AddRange(request.OfficePhone != null ? request.OfficePhone.Select(a => PhoneNumber.Create(a)).ToList() : null); } catch { }
                try { orgEmail.AddRange(request.OrgEmail != null ? request.OrgEmail.Select(a => Email.Create(a)).ToList() : null); } catch { }
                try { orgMobile.AddRange(request.OrgMobile != null ? request.OrgMobile.Select(a => PhoneNumber.Create(a)).ToList() : null); } catch { }


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
