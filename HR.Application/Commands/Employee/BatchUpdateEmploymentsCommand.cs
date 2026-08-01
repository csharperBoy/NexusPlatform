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

namespace HR.Application.Commands.Employee
{
    public record BatchUpdateEmploymentsCommand(List<UpdateEmploymentCommand> Employments) : IRequest<Result<List<Guid>>>;


    public class BatchUpdateEmploymentsCommandHandler : IRequestHandler<BatchUpdateEmploymentsCommand, Result<List<Guid>>>
    {
        private readonly IEmployeeInternalService _employmentService;
        private readonly IOrgChartInternalService _orgChartService;
        private readonly ILogger<BatchUpdateEmploymentsCommandHandler> _logger;

        public BatchUpdateEmploymentsCommandHandler(IOrgChartInternalService orgChartService, ILogger<BatchUpdateEmploymentsCommandHandler> logger, IEmployeeInternalService employmentService)
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
                   command.FirstlName,
                   command.LastName,
                   command.BirthDate,
                   command.BirthPlace,
                   command.FatherName,
                   command.EmployeeCode,
                   command.EmploymentTypeId,
                   command.EmploymentStatusId,
                   command.StartDate,
                   command.EndDate,
                   command.locationsId,
                   command.OfficePhone,
                   command.OrgEmail,
                   command.OrgMobile
                   );
                    if (command.PostId != Guid.Empty && command.PostId != null)
                    {
                        Guid assignId = await _orgChartService.AssignToEmployeeAsync((Guid)command.PostId, EmploymentId, command.AssigneeType, command.EffectiveFrom, command.EffectiveTo);
                    }
                    if (command.locationsId != null)
                    {
                        await _employmentService.AssignLocationsToEmployee(EmploymentId, command.locationsId);
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
