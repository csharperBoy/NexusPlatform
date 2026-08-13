using Core.Application.Abstractions.HR;
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
    public record UpdateEmploymentContactCommand(
        Guid Id,
        string? OfficePhone,
        string? OrgEmail,
        string? OrgMobile

) : IRequest<Result<Guid>>;


    public class UpdateEmploymentContactCommandHandler : IRequestHandler<UpdateEmploymentContactCommand, Result<Guid>>
    {
       private readonly ILogger<UpdateEmploymentContactCommandHandler> _logger;
        private readonly IEmploymentInternalService _employmentService;
        public UpdateEmploymentContactCommandHandler(IEmploymentInternalService employmentService,
            ILogger<UpdateEmploymentContactCommandHandler> logger)
        {
            _logger = logger;
            _employmentService = employmentService;
        }

        public async Task<Result<Guid>> Handle(UpdateEmploymentContactCommand request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation(
                    "Update Employment Contact:{Id}" , request.Id);
                Guid EmploymentId = await _employmentService.UpdateEmploymentAsync(
                    request.Id,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    request.OfficePhone,
                    request.OrgEmail,
                    request.OrgMobile
                    );
                
                await _employmentService.SaveAsync();
                _logger.LogInformation(
                    "Employment Contact Update successfully: {EmploymentId}",
                    EmploymentId);

                return Result<Guid>.Ok(EmploymentId);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to update contact of Employment: {request.Id}",
                     request.Id);

                return Result<Guid>.Fail(ex.Message);
            }
        }
    }
}
