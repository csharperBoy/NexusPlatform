using Core.Application.Helper;
using Core.Shared.Results;
using HR.Application.Commands.OrgChart;
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
    public record BatchUpdateEmploymentsCommand(List<UpdateEmploymentCommand> Employments)
        : IRequest<BatchResult>;


    public class BatchUpdateEmploymentsCommandHandler
        : IRequestHandler<BatchUpdateEmploymentsCommand, BatchResult>
    {
        private readonly IEmploymentInternalService _employmentService;
        private readonly IPostInternalService _orgChartService;
        private readonly ILogger<BatchUpdateEmploymentsCommandHandler> _logger;


        public BatchUpdateEmploymentsCommandHandler(IPostInternalService orgChartService, ILogger<BatchUpdateEmploymentsCommandHandler> logger, IEmploymentInternalService employmentService)
        {
            _orgChartService = orgChartService;
            _logger = logger;
            _employmentService = employmentService;
        }

        public async Task<BatchResult> Handle(BatchUpdateEmploymentsCommand request, CancellationToken cancellationToken)
        {

            var successMessages = new List<string>();
            var errors = new List<string>();

            try
            {
                foreach (var command in request.Employments)
                {
                    try
                    {
                        string successMessage = $"{IconInTextHelper.IconUpdate} برای کارمند با کد پرسنلی '{command.EmploymentCode}' ";

                        bool assignHasChange = false;
                        bool locHasChange = false;
                        // ۱. به‌روزرسانی اطلاعات پایه پست
                        bool hasChange = await _employmentService.UpdateEmploymentAsync(
                         command.Id,
                         command.Phone,
                         command.Address,
                         command.Email,
                         command.Mobile,
                         command.FirstName,
                         command.LastName,
                         command.BirthDate,
                         command.BirthPlace,
                         command.FatherName,
                         command.nationalCode,
                         command.EmploymentCode,
                         command.EmploymentTypeId,
                         command.EmploymentStatusId,
                         command.StartDate,
                         command.EndDate,
                         command.locationsId,
                         command.OfficePhone,
                         command.OrgEmail,
                         command.OrgMobile
                         );
                        Guid EmploymentId = command.Id;
                        if (hasChange)
                        {
                            successMessage = $"اطلاعات مربوط شخصی با موفقیت بروزرسانی شد.";
                        }
                        if (command.PostId.IsSet)
                        {
                            assignHasChange = await _orgChartService.AssignToEmploymentAsync(new List<Guid?> { command.PostId.Value }, EmploymentId, command.AssigneeType.Value, command.EffectiveFrom.Value, command.EffectiveTo.Value);
                            if (assignHasChange)
                            {
                                successMessage = $"{successMessage} * اطلاعات مربوط به انتصاب به پست با موفقیت بروزرسانی شد";
                            }
                        }
                        if (command.locationsId.IsSet)
                        {
                            locHasChange = await _employmentService.AssignLocationsToEmployment(EmploymentId, command.locationsId.Value);
                            if (locHasChange)
                            {
                                successMessage = $"{successMessage} * اطلاعات مربوط به محل استقرار با موفقیت بروزرسانی شد";
                            }
                        }
                        if (hasChange || assignHasChange || locHasChange)
                        {
                            successMessages.Add(successMessage);
                        }
                    }
                    catch (Exception ex)
                    {
                        errors.Add($"{IconInTextHelper.IconError} بروزرسانی اطلاعات کارمند با کد پرسنلی '{command.EmploymentCode}' با خطا مواجه شد!!!: {ex.Message}");
                    }
                }

                // ۳. ذخیره‌سازی یکباره همه تغییرات
                await _orgChartService.SaveAsync();
                await _employmentService.SaveAsync();

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
