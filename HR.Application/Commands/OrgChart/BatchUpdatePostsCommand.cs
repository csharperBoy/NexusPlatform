using Core.Application.Helper;
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
    public record BatchUpdatePostsCommand(List<UpdatePostCommand> Posts) : IRequest<BatchResult>;


    public class BatchUpdatePostsCommandHandler : IRequestHandler<BatchUpdatePostsCommand, BatchResult>
    {
        private readonly IPostInternalService _orgChartService;
        private readonly ILogger<BatchUpdatePostsCommandHandler> _logger;

        public BatchUpdatePostsCommandHandler(IPostInternalService orgChartService, ILogger<BatchUpdatePostsCommandHandler> logger)
        {
            _orgChartService = orgChartService;
            _logger = logger;
        }

        public async Task<BatchResult> Handle(BatchUpdatePostsCommand request, CancellationToken cancellationToken)
        {
            var successMessages = new List<string>();
            var errors = new List<string>();

            try
            {

                foreach (var command in request.Posts)
                {
                    bool assignHasChange = false;
                    bool hasChange = false;
                    string jobTitleName = null;
                    bool locHasChange = false;

                    try
                    {


                        // ۱. به‌روزرسانی اطلاعات پایه پست
                        (hasChange, jobTitleName) = await _orgChartService.UpdatePostAsync(
                            command.Id,
                            command.Code,
                            command.OrganizationUnitId,
                            command.JobTitleId,
                            command.JobLevelId,
                            command.GradeId,
                            command.CostCenterId,
                            command.ReportsToPostId,
                            command.IsActive,
                            command.OfficePhone,
                            command.OrgEmail,
                            command.OrgMobile
                        );
                        string successMessage = $"{IconInTextHelper.IconUpdate} برای پست با عنوان شغلی '{jobTitleName}' و کد '{command.Code.Value}' ";

                        Guid postId = command.Id;
                        if (hasChange)
                        {
                            successMessage = $"اطلاعات پست با موفقیت بروزرسانی شد.";
                        }
                        // ۲. تخصیص به کارمند (در صورت وجود)
                        if (command.EmploymentId.IsSet)
                        {
                            assignHasChange = await _orgChartService.AssignToPostAsync(postId, new List<Guid?> { command.EmploymentId.Value }, command.AssignType.Value);
                            if (assignHasChange)
                            {
                                successMessage = $"{successMessage} * اطلاعات مربوط به انتصاب به کارمند با موفقیت بروزرسانی شد";
                            }
                        }
                        if (command.locationsId.IsSet)
                        {
                            locHasChange = await _orgChartService.AssignLocationsToPost(postId, command.locationsId.Value);
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
                        errors.Add($"{IconInTextHelper.IconError} بروزرسانی اطلاعات پست با عنوان شغلی '{jobTitleName}' با خطا مواجه شد!!!: {ex.Message}");
                    }
                }

                // ۳. ذخیره‌سازی یکباره همه تغییرات
                await _orgChartService.SaveAsync();

                _logger.LogInformation(
              "Batch update completed. SuccessCount: {SuccessCount}, ErrorCount: {ErrorCount}",
              successMessages.Count, errors.Count);

                // ۳. ساخت نتیجه نهایی بر اساس وجود خطا یا عدم آن
                return new BatchResult(
                           succeeded: true,
                           successMessages: successMessages,
                           errors: errors 
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
