using Core.Application.Abstractions.People;
using Core.Application.Context;
using Core.Application.Helper;
using Core.Application.Provider;
using Core.Domain.ValueObjects;
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
   
    public record BatchCreateEmploymentsCommand(List<CreateEmploymentCommand> Employments)
        : IRequest<BatchResult>;


    public class BatchCreateEmploymentsCommandHandler
        : IRequestHandler<BatchCreateEmploymentsCommand, BatchResult>
    {
        private readonly IEmploymentInternalService _employmentService;
        private readonly IPostInternalService _orgChartService;
        private readonly ILogger<BatchCreateEmploymentsCommandHandler> _logger;

        private readonly IPersonPublicService _personService;
        private readonly IUserDataContextProvider _userProvider;


        public BatchCreateEmploymentsCommandHandler(IPostInternalService orgChartService, 
            ILogger<BatchCreateEmploymentsCommandHandler> logger,
            IPersonPublicService personService,
            IEmploymentInternalService employmentService,
           IUserDataContextProvider userProvider
            )
        {
            _orgChartService = orgChartService;
            _logger = logger;
            _personService = personService;
            _employmentService = employmentService;
            _userProvider = userProvider;
        }

        public async Task<BatchResult> Handle(BatchCreateEmploymentsCommand request, CancellationToken cancellationToken)
        {

            var successMessages = new List<string>();
            var errors = new List<string>();

            try
            {
                foreach (var command in request.Employments)
                {
                    try
                    {
                        //string successMessage = $"{IconInTextHelper.IconUpdate} کارمند با کد پرسنلی '{command.EmploymentCode}' ";

                        UserDataContext userContext = await _userProvider.GetAsync(new CancellationToken());
                        #region ساخت شخصیت حقیقی

                        List<PhoneNumber> phone = new();
                        List<Email> email = new();
                        List<PhoneNumber> mobile = new();

                        phone.AddRange(command.Phone?.Select(s => PhoneNumber.TryCreate(s, out var p) ? p : null).Where(p => p != null) ?? Enumerable.Empty<PhoneNumber>());
                        email.AddRange(command.Email?.Select(s => Email.TryCreate(s, out var p) ? p : null).Where(p => p != null) ?? Enumerable.Empty<Email>());
                        mobile.AddRange(command.Mobile?.Select(s => PhoneNumber.TryCreate(s, out var p) ? p : null).Where(p => p != null) ?? Enumerable.Empty<PhoneNumber>());






                        Guid personId = await _personService.CreatePersonAsync(
                            command.NationalCode,
                            command.FirstName,
                            command.LastName,
                            command.BirthDate,
                            command.BirthPlace,
                            command.FatherName,
                            command.Gender,
                            phone, command.Address, email, mobile
                          , userContext.UserName
                            );
                        #endregion

                        #region ایجاد کارمند

                        List<PhoneNumber> orgPhone = new();
                        List<Email> orgEmail = new();
                        List<PhoneNumber> orgMobile = new();


                        orgPhone.AddRange(command.OfficePhone?.Select(s => PhoneNumber.TryCreate(s, out var p) ? p : null).Where(p => p != null) ?? Enumerable.Empty<PhoneNumber>());
                        orgEmail.AddRange(command.OrgEmail?.Select(s => Email.TryCreate(s, out var p) ? p : null).Where(p => p != null) ?? Enumerable.Empty<Email>());
                        orgMobile.AddRange(command.OrgMobile?.Select(s => PhoneNumber.TryCreate(s, out var p) ? p : null).Where(p => p != null) ?? Enumerable.Empty<PhoneNumber>());



                        Guid employmentId = await _employmentService.CreateEmploymentAsync(
                            command.EmploymentCode, personId, command.EmploymentTypeId, command.EmploymentStatusId, command.StartDate, command.EndDate);
                        #endregion


                        #region انتصاب مکان ها به شخص
                        if (command.locationsId != null && command.locationsId.Count() > 0)
                        {
                            await _employmentService.AssignLocationsToEmployment(employmentId, command.locationsId);
                        }
                        #endregion

                        #region انتصاب شخص به پست سازمانی
                        if (command.PostId != null)
                        {
                            await _orgChartService.AssignToEmploymentAsync(new List<Guid?> { command.PostId }, employmentId, command.AssigneeType, command.EffectiveFrom, command.EffectiveTo);
                        }
                        #endregion

                        // ۳. ذخیره‌سازی یکباره همه تغییرات


                        await _personService.SaveAsync();
                        await _employmentService.SaveAsync();
                        await _orgChartService.SaveAsync();
                        successMessages.Add($"{IconInTextHelper.IconUpdate} کارمند با کد پرسنلی '{command.EmploymentCode}' با موفقیت افزوده شد. ");
                    }
                    catch (Exception ex)
                    {
                        errors.Add($"{IconInTextHelper.IconError} افزودن کارمند با کد پرسنلی '{command.EmploymentCode}' با خطا مواجه شد!!!: {ex.Message}");
                    }
                }


                _logger.LogInformation(
               "Batch update completed. SuccessCount: {SuccessCount}, ErrorCount: {ErrorCount}",
               successMessages.Count, errors.Count);

                // ۳. ساخت نتیجه نهایی بر اساس وجود خطا یا عدم آن
                return new BatchResult(
                           succeeded: true,
                           successMessages: successMessages,
                           errors: errors  // اگر خطایی نبود، null بفرست
                       );
            }
            catch (Exception ex)
            {
                // خطای سطح کلی (مثلاً خطا در SaveAsync یا قطعی شبکه)
                _logger.LogError(ex, "Critical failure during batch update.");
                return BatchResult.Fail(ex.Message);
            }
        }
    }
}
