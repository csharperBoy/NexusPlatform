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

namespace HR.Application.Commands.OrgChart
{
    public record BatchUpdatePostsCommand(List<UpdatePostCommand> Posts) : IRequest<Result<List<Guid>>>;

    
    public class BatchUpdatePostsCommandHandler : IRequestHandler<BatchUpdatePostsCommand, Result<List<Guid>>>
    {
        private readonly IPostInternalService _orgChartService;
        private readonly ILogger<BatchUpdatePostsCommandHandler> _logger;

        public BatchUpdatePostsCommandHandler(IPostInternalService orgChartService, ILogger<BatchUpdatePostsCommandHandler> logger)
        {
            _orgChartService = orgChartService;
            _logger = logger;
        }

        public async Task<Result<List<Guid>>> Handle(BatchUpdatePostsCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var results = new List<Guid>();
                foreach (var command in request.Posts)
                {
                    // ۱. به‌روزرسانی اطلاعات پایه پست
                    Guid postId = await _orgChartService.UpdatePostAsync(
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

                    // ۲. تخصیص به کارمند (در صورت وجود)
                    if (command.EmploymentId.IsSet && command.EmploymentId.Value != Guid.Empty)
                    {
                        await _orgChartService.AssignToPostAsync( postId , new List<Guid?> { command.EmploymentId.Value }, command.AssignType.Value);
                    }
                    if (command.locationsId.IsSet)
                    {
                        await _orgChartService.AssignLocationsToPost(postId, command.locationsId.Value);
                    }
                    results.Add(postId);
                }

                // ۳. ذخیره‌سازی یکباره همه تغییرات
                await _orgChartService.SaveAsync();

                _logger.LogInformation("Batch update of {count} posts completed successfully.", results.Count);
                return Result<List<Guid>>.Ok(results);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to batch update posts.");
                return Result<List<Guid>>.Fail(ex.Message);
            }
        }
    }
}
