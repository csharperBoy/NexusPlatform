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
    public record BatchUpdateEmploymentsCommand(List<UpdateEmploymentCommand> Employments) : IRequest<Result<List<Guid>>>;


    public class BatchUpdateEmploymentsCommandHandler : IRequestHandler<BatchUpdateEmploymentsCommand, Result<List<Guid>>>
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

        public async Task<Result<List<Guid>>> Handle(BatchUpdateEmploymentsCommand request, CancellationToken cancellationToken)
        {
            
            try
            {
                var results = new List<Guid>();
                foreach (var command in request.Employments)
                {
                    // ۱. به‌روزرسانی اطلاعات پایه پست
                    Guid EmploymentId = await _employmentService.UpdateEmploymentAsync(
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
                    if (command.PostId.IsSet)
                    {
                        Guid assignId = await _orgChartService.AssignToEmploymentAsync(new List<Guid?> { command.PostId.Value }, EmploymentId, command.AssigneeType.Value, command.EffectiveFrom.Value, command.EffectiveTo.Value);
                    }
                    if (command.locationsId.IsSet)
                    {
                        await _employmentService.AssignLocationsToEmployment(EmploymentId, command.locationsId.Value);
                    }

                    results.Add(EmploymentId);
                }

                // ۳. ذخیره‌سازی یکباره همه تغییرات
                await _orgChartService.SaveAsync();
                await _employmentService.SaveAsync();

                _logger.LogInformation("Batch update of {count} Employment completed successfully.", results.Count);
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
