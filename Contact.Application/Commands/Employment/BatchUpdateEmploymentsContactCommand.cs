using Core.Domain.Common;
using Core.Shared.Results;
using HR.Application.Commands.Employment;
using HR.Application.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contact.Application.Commands.Employment
{
    public record BatchUpdateEmploymentsContactCommand(List<UpdateEmploymentContactCommand> EmploymentsContact) : IRequest<Result<List<Guid>>>;


    public class BatchUpdateEmploymentsContactCommandHandler : IRequestHandler<BatchUpdateEmploymentsContactCommand, Result<List<Guid>>>
    {
        private readonly IEmploymentInternalService _employmentService;
        private readonly ILogger<BatchUpdateEmploymentsContactCommandHandler> _logger;

        public BatchUpdateEmploymentsContactCommandHandler( ILogger<BatchUpdateEmploymentsContactCommandHandler> logger, IEmploymentInternalService employmentService)
        {
            _logger = logger;
            _employmentService = employmentService;
        }

        public async Task<Result<List<Guid>>> Handle(BatchUpdateEmploymentsContactCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var results = new List<Guid>();
                foreach (var command in request.EmploymentsContact)
                {
                    // ۱. به‌روزرسانی اطلاعات پایه پست
                    await _employmentService.UpdateEmploymentAsync(
                   command.Id,
                    Optional<List<string>?>.Undefined,
                   Optional<List<string>?>.Undefined,
                   Optional<List<string>?>.Undefined,
                   Optional<List<string>?>.Undefined,
                   Optional<string?>.Undefined,
                   Optional<string?>.Undefined,
                   Optional<DateTime?>.Undefined,
                   Optional<string?>.Undefined,
                   Optional<string?>.Undefined,
                   Optional<string?>.Undefined,
                   Optional<string?>.Undefined,
                   Optional<Guid?>.Undefined,
                   Optional<Guid?>.Undefined,
                   Optional<DateOnly?>.Undefined,
                   Optional<DateOnly?>.Undefined,
                   Optional<List<Guid>?>.Undefined,
                   command.OfficePhones,
                   command.OrgEmails,
                   command.OrgMobiles
                   );

                    Guid EmploymentId = command.Id;
                    results.Add(EmploymentId);
                }

                // ۳. ذخیره‌سازی یکباره همه تغییرات
                await _employmentService.SaveAsync();

                _logger.LogInformation("Batch update of {count} Employment Contact completed successfully.", results.Count);
                return Result<List<Guid>>.Ok(results);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to batch update Employment Contact.");
                return Result<List<Guid>>.Fail(ex.Message);
            }
        }
    }
}
